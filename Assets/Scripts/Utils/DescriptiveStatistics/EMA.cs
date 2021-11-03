using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMA 
{
	public float[] ema { get; }
	public float last { get; }

//**************************************************************************************

	// EMA - Exponential moving average. iNumbers[0] - oldest entry
	public EMA(float[] iNumbers, int iNumberOfPeriods)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return;

		float k = 2.0f/(iNumberOfPeriods+1.0f);
		ema = new float[iNumbers.Length];
		ema[0] = iNumbers[0];

		for(int i=1; i<iNumbers.Length; i++)
			ema[i] = iNumbers[i] * k + ema[i - 1] * (1 - k);

		last = ema[ema.Length-1];	
	}

//**************************************************************************************	
}
