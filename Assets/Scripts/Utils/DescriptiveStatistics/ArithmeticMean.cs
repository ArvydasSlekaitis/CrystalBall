using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArithmeticMean 
{
	public float mean { get; }
	public float standardDeviation { get; }

//**************************************************************************************

	public ArithmeticMean(float[] iRawData)
	{
		// Arithmetic mean		
		mean = SharedFunctions.Sum(iRawData) / (float)iRawData.Length;
			
		// Standard deviation
		standardDeviation = SharedFunctions.CalculateStandardDeviation(iRawData, mean);
	}

//**************************************************************************************	
}
