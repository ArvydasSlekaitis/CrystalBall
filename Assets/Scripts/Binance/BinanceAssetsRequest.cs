using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


public class BinanceAssetsRequest : BinanceRequest
{
	bool isError = false;
	bool isComplete = false;
	Dictionary<string, float> assets;

//**************************************************************************************

	public IEnumerator Request(ulong iTimeNow) 
	{
		if(assets!=null)
		{
			Debug.Log("Multiple requests is not supported by BinanceAssetsRequest");
			yield break;
		}
		isComplete = false;
		assets = new Dictionary<string, float>();

		WWW www = CreateSignedRequest("https://api.binance.com/api/v3/account", "timestamp=" + iTimeNow.ToString());
		yield return www;

		if(www.error!=null)
		{
			isError = true;
			assets = null;
			Debug.Log("Could not retrive Account Information data from binance: " + www.error);
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
			JToken jBalances = jObject["balances"];

			foreach (JToken entry in jBalances)
				assets.Add(entry["asset"].Value<string>(), entry["free"].Value<float>());				
		}
		catch (System.Exception e)
		{
			Debug.Log("Could not parse Binance Account Information data: " + e.ToString());
			assets.Clear();
			assets = null;
			isError = true;
		}
	}

//**************************************************************************************	

	public bool IsError() { return isError; }
	
//**************************************************************************************

	public Dictionary<string, float> GetAssets() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return assets;
	}

//**************************************************************************************



}
