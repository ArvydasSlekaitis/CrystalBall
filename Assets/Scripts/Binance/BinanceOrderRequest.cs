using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinanceOrderRequest : BinanceRequest
{
	bool isError = false;
	bool isComplete = true;

//**************************************************************************************

	public IEnumerator Request(ulong iTimeNow, string iPair, bool iIsBuy, float iQuantity, float iPrice) 
	{
		if(!isComplete)
			throw new System.Exception("Multiple requests is not supported by BinanceOrderRequest");

		isComplete = false;

		WWW www = CreateSignedPostRequest("https://api.binance.com/api/v3/order/test", "symbol=" + iPair + "&side=" + (iIsBuy ? "BUY" : "SELL") + "&type=LIMIT&timeInForce=FOK" + "&quantity=" + iQuantity + "&price=" + iPrice + "&recvWindow=5000" +"&timestamp=" + iTimeNow.ToString());
		
		Debug.Log("Creating Order Request: " + www.url);

		yield return www;

		if(www.error!=null)
		{
			isError = true;
			Debug.Log("Could not fulfill Order request to binance: " + www.error);
		}
		else
			BinanceToRawData(www.text);

		www.Dispose();			
		isComplete = true;
	}

//**************************************************************************************

	void BinanceToRawData(string jsonString)
	{
		Debug.Log(jsonString);
	}

//**************************************************************************************	

	public bool IsError() { return isError; }
	
//**************************************************************************************



}
