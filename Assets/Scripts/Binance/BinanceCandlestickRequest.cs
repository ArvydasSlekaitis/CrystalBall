using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

public class BinanceCandlestickRequest 
{
	List<CandlestickEntry> rawData;
	bool isError = false;
	bool isComplete = false;
	string pair;
	string interval;
	ulong startTime;
	ulong endTime;
	
//**************************************************************************************

	public BinanceCandlestickRequest(string iPair, string iInterval, ulong iStartTime, ulong iEndTime)
	{
		pair = iPair;
		startTime = iStartTime;
		endTime = iEndTime;
		interval = iInterval;
	}

//**************************************************************************************

	public IEnumerator Request() 
	{
		if(rawData!=null)
		{
			Debug.Log("Multiple requests is not supported by MinutesDataRequest");
			yield break;
		}
		isComplete = false;
		rawData = new List<CandlestickEntry>();

		//Every 500 entries
		while(startTime < endTime)
		{
			ulong duration = endTime - startTime;
			ulong maxDuration = 0;
			switch (interval)
			{
				case "1m":
					maxDuration = 30000000;
					break;

				case "5m":
					maxDuration = 150000000;
					break;	

				case "15m":
					maxDuration = 450000000;
					break;	

				case "1h":
					maxDuration = 1800000000;
					break;		

				case "1d":
					maxDuration = 43200000000;
					break;	

				default: 
					throw new System.ArgumentException("Unsuported interval:" + interval);		

			}
			if(duration > maxDuration)
				duration = maxDuration;

			yield return RequestData(startTime, startTime+duration);

			if(isError)
			{
				rawData.Clear();
				rawData = null;
				isComplete = true;
				yield break;
			}

			startTime += duration;		
		}
				
		if(rawData.Count > 0 && rawData[rawData.Count-1].endTime > endTime)
			rawData.RemoveAt(rawData.Count-1);
			
		isComplete = true;
	}

//**************************************************************************************

	IEnumerator RequestData(ulong iStartTime, ulong iEndTime) 
	{
		WWW www = new WWW("https://api.binance.com/api/v1/klines?symbol=" + pair + "&interval="+interval+"&limit=500&startTime=" + iStartTime + "&endTime=" + iEndTime);
		yield return www;

		if(www.error!=null)
		{
			isError = true;
			Debug.Log("Could not retrive minutes data from binance: " + www.error);
		}
		else
			BinanceToRawData(www.text);

		www.Dispose();
	}

//**************************************************************************************

	void BinanceToRawData(string jsonString)
	{
		try
		{
			JArray array = JArray.Parse(jsonString);
			for(int i=0; i<array.Count; i++)
			{
				JToken token =  array[i];
				rawData.Add(new CandlestickEntry(token[0].Value<ulong>(), token[6].Value<ulong>(), token[1].Value<float>(), token[4].Value<float>(), token[2].Value<float>(), token[3].Value<float>(), token[5].Value<float>(), token[8].Value<uint>()));
			}
		}
		catch (System.Exception e)
		{
			Debug.Log("Could not parse Binance minutes data: " + e.ToString());
			isError = true;
		}
	}

//**************************************************************************************

	public bool IsError() { return isError; }
	
//**************************************************************************************

	public List<CandlestickEntry> GetRawData() 
	{ 
		if(!isComplete || isError) 
			return null; 
		else
			return rawData;
	}

//**************************************************************************************

}
