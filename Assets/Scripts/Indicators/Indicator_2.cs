using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator_2 : Indicator 
{
  	ushort candlestickPeriod;

    CandlestickConsolidator candlestick;
    
    public const ushort kMaxConfigurationID = 3; 

//**************************************************************************************	

    public Indicator_2(Miner iMiner, ushort iPairID, ushort iConfigurationID) : base(iMiner, iPairID, iConfigurationID)
    {
        ParseConfiguration(iConfigurationID);

        candlestick = iMiner.GetCandlestickConsolidator(iPairID, candlestickPeriod);
        candlestick.AddListiner();   
    }    

//************************************************************************************** 

    protected override float ShouldBuy(ulong iTimeNow)
    {
        LHA lha = candlestick.data.GetLHA();

        if(lha==null)
            return 0;

        if(lha.value >= 0)
            return 0;
        else
            return Mathf.Lerp(0.5f, 1.0f, -lha.value);
    }

//**************************************************************************************

    protected override float ShouldSell(ulong iTimeNow)
    {
        LHA lha = candlestick.data.GetLHA();

        if(lha==null)
            return 0;

        if(lha.value <= 0)
            return 0;
        else
            return Mathf.Lerp(0.5f, 1.0f, lha.value);
    }

//**************************************************************************************  

    public override float GetDecision(ulong iTimeNow)
    {
        return ShouldBuy(iTimeNow) - ShouldSell(iTimeNow);
    }

//**************************************************************************************    

    public override ushort GetIndicatorID()
    {
        return 2;
    }

//**************************************************************************************

    void ParseConfiguration(ushort iConfigurationID)
    {
        if(iConfigurationID > kMaxConfigurationID)
            throw new System.ArgumentException("Provided value exceeds max configuration ID: " + kMaxConfigurationID.ToString(), "iConfigurationID: " + iConfigurationID.ToString());

        switch(iConfigurationID)
        {
            case 0:
                candlestickPeriod = 96;
                break;

            case 1:
                candlestickPeriod = 144;  
                break;  

            case 2:
                candlestickPeriod = 216;  
                break;   

            case 3:
                candlestickPeriod = 336;  
                break;            
        }
      }

//**************************************************************************************
}
