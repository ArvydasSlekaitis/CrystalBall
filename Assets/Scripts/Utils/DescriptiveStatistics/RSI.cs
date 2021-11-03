using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RSI 
{
	public float[] rsi { get; }
	public float last { get; }

//**************************************************************************************

	public RSI(float[] iPrice, int iNumberOfPeriods)
	{
		if (iPrice == null || iPrice.Length < 1)
			return;

		rsi = new float[iPrice.Length];

		float gain = 0.0f;
		float loss = 0.0f;

		// first RSI value
		rsi[0] = 0.0f;
		for (int i = 1; i <= iNumberOfPeriods; ++i)
		{
			var diff = iPrice[i] - iPrice[i - 1];
			if (diff >= 0)
			{
				gain += diff;
			}
			else
			{
				loss -= diff;
			}
		}

		float avrg = gain / iNumberOfPeriods;
		float avrl = loss / iNumberOfPeriods;
		float rs = gain / loss;
		rsi[iNumberOfPeriods] = 100 - (100 / (1 + rs));

		for (int i = iNumberOfPeriods + 1; i < iPrice.Length; ++i)
		{
			float diff = iPrice[i] - iPrice[i - 1];

			if (diff >= 0)
			{
				avrg = ((avrg * (iNumberOfPeriods - 1)) + diff) / iNumberOfPeriods;
				avrl = (avrl * (iNumberOfPeriods - 1)) / iNumberOfPeriods;
			}
			else
			{
				avrl = ((avrl * (iNumberOfPeriods - 1)) - diff) / iNumberOfPeriods;
				avrg = (avrg * (iNumberOfPeriods - 1)) / iNumberOfPeriods;
			}

			rs = avrg / avrl;

			rsi[i] = 100 - (100 / (1 + rs));
		}

		last = rsi[rsi.Length-1];	
	}

//**************************************************************************************	
}
