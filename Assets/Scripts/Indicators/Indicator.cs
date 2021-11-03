using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public abstract class Indicator 
{
    protected enum State {LookingToBuy, LookingToSell}

    static List<ushort> maxConfigurationID;
    static List<Dictionary<ushort, ushort>> highestConfigurationID;

    State state;
    ushort pairID;
    ushort configurationID;
    float lifetimePerformance;
    float productOfPerformance;

    OrderBookDataStore orderBook;
    List<Trade> trades;

    protected abstract float ShouldBuy(ulong iTimeNow);
    protected abstract float ShouldSell(ulong iTimeNow);
    public abstract float GetDecision(ulong iTimeNow);
    public abstract ushort GetIndicatorID();

    public const ushort kMaxIndicatorID = 3;
 
//**************************************************************************************

    public Indicator(Miner iMiner, ushort iPairID, ushort iConfigurationID) 
    {
        pairID = iPairID;
        configurationID = iConfigurationID;
        trades = new List<Trade>();

       LoadTradesFromBinary();

        if(trades.Count == 0 || trades[trades.Count-1].sellTime != 0)
            state = State.LookingToBuy;
        else
            state = State.LookingToSell;

        orderBook = iMiner.GetOrderBook(iPairID);   

        UpdateHighestConfigurationID(GetIndicatorID(), pairID, configurationID);
    }        

//**************************************************************************************

    public bool Execute(ulong iTimeNow) 
    {      
        if(trades.Count >= 1000 && lifetimePerformance < 1.0f)
            return false;

        if(GetBestPrice(state) <= 0.0f)
            return false;

        switch(state)
        {
            case State.LookingToBuy:
                if(ShouldBuy(iTimeNow) >= 0.5f)
                {
                    Buy(iTimeNow);
                    return true; 
                }  
                break;

            case State.LookingToSell:
                if(ShouldSell(iTimeNow) >= 0.5f)
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
        float price = GetBestPrice(state);

        if(trades.Count>0 && trades[trades.Count-1].sellTime==0)
            throw new System.Exception("Bot is buying before selling. PairID:" + pairID);

        trades.Add(new Trade(price, 0, iTimeNow, 0));

        state = State.LookingToSell;
    }

//**************************************************************************************

    void Sell(ulong iTimeNow)
    {
        float price = GetBestPrice(state);
        
        if(trades[trades.Count-1].sellTime!=0)
            throw new System.Exception("Bot is seeling before buying. PairID:" + pairID);

        trades[trades.Count-1].sellPrice = price;
        trades[trades.Count-1].sellTime = iTimeNow;

        if(trades.Count > 1000)
            trades.RemoveAt(0);

        RecalculateLifetimePerformance();      
        state = State.LookingToBuy;
    }

//**************************************************************************************

	public void SaveTradesToBinary()
	{
        string path = Path.Combine(Application.persistentDataPath, "DataStore");
        path = Path.Combine(path, Pair.ToString(pairID));
        Trade.SaveToBinary(path, "Indicator_" + GetIndicatorID() + "_" + configurationID + ".dat", trades);
	}

//**************************************************************************************

	void LoadTradesFromBinary()
	{
        string path = Path.Combine(Application.persistentDataPath, "DataStore");
        path = Path.Combine(path, Pair.ToString(pairID));
        trades = Trade.LoadFromBinary(path, "Indicator_" + GetIndicatorID() + "_" + configurationID + ".dat");
	}

//**************************************************************************************

    public void SaveToCSV(StreamWriter iWriter)
	{
        iWriter.WriteLine(Pair.ToString(pairID) + "," + GetIndicatorID().ToString() + "," + configurationID + "," + trades.Count + "," + productOfPerformance + "," + lifetimePerformance);
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

    static void CreateConfiguration()
    {
        if(maxConfigurationID == null)
        {
            maxConfigurationID = new List<ushort>();

            maxConfigurationID.Add(Indicator_0.kMaxConfigurationID);
            maxConfigurationID.Add(Indicator_1.kMaxConfigurationID);
            maxConfigurationID.Add(Indicator_2.kMaxConfigurationID);
            maxConfigurationID.Add(Indicator_3.kMaxConfigurationID);
        }

        if(highestConfigurationID == null)
        {
            highestConfigurationID = new List<Dictionary<ushort, ushort>>();

            for(int i=0; i<maxConfigurationID.Count; i++)
                highestConfigurationID.Add(new Dictionary<ushort, ushort>());
        }
    }

//**************************************************************************************  

    public static ushort GetMaxIndicatorID()
    {
        CreateConfiguration();

        return (ushort)(maxConfigurationID.Count-1);
    }

//**************************************************************************************  

    public static ushort GetMaxConfigurationID(ushort iBotID)
    {
        CreateConfiguration();

        return maxConfigurationID[iBotID];
    }

//**************************************************************************************  

    public static ushort GetNextConfigurationID(ushort iBotID, ushort iPairID)
    {
        CreateConfiguration();

        if(highestConfigurationID[iBotID].ContainsKey(iPairID))
        {
            ushort id = highestConfigurationID[iBotID][iPairID];
            id++;
            return id;
        }
        else
            return 0;
     }

//**************************************************************************************  

    static void UpdateHighestConfigurationID(ushort iBotID, ushort iPairID, ushort iConfigurationID)
    {
        CreateConfiguration();

        if(!highestConfigurationID[iBotID].ContainsKey(iPairID))
            highestConfigurationID[iBotID].Add(iPairID, iConfigurationID);
        else if(iConfigurationID>highestConfigurationID[iBotID][iPairID])
            highestConfigurationID[iBotID][iPairID] = iConfigurationID;
    }

//**************************************************************************************  

    protected float GetBestPrice(State iState)
    {
        switch(iState)
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
