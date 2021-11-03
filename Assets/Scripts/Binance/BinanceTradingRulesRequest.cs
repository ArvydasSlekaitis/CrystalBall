using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


public class BinanceTradingRulesRequest : BinanceRequest
{
	bool isError = false;
	bool isComplete = false;
	Dictionary<string, float> minQuantity;
	Dictionary<string, float> minValue;

//**************************************************************************************

	public IEnumerator Request() 
	{
		if(minQuantity!=null || minValue!=null)
		{
			Debug.Log("Multiple requests is not supported by BinanceTradingRulesRequest");
			yield break;
		}
		isComplete = false;
		minQuantity = new Dictionary<string, float>();
		minValue = new Dictionary<string, float>();

		WWW www = new WWW("https://api.binance.com/api/v1/exchangeInfo");
		yield return www;

		if(www.error!=null)
		{
			isError = true;
			minQuantity = null;
			minValue = null;
			Debug.Log("Could not retrive Trading Rules data from binance: " + www.error);
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
			JToken jSymbols = jObject["symbols"];

			foreach (JToken entry in jSymbols)
			{
				string symbol = entry["symbol"].Value<string>();
				JToken filters = entry["filters"];
		
				foreach(JToken filter in filters)
				{
					if(filter["filterType"].Value<string>() == "LOT_SIZE")
						minQuantity.Add(symbol, filter["minQty"].Value<float>());

					if(filter["filterType"].Value<string>() == "MIN_NOTIONAL")
						minValue.Add(symbol, filter["minNotional"].Value<float>());
				}
					
			}			
		}
		catch (System.Exception e)
		{
			Debug.Log("Could not parse Binance Account Information data: " + e.ToString());
			minQuantity.Clear();
			minValue.Clear();
			minQuantity = null;
			minValue = null;
			isError = true;
		}
	}

//**************************************************************************************	

	public bool IsError() { return isError; }
	
//**************************************************************************************

	public Dictionary<string, float> GetMinQuantity() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return minQuantity;
	}

//**************************************************************************************

	public Dictionary<string, float> GetMinValue() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return minValue;
	}	

//**************************************************************************************



}
