using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Miner 
{
	MonoBehaviour mono;

	List<CandlestickDataStore> candlestickDataStore;
	List<CandlestickConsolidator> candlestickConsolidator;

	List<OrderBookDataStore> orderBook;

//**************************************************************************************

	public Miner(MonoBehaviour iMono)
	{
		if(iMono == null)
			throw new System.ArgumentException("Parameter cannot be null", "iMono");

		mono = iMono;
		candlestickDataStore = new List<CandlestickDataStore>();
		candlestickConsolidator = new List<CandlestickConsolidator>();
		orderBook = new List<OrderBookDataStore>();
	}

//**************************************************************************************	

	public IEnumerator Update(ulong iTimeNow)
	{
		Cleanup();

		Coroutine dataStore1 = mono.StartCoroutine(Update(candlestickDataStore.Cast<DataStore>().ToArray(), iTimeNow));
		Coroutine dataStore2 = mono.StartCoroutine(Update(orderBook.Cast<DataStore>().ToArray(), iTimeNow));

		yield return dataStore1;
		yield return dataStore2;

		Debug.Log("Miner update complete (candlestick:" + candlestickDataStore.Count + "; orderbook:" + orderBook.Count + ";)");
	}

//**************************************************************************************

	IEnumerator Update(DataStore[] iDataStore, ulong iTimeNow)
	{
		if(iDataStore.Length <= 0)
			yield break;

		Coroutine[] coroutines = new Coroutine[iDataStore.Length];

		// Update data stores
		for(int i=0; i<iDataStore.Length; i++)
		{
			try
			{
				coroutines[i] = mono.StartCoroutine(iDataStore[i].Update(iTimeNow));
			}
			catch (System.Exception ex)
			{
				Debug.Log("Could not update DataStore:" + ex.ToString());
			}
		}	

		// Wait for data stores to complete
		for(int i=0; i<coroutines.Length; i++)
		{
			if(coroutines[i]!=null)
				yield return coroutines[i];
		}
	}

//**************************************************************************************	

	public CandlestickConsolidator GetCandlestickConsolidator(ushort iPairID, ushort iCandlestickPeriod)
	{
		// Data store
		CandlestickDataStore requiredDataStore = FindCandlestickDataStore(iPairID);

		if(requiredDataStore == null)
		{
			requiredDataStore = new CandlestickDataStore(mono, iPairID);	
			candlestickDataStore.Add(requiredDataStore);	
		}

		// Consolidator
		CandlestickConsolidator requiredConsolidator = FindCandlestickConsolidator(requiredDataStore, iCandlestickPeriod);

		if(requiredConsolidator == null)
		{
			requiredConsolidator = new CandlestickConsolidator(requiredDataStore, iCandlestickPeriod);
			candlestickConsolidator.Add(requiredConsolidator);
		}

		return requiredConsolidator;
	}


//**************************************************************************************

	public OrderBookDataStore GetOrderBook(ushort iPairID)
	{
		// Data store
		OrderBookDataStore dataStore = FindOrderBook(iPairID);

		if(dataStore == null)
		{
			dataStore = new OrderBookDataStore(mono, iPairID);	
			orderBook.Add(dataStore);	
		}

		return dataStore;
	}

//**************************************************************************************

	CandlestickDataStore FindCandlestickDataStore(ushort iPairID)
	{
		for(int i=0; i<candlestickDataStore.Count; i++)
			if(candlestickDataStore[i].Is(iPairID))
				return candlestickDataStore[i];

		return null;	
	}

//**************************************************************************************

	OrderBookDataStore FindOrderBook(ushort iPairID)
	{
		for(int i=0; i<orderBook.Count; i++)
			if(orderBook[i].Is(iPairID))
				return orderBook[i];

		return null;	
	}

//**************************************************************************************

	CandlestickConsolidator FindCandlestickConsolidator(CandlestickDataStore iDataStore, ushort iCandlestickPeriod)
	{
		for(int i=0; i<candlestickConsolidator.Count; i++)
		if(candlestickConsolidator[i].Is(iDataStore, iCandlestickPeriod))
			return candlestickConsolidator[i];

		return null;	
	}

//**************************************************************************************

	void Cleanup()
	{
		// Candlestick consolidators
		for(int i=0; i<candlestickConsolidator.Count; i++)
			if(candlestickConsolidator[i].listeners <= 0)
			{
				Debug.Log("candlestickConsolidator removed");
				candlestickConsolidator[i].Release();
				candlestickConsolidator.RemoveAt(i);
				i--;
			}

		// Candlestick data store
		for(int i=0; i<candlestickDataStore.Count; i++)
			if(candlestickDataStore[i].GetListinerCount() <= 0)
			{
				Debug.Log("candlestickDataStore removed");
				candlestickDataStore.RemoveAt(i);
				i--;
			}				
	}

//**************************************************************************************
}
