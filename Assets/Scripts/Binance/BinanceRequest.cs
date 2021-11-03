using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public abstract class BinanceRequest
{
	const string key = "ysiM2pNwE7z4O5iZXGrDvW3h3g6LcZfi85U0RZH46LS5OYyN4GPbQ2z5vtneBe3M";
	const string api = "cRiEz1vuXSUwZwN7XJVdMoXxwi1GoXLgmbArwPsglrAEdEO4hNxhmagoMywFLIGm";
	static Dictionary<string, string> headers;

//**************************************************************************************

	public BinanceRequest()
	{
		if(headers == null)
		{
			 headers = new Dictionary<string,string>();
			 headers.Add("X-MBX-APIKEY", api);
		}
	}


//**************************************************************************************

	protected WWW CreateSignedRequest(string iURL, string iMessage)
	{
		if(iMessage == null || iMessage.Length < 1)
			throw new System.ArgumentException("Must be a valid message", "iMessage");

		string hashHMACHex = SharedFunctions.HmacSha256Digest(iMessage, key);
		return new WWW(iURL + "?" + iMessage + "&signature=" + hashHMACHex, null, headers);
	}

//**************************************************************************************

	protected WWW CreateSignedPostRequest(string iURL, string iMessage)
	{
		if(iMessage == null || iMessage.Length < 1)
			throw new System.ArgumentException("Must be a valid message", "iMessage");

		WWWForm form = new WWWForm();
		string hashHMACHex = SharedFunctions.HmacSha256Digest(iMessage, key);

		ASCIIEncoding encoding = new ASCIIEncoding();
		byte[] postData = encoding.GetBytes(iMessage + "&signature=" + hashHMACHex);

		return new WWW(iURL, postData, headers);
	}

//**************************************************************************************

}
