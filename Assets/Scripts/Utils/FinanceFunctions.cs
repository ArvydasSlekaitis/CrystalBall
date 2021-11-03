using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinanceFunctions
{
	public const float kMinTreshold = 0.0015f;

//**************************************************************************************

	public static bool ShouldBuy(float iMeanPrice, float iMeanPriceStdDev, float iBestPrice)
	{
        if(iMeanPrice <= 0)
		    throw new System.ArgumentException("Value should be greater than zero", "iMeanPrice");

		if(iMeanPriceStdDev < 0)
		    throw new System.ArgumentException("Value should be greater than zero", "iPriceStandardDeviation");	

		if(iBestPrice <= 0)
			throw new System.ArgumentException("Value should be greater than zero", "iBestPrice");

		return iBestPrice < (iMeanPrice - (Mathf.Max(iMeanPriceStdDev, iMeanPrice*kMinTreshold)));
	}

//**************************************************************************************

	public static bool ShouldSell(float iMeanPrice, float iMeanPriceStdDev, float iBestPrice)
	{
        if(iMeanPrice <= 0)
		    throw new System.ArgumentException("Value should be greater than zero", "iMeanPrice");

		if(iMeanPriceStdDev < 0)
		    throw new System.ArgumentException("Value should be greater than zero", "iPriceStandardDeviation");	

		if(iBestPrice <= 0)
			throw new System.ArgumentException("Value should be greater than zero", "iBestPrice");
		return iBestPrice > (iMeanPrice + (Mathf.Max(iMeanPriceStdDev, iMeanPrice*kMinTreshold)));
	}

//**************************************************************************************	

	public static float GetSellPerformance(float iBuyPrice, float iSellPrice)
	{
        return (iSellPrice * (1.0f - kMinTreshold)) / (iBuyPrice * (1.0f + kMinTreshold));
	}

//**************************************************************************************

	public static float GetLifetimePerformance(uint iNumberOfTrades, uint iLifetimeTicks, float iProductOfPerformance, float iLastTradePerformance)
	{
		if(iNumberOfTrades < 10)
            return 0.0f;

		float rawPerformance = Mathf.Min(iLastTradePerformance, Mathf.Pow(iProductOfPerformance, 1.0f/(float)iNumberOfTrades));
		
        if(rawPerformance < 1.0f)
            return 0.0f;
        else
            return rawPerformance * ((float)iNumberOfTrades/Mathf.Max(1.0f, (float)iLifetimeTicks/5760.0f)) * Mathf.Min(1.0f, (float)iNumberOfTrades/1000.0f);
	}

//**************************************************************************************	

	public static float CalculateFutureProfits(float[] iPrice, float iCOC, short iStartTime, short iMaxProfitTime)
	{
		float kCOCMultiplier = 1 + iCOC;
		float kP0 = iPrice[iStartTime];
		float profit = 0;
		float culmulativeCOC = kCOCMultiplier;

		if(iPrice == null)
			throw new System.ArgumentException("Array should not be NULL", "iPrice");

		if(iCOC < 0)
			throw new System.ArgumentException("Cost of capital cannot be less than 0", "iCOC");

		for (int i = iStartTime+1; i < iStartTime + 1 + iMaxProfitTime; i++)
		{
			profit += (iPrice[i] - kP0) / culmulativeCOC;
			culmulativeCOC *= kCOCMultiplier;
		}

		return profit;
	}

//**************************************************************************************

	public static float GetIndicatorValue_MeanToStdDev(float iMeanPrice, float iMeanPriceStandardDeviation, float iCurrentPrice)
	{
		if(iMeanPrice < 0)
			throw new System.ArgumentException("Mean price cannot be less than 0", "iMeanPrice");

		if(iMeanPriceStandardDeviation < 0)
			throw new System.ArgumentException("Mean price standard deviation cannot be less than 0", "iMeanPriceStandardDeviation");
	
		if(iCurrentPrice <= 0)
			throw new System.ArgumentException("Current price cannot be less or equal to 0", "iCurrentPrice");

		return (iCurrentPrice - iMeanPrice)/iMeanPriceStandardDeviation;   
	}

//**************************************************************************************
}
