using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class SharedFunctions 
{

//**************************************************************************************

	public static float CalculateArithmeticMean(float[] iNumbers)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		float sum = 0.0f;

		for(int i=0; i<iNumbers.Length; i++)
			sum += iNumbers[i];

		return sum / (float)iNumbers.Length;
	}

//**************************************************************************************

	// Constrain: Sum(iWeights) == 1
	public static float CalculateWeightedArithmeticMean(float[] iNumbers, float[] iWeights)
	{
		if (iNumbers == null || iNumbers.Length < 1 || iWeights == null || iNumbers.Length != iWeights.Length)
			return 0;

		float sum = 0.0f;

		for(int i=0; i<iNumbers.Length; i++)
			sum += iNumbers[i]*iWeights[i];

		return sum;
	}

//**************************************************************************************

	public static float CalculateMedian(float[] iNumbers)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		if(iNumbers.Length == 1)
			return iNumbers[0];

		List<float> sortedList = new List<float>(iNumbers);
		sortedList.Sort();

		float halfRange = ((float)sortedList.Count-1.0f)/2.0f;
		int floorHalfRange = Mathf.FloorToInt(halfRange);

		if(floorHalfRange == Mathf.CeilToInt(halfRange))
			return sortedList[floorHalfRange];
		else
			return (sortedList[floorHalfRange] + sortedList[floorHalfRange + 1])*0.5f;
	}

//**************************************************************************************

	public static float CalculateGeometricMean(float[] iNumbers)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		float product = iNumbers [0];

		for(int i=1; i<iNumbers.Length; i++)
			product *= iNumbers[i];

		return Mathf.Pow (product, 1.0f / (float)iNumbers.Length);;
	}

//**************************************************************************************

	public static float CalculateHarmonicMean(float[] iNumbers)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		float sum = 0.0f;

		foreach (float v in iNumbers)
			sum += 1.0f/v;

		return (float)iNumbers.Length / sum;
	}

//**************************************************************************************

	// EMA - Exponential moving average. iNumbers[0] - oldest entry
	public static float CalculateEMA(float[] iNumbers, int iNumberOfPeriods)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		float k = 2.0f/(iNumberOfPeriods+1.0f);
		float[] ema = new float[iNumbers.Length];
		ema[0] = iNumbers[0];

		for(int i=1; i<iNumbers.Length; i++)
			ema[i] = iNumbers[i] * k + ema[i - 1] * (1 - k);

		return ema[ema.Length-1];
	}

//**************************************************************************************

	public static void Max(float[] iNumbers, ref float oMaxValue, ref int oMaxID)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return;
		
		oMaxValue = iNumbers [0];
		oMaxID = 0;

		for (int i = 1; i < iNumbers.Length; i++)
			if (iNumbers [i] > oMaxValue) 
			{			
				oMaxValue = iNumbers [i];
				oMaxID = i;
			}
	}

//**************************************************************************************

	public static void Min(float[] iNumbers, ref float oMinValue, ref int oMinID)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return;

		oMinValue = iNumbers [0];
		oMinID = 0;

		for (int i = 1; i < iNumbers.Length; i++)
			if (iNumbers [i] < oMinValue) 
			{
				oMinValue = iNumbers [i];
				oMinID = i;
			}
	}

//**************************************************************************************

	public static float Sum(float[] iNumbers)
	{
		if (iNumbers == null || iNumbers.Length < 1)
			return 0;

		float sum = iNumbers [0];

		for (int i = 1; i < iNumbers.Length; i++)
			sum += iNumbers [i];

		return sum;
	}


//**************************************************************************************

	public static float CalculateStandardDeviation(float[] iNumbers, float iMean)
	{
		if (iNumbers == null || iNumbers.Length < 2)
			return 0;

		float sum = 0.0f;

		for(int i=0; i<iNumbers.Length; i++)
			sum += Mathf.Pow(iNumbers[i] - iMean, 2.0f);

		return Mathf.Sqrt(sum / ((float)iNumbers.Length - 1.0f));
	}

//**************************************************************************************

	public static float CalculateStandardDeviation(float[] iNumbers1, float[] iNumbers2)
	{
		if (iNumbers1 == null || iNumbers1.Length < 1 || iNumbers2 == null || iNumbers2.Length < 1 || iNumbers1.Length!=iNumbers2.Length)
			return 0;

		float sum = 0.0f;

		for(int i=0; i<iNumbers1.Length; i++)
			sum += Mathf.Pow(iNumbers1[i] - iNumbers2[i], 2.0f);

		return Mathf.Sqrt(sum / ((float)iNumbers1.Length - 1.0f));
	}

//**************************************************************************************

	public static float CalculateWeightedStandardDeviation(float[] iNumbers, float[] iWeights, float iMean)
	{
		if (iNumbers == null || iNumbers.Length < 2)
			return 0;

		float sum = 0.0f;

		for(int i=0; i<iNumbers.Length; i++)
			sum += iWeights[i] * Mathf.Pow(iNumbers[i] - iMean, 2.0f);

		return Mathf.Sqrt(sum / ((float)iNumbers.Length - 1.0f));
	}	

