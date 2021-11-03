using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator_1 : Indicator 
{
  	ushort candlestickPeriod;

    CandlestickConsolidator candlestick;
    
    public const ushort kMaxConfigurationID = 3; 

//**************************************************************************************	

    public Indicator_1(Miner iMiner, ushort iPairID, ushort iConfigurationID) : base(iMiner, iPairID, iConfigurationID)
    {
        ParseConfiguration(iConfigurationID);

        candlestick = iMiner.GetCandlestickConsolidator(iPairID, candlestickPeriod);
        candlestick.AddListiner();   
    }    

//************************************************************************************** 

    protected override float ShouldBuy(ulong iTimeNow)
    {
        float decision = GetDecision(iTimeNow);
        if(decision >= 0)
            return 0;
        else
            return -decision;
    }

//**************************************************************************************

    protected override float ShouldSell(ulong iTimeNow)
    {
        float decision = GetDecision(iTimeNow);

        if(decision <= 0)
            return 0;
        else
            return decision;
    }

//**************************************************************************************  

    public override float GetDecision(ulong iTimeNow)
    {
        RSI rsi = candlestick.data.GetRSI();

        if(rsi==null)
            return 0;

        return (rsi.last/100.0f)*2.0f - 1.0f;
    }

//**************************************************************************************    

    public override ushort GetIndicatorID()
    {
        return 1;
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
