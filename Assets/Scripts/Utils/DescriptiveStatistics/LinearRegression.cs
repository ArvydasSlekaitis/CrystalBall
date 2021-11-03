using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearRegression 
{
	public float rSq { get; }
	public float intercept { get; }
	public float slope { get; }

//**************************************************************************************

	public LinearRegression(float[] yVals)
	{
		float sumOfX = 0;
		float sumOfY = 0;
		float sumOfXSq = 0;
		float sumOfYSq = 0;
		float ssX = 0;
		float ssY = 0;
		float sumCodeviates = 0;
		float sCo = 0;
		float count = yVals.Length;

		for (int ctr = 0; ctr < yVals.Length; ctr++)
		{
			float x = ctr;
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
		rSq = dblR * dblR;
		intercept = meanY - ((sCo / ssX) * meanX);
		slope = sCo / ssX;
	}

//**************************************************************************************
}
