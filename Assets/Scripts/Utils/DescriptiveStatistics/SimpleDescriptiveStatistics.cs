using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDescriptiveStatistics : DescriptiveStatistics 
{
	float[] rawData;
	LinearRegression linearRegression;
	MinMax minMax;
	Dictionary<int, EMA> ema;
	ArithmeticMean arithmeticMean;

//**************************************************************************************

	public SimpleDescriptiveStatistics(float[] iRawData) 
	{
		if(iRawData == null)
			throw new System.ArgumentException("Parameter cannot be null", "iRawData");
		
		if (iRawData.Length < 1)
			throw new System.ArgumentException("Parameter cannot be empty", "iRawData");

		rawData = iRawData;
		ema = new Dictionary<int, EMA>();
	}

//**************************************************************************************

	public override LinearRegression GetLinearRegression()
	{
		if(linearRegression == null && rawData.Length > 0)
			linearRegression = new LinearRegression(rawData);

		return linearRegression;
	}

//**************************************************************************************

	public override MinMax GetMinMax()
	{
		if(minMax == null && rawData.Length > 0)
			minMax = new MinMax(rawData);

		return minMax;
	}

//**************************************************************************************

	public override EMA GetEMA(int iNumberOfPeriods)
	{
		if(rawData.Length <= 0)
			return null;

		EMA value = null;

		if(ema.TryGetValue(iNumberOfPeriods, out value))
			return value;
		else
		{
			value = new EMA(rawData, iNumberOfPeriods);
			ema.Add(iNumberOfPeriods, value);
			return value;
		}
	}

//**************************************************************************************

	public override ArithmeticMean GetArithmeticMean()
	{
		if(arithmeticMean == null && rawData.Length > 0)
			arithmeticMean = new ArithmeticMean(rawData);

		return arithmeticMean;
	}

//**************************************************************************************	

}
