using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DescriptiveStatistics
{

//**************************************************************************************

	public abstract LinearRegression GetLinearRegression();
	public abstract MinMax GetMinMax();
	public abstract EMA  GetEMA(int iNumberOfPeriods);
	public abstract ArithmeticMean GetArithmeticMean();

//**************************************************************************************

	public DescriptiveStatistics()
	{
	}
	
//**************************************************************************************


}
