using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandlestickDescriptiveStatistics : DescriptiveStatistics 
{
	float[] averageData;
	float[] lowData;
	float[] highData;

	LinearRegression linearRegression;
	MinMax minMax;
	Dictionary<int, EMA> ema;
	ArithmeticMean arithmeticMean;

//**************************************************************************************

	public CandlestickDescriptiveStatistics(float[] iAverage, float[] iLow, float[] iHigh)
	{
		if(iAverage == null || iLow == null || iHigh == null)
			throw new System.ArgumentException("Parameter cannot be null", "iAverage || iLow || iHigh");

		if (iAverage.Length < 1)
			throw new System.ArgumentException("Parameter cannot be empty", "iAverage");

		if(iLow.Length != iHigh.Length || iLow.Length != iAverage.Length)
			throw new System.ArgumentException("Parameters must be exactely the same size", "iAverage, iHigh, iLow");	

		averageData = iAverage;
		lowData = iLow;
		highData = iHigh;
		ema = new Dictionary<int, EMA>();
	}

//**************************************************************************************

	public override LinearRegression GetLinearRegression()
	{
		if(linearRegression == null && averageData.Length > 0)
			linearRegression = new LinearRegression(averageData);

		return linearRegression;
	}

//**************************************************************************************

	public override MinMax GetMinMax()
	{
		if(minMax == null && lowData.Length > 0)
			minMax = new MinMax(lowData, highData);

		return minMax;
	}

//**************************************************************************************

	public override EMA GetEMA(int iNumberOfPeriods)
	{
		if(averageData.Length <= 0)
			return null;

		EMA value = null;

		if(ema.TryGetValue(iNumberOfPeriods, out value))
			return value;
		else
		{
			value = new EMA(averageData, iNumberOfPeriods);
			ema.Add(iNumberOfPeriods, value);
			return value;
		}
	}

//**************************************************************************************	

	public override ArithmeticMean GetArithmeticMean()
	{
		if(arithmeticMean == null && averageData.Length > 0)
			arithmeticMean = new ArithmeticMean(averageData);

		return arithmeticMean;
	}

//**************************************************************************************

}
