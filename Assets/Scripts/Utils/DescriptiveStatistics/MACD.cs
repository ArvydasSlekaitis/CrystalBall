using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MACD 
{
	public float macd  { get; }
	public float signal { get; }
	public float hist { get; }

//**************************************************************************************

	public MACD(float[] iNumbers, int iSlowEMA, int iFastEMA, int iSignalEMA)
	{
		if(iFastEMA > iSlowEMA)
			throw new System.ArgumentException("iFastEMA should be smaller than iSlowEMA");

		EMA slowEMA = new EMA(iNumbers, iSlowEMA);
        EMA fastEMA = new EMA(iNumbers, iFastEMA);

		float[] signalEMAData = new float[iNumbers.Length];
		
		for(int i=0; i<signalEMAData.Length; i++)
			signalEMAData[i] = fastEMA.ema[i] - slowEMA.ema[i];

    	EMA signalEMA = new EMA(signalEMAData, iSignalEMA);

		macd = fastEMA.last - slowEMA.last;
		signal = signalEMA.last;
		hist = macd - signal;
	}

//**************************************************************************************	

}
