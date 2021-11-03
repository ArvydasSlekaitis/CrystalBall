using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class BinanceRecentTradeRequest 
{
    OrderBookEntry recentTrade;

	bool isError = false;
	bool isComplete = false;
	string pair;
	
//**************************************************************************************

	public BinanceRecentTradeRequest(string iPair)
	{
		pair = iPair;
	}

//**************************************************************************************

	public IEnumerator Request() 
	{
		if(recentTrade!=null)
		{
			Debug.Log("Multiple requests is not supported by BinanceRecentTradesRequest");
			yield break;
		}
		isComplete = false;

		WWW www = new WWW("https://api.binance.com/api/v1/trades?symbol=" + pair + "&limit=1");
		yield return www;

		if(www.error!=null)
		{
			isError = true;
			Debug.Log("Could not retrive Recent Trades data from binance: " + www.error);
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
            JArray array = JArray.Parse(jsonString);
			if(array.Count > 0)
			{
				JToken token =  array[0];
				recentTrade = new OrderBookEntry(token["price"].Value<float>(), token["qty"].Value<float>());
			}	  
		}
		catch (System.Exception e)
		{
			Debug.Log("Could not parse Binance Recent Trades requestd data: " + e.ToString());
			recentTrade = null;
			isError = true;
		}
	}

//**************************************************************************************	

	public bool IsError() { return isError; }
	
//**************************************************************************************

	public OrderBookEntry GetRecentTrade() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return recentTrade;
	}

//**************************************************************************************
}
