using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator_0 : Indicator 
{
  	ushort candlestickPeriod;

    CandlestickConsolidator candlestick;
    
    public const ushort kMaxConfigurationID = 3; 

//**************************************************************************************	

    public Indicator_0(Miner iMiner, ushort iPairID, ushort iConfigurationID) : base(iMiner, iPairID, iConfigurationID)
    {
        ParseConfiguration(iConfigurationID);

        candlestick = iMiner.GetCandlestickConsolidator(iPairID, candlestickPeriod);
        candlestick.AddListiner(); 
    }    

//************************************************************************************** 

    protected override float ShouldBuy(ulong iTimeNow)
    {
        if(!candlestick.IsValid(iTimeNow))
            return 0;

        // Mean price
        float meanPrice = candlestick.data.averagePrice.GetArithmeticMean().mean;
        float meanStdDev = candlestick.data.averagePrice.GetArithmeticMean().standardDeviation;
        if(meanPrice <= 0 || meanStdDev <= 0)
            return 0;

        float bestPrice = GetBestPrice(State.LookingToBuy);
        if(bestPrice <= 0.0f)
            return 0;

        if(meanStdDev < bestPrice*FinanceFunctions.kMinTreshold)
            return 0;

        float meanMultiplier = (Mathf.Clamp(bestPrice*(1.0f+FinanceFunctions.kMinTreshold), meanPrice-2.0f*meanStdDev, meanPrice+2.0f*meanStdDev) - (meanPrice - 2.0f*meanStdDev))/(4.0f*meanStdDev);   
        float meanScaled = (meanMultiplier*2.0f-1.0f);
       
        if(meanScaled >= 0)
            return 0;
        else
            return -meanScaled;
    }

//**************************************************************************************

    protected override float ShouldSell(ulong iTimeNow)
    {
        if(!candlestick.IsValid(iTimeNow))
            return 0;

        // Mean price
        float meanPrice = candlestick.data.averagePrice.GetArithmeticMean().mean;
        float meanStdDev = candlestick.data.averagePrice.GetArithmeticMean().standardDeviation;
        if(meanPrice <= 0 || meanStdDev <= 0)
            return 0;

        float bestPrice = GetBestPrice(State.LookingToSell);
        if(bestPrice <= 0.0f)
            return 0;

        float meanMultiplier = (Mathf.Clamp(bestPrice*(1.0f-FinanceFunctions.kMinTreshold), meanPrice-2.0f*meanStdDev, meanPrice+2.0f*meanStdDev) - (meanPrice - 2.0f*meanStdDev))/(4.0f*meanStdDev);   
        float meanScaled = (meanMultiplier*2.0f-1.0f);
       
        if(meanScaled <= 0)
            return 0;
        else
            return meanScaled;
     }

//**************************************************************************************  

    public override float GetDecision(ulong iTimeNow)
    {
        return ShouldBuy(iTimeNow) - ShouldSell(iTimeNow);
    }

//**************************************************************************************    

    public override ushort GetIndicatorID()
    {
        return 0;
    }

//**************************************************************************************

    void ParseConfiguration(ushort iConfigurationID)
    {
        if(iConfigurationID > kMaxConfigurationID)
            throw new System.ArgumentException("Provided value exceeds max configuration ID: " + kMaxConfigurationID.ToString(), "iConfigurationID: " + iConfigurationID.ToString());

        switch(iConfigurationID)
        {
            case 0:
                candlestickPeriod = 24;
                break;

            case 1:
                candlestickPeriod = 48;  
                break;  

            case 2:
                candlestickPeriod = 96;  
                break;   

            case 3:
                candlestickPeriod = 192;  
                break;            
        }
      }

//**************************************************************************************     
}
