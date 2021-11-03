using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair 
{
	static Dictionary<string, ushort> toID;
	static List<string> toString;

	static Dictionary<ushort, string> toSourceAsset;
	static Dictionary<ushort, string> toDestinationAsset;

	static List<bool> isEnabled;

//**************************************************************************************

	static void Initialize()
	{
		if(toID!=null)
			return;

		toID = new Dictionary<string, ushort>();
		toString = new List<string>();
		isEnabled = new List<bool>();
		toSourceAsset = new Dictionary<ushort, string>();
		toDestinationAsset = new Dictionary<ushort, string>();

		RegisterNewPair("BTCUSDT", "USDT", "BTC", 10.0f);
		RegisterNewPair("BNBBTC", "BTC", "BNB", 0.001f);
		RegisterNewPair("NEOBNB", "BNB", "NEO", 1.00f);
		RegisterNewPair("AEBNB", "BNB", "AE", 1.00f);
		RegisterNewPair("CMTBNB", "BNB", "CMT", 1.00f);
	}

//**************************************************************************************

	static void RegisterNewPair(string iName, string iSourceAsset, string iDestinationAsset, float iMinOrder)
	{
		ushort id = (ushort)toID.Count;

		toID.Add(iName, id);
		toString.Add(iName);
		toSourceAsset.Add(id, iSourceAsset);
		toDestinationAsset.Add(id, iDestinationAsset);
		isEnabled.Add(true);
	}

//**************************************************************************************

	public static ushort ToID(string iName)
	{
		Initialize();
		
		return toID[iName];
	} 

//**************************************************************************************	

	public static string ToString(ushort iID)
	{
		Initialize();
		
		return toString[iID];
	} 

//**************************************************************************************

	public static int GetMaxID()
	{
		Initialize();

		return toString.Count-1;
	}

//**************************************************************************************

	public static bool IsEnabled(ushort iID)
	{
		Initialize();

		return isEnabled[iID];
	}

//**************************************************************************************

	public static string ToSourceAsset(ushort iID)
	{
		Initialize();
		
		return toSourceAsset[iID];
	} 

//**************************************************************************************	

	public static string ToDestinationAsset(ushort iID)
	{
		Initialize();
		
		return toDestinationAsset[iID];
	} 

//**************************************************************************************
}
