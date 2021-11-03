using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class BinanceOrderBookRequest 
{
	List<OrderBookEntry> bids;
	List<OrderBookEntry> asks;

	bool isError = false;
	bool isComplete = false;
	string pair;
	
//**************************************************************************************

	public BinanceOrderBookRequest(string iPair)
	{
		pair = iPair;
	}

//**************************************************************************************

	public IEnumerator Request() 
	{
		if(bids!=null || asks!=null)
		{
			Debug.Log("Multiple requests is not supported by BinanceOrderBookRequest");
			yield break;
		}
		isComplete = false;
		asks = new List<OrderBookEntry>();
		bids = new List<OrderBookEntry>();

		WWW www = new WWW("https://api.binance.com/api/v1/depth?symbol=" + pair + "&limit=10");
		yield return www;

		if(www.error!=null)
		{
			isError = true;
			Debug.Log("Could not retrive Order Book data from binance: " + www.error);
		}
		else
			BinanceToRawData(www.text);
	
		www.Dispose();			
		isComplete = true;
	}

//**************************************************************************************

	void BinanceToRawData(string jsonString)
	{
		try
		{
			JObject jObject = JObject.Parse(jsonString);
			JToken jBids = jObject["bids"];
			JToken jAsks = jObject["asks"];

			foreach (JToken entry in jBids)
				bids.Add(new OrderBookEntry(entry[0].Value<float>(), entry[1].Value<float>()));	
		
			foreach (JToken entry in jAsks)
				asks.Add(new OrderBookEntry(entry[0].Value<float>(), entry[1].Value<float>()));	
		}
		catch (System.Exception e)
		{
			Debug.Log("Could not parse Binance Order Book data: " + e.ToString());
			bids.Clear();
			asks.Clear();
			bids = null;
			asks = null;
			isError = true;
		}
	}

//**************************************************************************************	

	public bool IsError() { return isError; }
	
//**************************************************************************************

	public List<OrderBookEntry> GetBids() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return bids;
	}

//**************************************************************************************

	public List<OrderBookEntry> GetAsks() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return asks;
	}

//**************************************************************************************

}
