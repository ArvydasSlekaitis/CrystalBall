using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandlestickConsolidator : Consolidator 
{ 
	public ConsolidatedCandlestick data { get; private set; }

	CandlestickDataStore dataStore;
	ushort periods;

//**************************************************************************************

	public CandlestickConsolidator(CandlestickDataStore iDataStore, ushort iPeriods)
	{
		dataStore = iDataStore;
		periods = iPeriods;

		if(iDataStore == null)
			throw new System.ArgumentException("Parameter cannot be null", "iDataStore");

		if(periods < 1)
			throw new System.ArgumentException("Parameter must be 1 or greater", "iPeriods");

		iDataStore.AddListiner(this);
	}

//**************************************************************************************

	public override void Consolidate()
	{
		if(dataStore.data.Count < periods)
		{
			Debug.Log("Could not update historical data. Data store does not contain enough periods. Need:" + periods + " Given:" + dataStore.data.Count + " Pair:" + Pair.ToString(dataStore.pairID));
			return;
		}	

		CandlestickEntry[] rawData = new CandlestickEntry[periods];
		dataStore.data.CopyTo(dataStore.data.Count-periods, rawData, 0, periods);	

		try
		{
			data = new ConsolidatedCandlestick(rawData);
		}
		catch (System.Exception ex)
		{
				validTill = 0;
				Debug.Log("Could not consolidate data:" + ex.ToString());
				return;
		}

		validTill = dataStore.validTill;
	}

//**************************************************************************************

	public override void Release()
	{
		dataStore.RemoveListiner(this);
	}

//**************************************************************************************

	public bool Is(CandlestickDataStore iDataStore, ushort iPeriods)
	{
		return dataStore == iDataStore && periods == iPeriods;
	}

//**************************************************************************************
}