//**************************************************************************************

	public static void SortPoints(ref float[] iPointValue, ref float[] iPointLocation)
	{
		if (iPointValue == null || iPointValue.Length < 1 || iPointLocation == null || iPointLocation.Length < 1 || iPointValue.Length != iPointLocation.Length)
			return;
		
		for (int i = 0; i < iPointLocation.Length; i++)
		{
			int lowestPointID = i;
			float lowestPointLocation = iPointLocation[i];

			for (int ii = i+1; ii < iPointLocation.Length; ii++)
				if (iPointLocation [ii] < lowestPointLocation)
				{
					lowestPointID = ii;
					lowestPointLocation = iPointLocation [ii];
				}

			if (lowestPointID != i) 
			{
				float previousPointValue = iPointValue [i];
				float previousPointLocation = iPointLocation [i];

				iPointValue [i] = iPointValue [lowestPointID];
				iPointLocation [i] = iPointLocation [lowestPointID];

				iPointValue [lowestPointID] = previousPointValue;
				iPointLocation [lowestPointID] = previousPointLocation;
			}
		}					
	}

//**************************************************************************************

	//Points locations should be pre-sorted 
	public static float GetExtrapolatedPointValue(float iTargetPointLocation, float[] iPointValue, float[] iPointLocation)
	{
		if (iPointValue == null || iPointValue.Length < 1 || iPointLocation == null || iPointLocation.Length < 1)
			return-1;
		
		iTargetPointLocation = Mathf.Clamp (iTargetPointLocation, 0.0f, 1.0f);

		for (int i = 0; i < iPointLocation.Length; i++)
			if (iTargetPointLocation <= iPointLocation[i]) 
			{
				if (i == 0)
					return iPointValue [i];
				else
					return iPointValue [i - 1] + ((iPointValue [i] - iPointValue [i - 1]) / (iPointLocation [i] - iPointLocation [i - 1])) * (iTargetPointLocation - iPointLocation [i - 1]);
			}
			
		return -1;
	}

//**************************************************************************************

	//Points locations should be pre-sorted 
	public static float[] GetExtrapolatedPoints(int iDesiredNumberOfPoints, float[] iPointValue, float[] iPointLocation)
	{
		if (iPointValue == null || iPointValue.Length < 1 || iPointLocation == null || iPointLocation.Length < 1 || iDesiredNumberOfPoints < 2)
			return null;

		float[] extrapolatedPoints = new float[iDesiredNumberOfPoints];
		float intervalBetweenPoints = 1.0f / ((float)iDesiredNumberOfPoints - 1.0f);

		float currentPointLocation = 0;
		int currentPointID = 0;

		while(currentPointLocation <=iPointLocation[iPointLocation.Length-1] && currentPointID < extrapolatedPoints.Length)
		{
			for (int i = 0; i < iPointLocation.Length; i++)
				if (currentPointLocation <= iPointLocation[i]) 
				{
					if (i == 0)
						extrapolatedPoints[currentPointID] =  iPointValue [i];
					else
						extrapolatedPoints[currentPointID] =  iPointValue [i - 1] + ((iPointValue [i] - iPointValue [i - 1]) / (iPointLocation [i] - iPointLocation [i - 1])) * (currentPointLocation - iPointLocation [i - 1]);

					break;
				}

			currentPointLocation = Mathf.Min (currentPointLocation + intervalBetweenPoints, iPointLocation [iPointLocation.Length - 1]);
			currentPointID++;
		}
			
		return extrapolatedPoints;
	}

//**************************************************************************************

	public static System.DateTime UnixToDateTime(double iUnixTime)
	{
		System.DateTime dtDateTime = new System.DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
    	dtDateTime = dtDateTime.AddMilliseconds( iUnixTime );
    	return dtDateTime;
	}

//**************************************************************************************

public static string HmacSha256Digest(string message, string secret)
{
    ASCIIEncoding encoding = new ASCIIEncoding();
    byte[] keyBytes = encoding.GetBytes(secret);
    byte[] messageBytes = encoding.GetBytes(message);
    System.Security.Cryptography.HMACSHA256 cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes);

    byte[] bytes = cryptographer.ComputeHash(messageBytes);

    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
}

//**************************************************************************************

	public static void LinearRegression(float[] xVals, float[] yVals, int inclusiveStart, int exclusiveEnd, out float rsquared, out float yintercept, out float slope)
	{
		Debug.Assert(xVals.Length == yVals.Length);
		float sumOfX = 0;
		float sumOfY = 0;
		float sumOfXSq = 0;
		float sumOfYSq = 0;
		float ssX = 0;
		float ssY = 0;
		float sumCodeviates = 0;
		float sCo = 0;
		float count = exclusiveEnd - inclusiveStart;

		for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
		{
			float x = xVals[ctr];
			float y = yVals[ctr];
			sumCodeviates += x * y;
			sumOfX += x;
			sumOfY += y;
			sumOfXSq += x * x;
			sumOfYSq += y * y;
		}
		ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
		ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
		float RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
		float RDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
		sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

		float meanX = sumOfX / count;
		float meanY = sumOfY / count;
		float dblR = RNumerator / Mathf.Sqrt(RDenom);
		rsquared = dblR * dblR;
		yintercept = meanY - ((sCo / ssX) * meanX);
		slope = sCo / ssX;
	}

//**************************************************************************************	

	public static ulong DateTimeToUnix(DateTime iDateTime)
	{
		return (ulong)iDateTime.Subtract(new System.DateTime(1970, 1, 1)).TotalMilliseconds;
	}

//**************************************************************************************	
}
