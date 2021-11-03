using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CandlestickEntry 
{
	public ulong startTime;
	public ulong endTime;
	public float openPrice;
	public float closePrice;
	public float highPrice;
	public float lowPrice;
	public float volume;
	public uint tradeCount;

//**************************************************************************************

	public CandlestickEntry() {}

//**************************************************************************************

	public CandlestickEntry(ulong iStartTime, ulong iEndTime, float iOpenPrice, float iClosePrice, float iHighPrice, float iLowPrice, float iVolume, uint iTradeCount) 
	{
		startTime = iStartTime;
		endTime = iEndTime;
		openPrice = iOpenPrice;
		closePrice = iClosePrice;
		highPrice = iHighPrice;
		lowPrice = iLowPrice;
		volume = iVolume;
		tradeCount = iTradeCount;
	}

//**************************************************************************************
	
	public CandlestickEntry(string iStartTime, string iEndTime, string iOpenPrice, string iClosePrice, string iHighPrice, string iLowPrice, string iVolume, string iTradeCount) 
	{
		startTime = ulong.Parse(iStartTime); 
		endTime = ulong.Parse(iEndTime); 
		openPrice = float.Parse(iOpenPrice);
		closePrice = float.Parse(iClosePrice);
		highPrice = float.Parse(iHighPrice);
		lowPrice = float.Parse(iLowPrice);
		volume = float.Parse(iVolume);
		tradeCount = uint.Parse(iTradeCount);
	}
	
//**************************************************************************************
	
	public CandlestickEntry(BinaryReader iReader) 
	{
		startTime = iReader.ReadUInt64();
		endTime = iReader.ReadUInt64();
		openPrice = iReader.ReadSingle();
		closePrice = iReader.ReadSingle();
		highPrice = iReader.ReadSingle();
		lowPrice = iReader.ReadSingle();
		volume = iReader.ReadSingle();
		tradeCount = iReader.ReadUInt32();
	}

//**************************************************************************************

	public void SaveToBinary(BinaryWriter iWriter)
	{
		iWriter.Write(startTime);
		iWriter.Write(endTime);
		iWriter.Write(openPrice);
		iWriter.Write(closePrice);
		iWriter.Write(highPrice);
		iWriter.Write(lowPrice);
		iWriter.Write(volume);
		iWriter.Write(tradeCount);
	}

//**************************************************************************************
	
}
