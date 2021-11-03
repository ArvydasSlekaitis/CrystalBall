using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsolidatedCandlestick 
{
	float[] rawAveragePrice;
	float[] rawLowPrice;
	float[] rawHighPrice;

	public ulong startTime { get; }
	public ulong endTime { get; }
	
	public float openPrice { get; }
	public float closePrice { get; }
	public float middlePrice { get; }

	public float volumeWeightedAveragePrice  { get; }
	public float volumeWeightedAveragePriceStdDev  { get; }

	public DescriptiveStatistics averagePrice { get; }
	public DescriptiveStatistics volume { get; }
	public DescriptiveStatistics tradeCount { get;  }

	MACD macd;
	RSI rsi;	
	LHA lha;

//**************************************************************************************

	public ConsolidatedCandlestick (CandlestickEntry[] iRawData)
	{
		//Raw data
		rawAveragePrice = new float[iRawData.Length];
		float[] rawVolumeWeights = new float[iRawData.Length];
		rawHighPrice = new float[iRawData.Length];
		rawLowPrice = new float[iRawData.Length];
		float[] rawVolume = new float[iRawData.Length];
		float[] rawTradeCount = new float[iRawData.Length];

		for (int i = 0; i < iRawData.Length; i++) 
		{
			rawAveragePrice [i] = (iRawData [i].lowPrice + iRawData [i].highPrice)/2.0f;
			rawHighPrice [i] = iRawData [i].highPrice;
			rawLowPrice [i] = iRawData [i].lowPrice;
			rawVolume [i] = iRawData [i].volume;
			rawTradeCount [i] = iRawData [i].tradeCount;
			rawVolumeWeights[i] = rawVolume [i];
		}
			
		//Open price
		openPrice = iRawData[0].openPrice;

		//Close price
		closePrice = iRawData [iRawData.Length - 1].closePrice;

		//Middle price
		middlePrice = rawAveragePrice[rawLowPrice.Length/2];

		//Price
		averagePrice = new CandlestickDescriptiveStatistics (rawAveragePrice, rawLowPrice, rawHighPrice);
	
		//Volume
		volume = new SimpleDescriptiveStatistics (rawVolume);

		//Trade count
		tradeCount = new SimpleDescriptiveStatistics (rawTradeCount);

		//Volume weighted price
		float volumeSum = SharedFunctions.Sum(rawVolume);
		if(volumeSum > 0)
		{
			for (int i = 0; i < rawVolumeWeights.Length; i++)
				rawVolumeWeights [i] /= volumeSum;

			volumeWeightedAveragePrice = SharedFunctions.CalculateWeightedArithmeticMean(rawAveragePrice, rawVolumeWeights);
			volumeWeightedAveragePriceStdDev = SharedFunctions.CalculateWeightedStandardDeviation(rawAveragePrice, rawVolumeWeights, volumeWeightedAveragePrice);
		}

		//Start & End time
		startTime = iRawData[0].startTime;
		endTime = iRawData[iRawData.Length-1].endTime;
	}

//**************************************************************************************

	public MACD GetMACD()
	{
		if(macd==null)
			macd = new MACD(rawAveragePrice, 26, 12, 9);
		
		return macd;
	}

//**************************************************************************************

	public RSI GetRSI()
	{
		if(rsi==null)
			rsi = new RSI(rawAveragePrice,  Mathf.Min(rawAveragePrice.Length-1, 14));
		
		return rsi;
	}

//**************************************************************************************

	public LHA GetLHA()
	{
		if(lha==null)
			lha = new LHA(rawLowPrice, rawHighPrice);
		
		return lha;
	}

//**************************************************************************************
}
