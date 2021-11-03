using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Bot 
{
    protected enum State {LookingToBuy, LookingToSell}
    PortfolioManager portfolioManager;

    List<Indicator> indicators;

    State state;
    ushort pairID;
    float lifetimePerformance;
    float productOfPerformance;

    List<Trade> trades;
    OrderBookDataStore orderBook;
 
//**************************************************************************************

    public Bot(Miner iMiner, PortfolioManager iPortfolioManager, ushort iPairID) 
    {
		if(iPortfolioManager == null)
			throw new System.ArgumentNullException("Parameter cannot be NULL", "iPortfolioManager");

        trades = new List<Trade>();
        pairID = iPairID;
        portfolioManager = iPortfolioManager;

        orderBook = iMiner.GetOrderBook(iPairID);

        LoadTradesFromBinary();

        if(trades.Count == 0 || trades[trades.Count-1].sellTime != 0)
            state = State.LookingToBuy;
        else
            state = State.LookingToSell;

        indicators = new List<Indicator>();    
		SpawnIndicators(iMiner);

        RecalculateLifetimePerformance();
    }        

//**************************************************************************************

    public virtual bool Execute(ulong iTimeNow) 
    { 
        for(int i=0; i<indicators.Count; i++)
            indicators[i].Execute(iTimeNow);

        if(GetBestPrice() <= 0.0f)
            return false;
    
        switch(state)
        {
            case State.LookingToBuy:
                if(ShouldBuy(iTimeNow))
                {
                    Buy(iTimeNow);
                    return true; 
                }  
                break;

            case State.LookingToSell:
                if(ShouldSell(iTimeNow))
                {
                    Sell(iTimeNow);
                    return true;
                }
                break;  
        }  

        return false;
    }

//**************************************************************************************

    void Buy(ulong iTimeNow)
    {
        Debug.Log("Bot Buy decision for " + Pair.ToString(pairID));
        for(int i=0; i<indicators.Count; i++)
        {
            float decision = indicators[i].GetDecision(iTimeNow);           
            float weight = decision == 0.0f ? 0.0f : indicators[i].GetLifetimePerformance();
            Debug.Log("Indicator " + i + " decision:" + decision + " weight:" + weight);
        }

        float price = GetBestPrice();

        if(trades.Count>0 && trades[trades.Count-1].sellTime==0)
            throw new System.Exception("Bot is buying before selling. PairID:" + pairID);

        trades.Add(new Trade(price, 0, iTimeNow, 0));
        portfolioManager.PlaceBuyOrder(this, price);
        state = State.LookingToSell;
    }

//**************************************************************************************

    void Sell(ulong iTimeNow)
    {
        Debug.Log("Bot Sell decision for " + Pair.ToString(pairID));
        for(int i=0; i<indicators.Count; i++)
        {
            float decision = indicators[i].GetDecision(iTimeNow);           
            float weight = decision == 0.0f ? 0.0f : indicators[i].GetLifetimePerformance();
            Debug.Log("Indicator " + i + " decision:" + decision + " weight:" + weight);
        }

        float price = GetBestPrice();

        if(trades[trades.Count-1].sellTime!=0)
            throw new System.Exception("Bot is seeling before buying. PairID:" + pairID);

        trades[trades.Count-1].sellPrice = price;
        trades[trades.Count-1].sellTime = iTimeNow;

        portfolioManager.PlaceSellOrder(this, price);
        state = State.LookingToBuy;     
    
        RecalculateLifetimePerformance();
    }

//**************************************************************************************

    public void SaveToCSV(StreamWriter iWriter)
	{
        iWriter.WriteLine(Pair.ToString(pairID) + "," + trades.Count + "," + productOfPerformance + "," + lifetimePerformance);
	}

//**************************************************************************************

    public void SaveIndicatorsToCSV(StreamWriter iWriter)
	{
        for(int i=0; i<indicators.Count; i++)
            indicators[i].SaveToCSV(iWriter);
    }

//************************************************************************************** 

    public float GetLifetimePerformance()
    {
        return lifetimePerformance;
    }

//**************************************************************************************  

    public ushort GetPairID()
    {
        return pairID;
    }

//**************************************************************************************  

    public string GetCurrentAsset()
    {
        switch (state)
        {
            case State.LookingToBuy:
                return Pair.ToSourceAsset(pairID);

            case State.LookingToSell:
                return Pair.ToDestinationAsset(pairID);

            default:
                return null;
        }
    }

//**************************************************************************************  

    protected float GetBestPrice()
    {
        switch(state)
        {
            case State.LookingToBuy:
                if(orderBook.asks == null || orderBook.asks.Count <= 0)
                    return -1;
                return orderBook.asks[0].price;

            case State.LookingToSell:
                if(orderBook.bids == null || orderBook.bids.Count <= 0)
                    return -1;
                return orderBook.bids[0].price;

            default:
                return -1;
        }
    }

//**************************************************************************************    

    virtual public void Save()
    {
        SaveTradesToBinary();

        for(int i=0; i<indicators.Count; i++)
            indicators[i].SaveTradesToBinary();
    }

//**************************************************************************************   

    protected bool ShouldBuy(ulong iTimeNow)
    {
        return GetDecision(iTimeNow) <= -0.5f;
    }

//**************************************************************************************

    protected bool ShouldSell(ulong iTimeNow)
    {
        return GetDecision(iTimeNow) >= 0.5f;
    }

//**************************************************************************************    

    // Buy [-1.0, -0.5]
    // Sell [0.5, 1.0]  
    float GetDecision(ulong iTimeNow)
    {
        float[] decisions = new float[indicators.Count];
        float[] weights = new float[indicators.Count];

        for(int i=0; i<indicators.Count; i++)
        {
            decisions[i] = indicators[i].GetDecision(iTimeNow);           
            weights[i] = decisions[i] == 0.0f ? 0.0f : indicators[i].GetLifetimePerformance();
        }

        float sumOfWeights = SharedFunctions.Sum(weights);

        if(sumOfWeights <= 0.0f)
            return 0.0f;

  		for (int i = 0; i < weights.Length; i++)
			weights[i] /= sumOfWeights;

		return SharedFunctions.CalculateWeightedArithmeticMean(decisions, weights);
    }

//**************************************************************************************

	void SpawnIndicators(Miner iMiner)
	{
        for(ushort indicatorID=0; indicatorID<=Indicator.GetMaxIndicatorID(); indicatorID++)
            for(ushort configurationID=Indicator.GetNextConfigurationID(indicatorID, GetPairID()); configurationID<=Indicator.GetMaxConfigurationID(indicatorID); configurationID++)
            {
                try
                {
                    Indicator b = CreateIndicator(iMiner, indicatorID, GetPairID(), configurationID);
                    if(b!=null)
                        indicators.Add(b);
                }
                catch (System.Exception ex) { Debug.Log("Could not create new Indicator_" + indicatorID.ToString() + ":" + ex.ToString()); }               
            }
	}

//**************************************************************************************

	Indicator CreateIndicator(Miner iMiner, ushort iIndicatorID, ushort iPairID, ushort iConfigurationID, ushort iState=0, uint iLifetimeTicks=0, float iAcquisitionPrice=0.0f)
	{
		if(iIndicatorID > Indicator.kMaxIndicatorID)
			throw new System.ArgumentException("Parameter must be less or equal to Indicator.kMaxIndicatorID:" + Indicator.kMaxIndicatorID, "iIndicatorID:" + iIndicatorID);

		if(iPairID > Pair.GetMaxID())
			throw new System.ArgumentException("Parameter must be less or equal to Pair.GetMaxID():" + Pair.GetMaxID(), "iBotID:" + iPairID);

		if(!Pair.IsEnabled(iPairID))
			return null;
			
		switch(iIndicatorID)
		{
			case 0: return new Indicator_0(iMiner, iPairID, iConfigurationID);	
            case 1: return new Indicator_1(iMiner, iPairID, iConfigurationID);	
            case 2: return new Indicator_2(iMiner, iPairID, iConfigurationID);	
            case 3: return new Indicator_3(iMiner, iPairID, iConfigurationID);	
		}	

		return null;
	}

//**************************************************************************************

	void LoadTradesFromBinary()
	{
        string path = Path.Combine(Application.persistentDataPath, "DataStore");
        path = Path.Combine(path, Pair.ToString(pairID));

        trades = Trade.LoadFromBinary(path, "BotTrades.dat");
	}

//**************************************************************************************

	public void SaveTradesToBinary()
	{
        string path = Path.Combine(Application.persistentDataPath, "DataStore");
        path = Path.Combine(path, Pair.ToString(pairID));

        Trade.SaveToBinary(path, "BotTrades.dat", trades);
	}

//**************************************************************************************       

    void RecalculateLifetimePerformance()
    {
        if(trades.Count<1 || trades[0].sellTime == 0)
        {
            lifetimePerformance = 0;
            productOfPerformance = 0;
            return;
        }

        float[] performances = new float[trades.Count];
        for(int i=0; i<performances.Length; i++)
            if(trades[i].sellTime != 0)
                performances[i] = FinanceFunctions.GetSellPerformance(trades[i].buyPrice, trades[i].sellPrice);

        productOfPerformance = SharedFunctions.CalculateGeometricMean(performances);
        uint lifetimeTicks = (uint)(((float)trades[performances.Length-1].sellTime - (float)trades[0].buyTime)/15.0f);

        lifetimePerformance = FinanceFunctions.GetLifetimePerformance((uint)performances.Length, lifetimeTicks, productOfPerformance, performances[performances.Length-1]);
    }

//**************************************************************************************

}
