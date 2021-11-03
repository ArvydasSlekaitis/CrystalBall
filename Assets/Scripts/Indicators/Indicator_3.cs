using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator_3 : Indicator 
{
  	ushort candlestickPeriod;

    CandlestickConsolidator candlestick;
    
    public const ushort kMaxConfigurationID = 2; 

//**************************************************************************************	

    public Indicator_3(Miner iMiner, ushort iPairID, ushort iConfigurationID) : base(iMiner, iPairID, iConfigurationID)
    {
        ParseConfiguration(iConfigurationID);

        candlestick = iMiner.GetCandlestickConsolidator(iPairID, candlestickPeriod);
        candlestick.AddListiner();   
    }    

//************************************************************************************** 

    protected override float ShouldBuy(ulong iTimeNow)
    {
        MACD macd = candlestick.data.GetMACD();

        if(macd==null)
            return 0;

        return macd.macd < macd.signal ? 1.0f : 0.0f;
    }

//**************************************************************************************

    protected override float ShouldSell(ulong iTimeNow)
    {
        MACD macd = candlestick.data.GetMACD();

        if(macd==null)
            return 0;

        return macd.macd > macd.signal ? 1.0f : 0.0f;
    }

//**************************************************************************************  

    public override float GetDecision(ulong iTimeNow)
    {
        return ShouldBuy(iTimeNow) - ShouldSell(iTimeNow);
    }

//**************************************************************************************    

    public override ushort GetIndicatorID()
    {
        return 3;
    }

//**************************************************************************************

    void ParseConfiguration(ushort iConfigurationID)
    {
        if(iConfigurationID > kMaxConfigurationID)
            throw new System.ArgumentException("Provided value exceeds max configuration ID: " + kMaxConfigurationID.ToString(), "iConfigurationID: " + iConfigurationID.ToString());

        switch(iConfigurationID)
        {
            case 0:
                candlestickPeriod = 12;
                break;

            case 1:
                candlestickPeriod = 24;  
                break;  

            case 2:
                candlestickPeriod = 48;  
                break;   

            case 3:
                candlestickPeriod = 336;  
                break;            
        }
      }

//**************************************************************************************
}
