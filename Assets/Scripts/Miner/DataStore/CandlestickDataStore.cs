using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CandlestickDataStore : DataStore 
{
	const ushort kTargetPeriods = 500;
	const ulong kPeriodDuration = (ulong)3600000;
	const ulong kDuration = kPeriodDuration * kTargetPeriods;
	const string kIntervalName = "1h";

	public List<CandlestickEntry> data { get; private set; }
	bool isUpdating = false;

//**************************************************************************************

	public CandlestickDataStore(MonoBehaviour iMono, ushort iPairID) : base(iMono, iPairID)
	{			
		data = new List<CandlestickEntry>();
	}

//**************************************************************************************

	public override IEnumerator Update(ulong iTimeNow)
	{
		if(isUpdating)
		{
			Debug.Log("Previous Update is not finished");
			yield break;
		}
		isUpdating = true;

		#if !BACKSTAGE
		//Wait for one interval before updating
		if(iTimeNow - GetLastEntryTime() < kPeriodDuration)
		{
			isUpdating = false;
			yield break;
		}

		ulong requestStartTime = iTimeNow - kDuration;
		ulong lastEntryTime = GetLastEntryTime();

		if(lastEntryTime > requestStartTime)
			requestStartTime = lastEntryTime;

		BinanceCandlestickRequest dataRequest = new BinanceCandlestickRequest(Pair.ToString(pairID), kIntervalName, requestStartTime, iTimeNow);
		yield return mono.StartCoroutine(dataRequest.Request());

		if(dataRequest.IsError())
		{
			isUpdating = false;			
			Debug.Log("Could not retrieve candlestick data from Binance for " + Pair.ToString(pairID));
			yield break;
		}

		List<CandlestickEntry> requestRawData = dataRequest.GetRawData();

		if(requestRawData.Count <= 0)
		{
			Debug.Log("Could not retrieve candlestick data from Binance for " + Pair.ToString(pairID));
			isUpdating = false;
			yield break;
		}

		//Remove same amount of old entries
		if(data.Count >= requestRawData.Count)
			data.RemoveRange(0, requestRawData.Count);

		data.AddRange(requestRawData);

		//We prefer complete data (binanace always returns based on start time, even though period is not finished)
		if(data.Count < kTargetPeriods - 1 || data.Count > kTargetPeriods + 1)
		{
			Debug.Log("Invalid raw data length: " + data.Count + " instead of:" + kTargetPeriods);
			data.Clear();
			isUpdating = false;
			validTill = 0;
		
			#if MINER 
			SaveData();
			#endif

			yield break;
		}

		if(data[data.Count-1].endTime - data[0].startTime > kDuration)
		{
			Debug.Log("Invalid raw data duration: " + (data[data.Count-1].endTime - data[0].startTime) + " Instead of " + kDuration);
			data.Clear();
			isUpdating = false;
			validTill = 0;

			#if MINER
			SaveData();
			#endif

			yield break;
		}
		#endif

		#if MINER
		SaveData();
		#endif

		#if BACKSTAGE
		if(LoadData())
		{
			validTill = iTimeNow + kPeriodDuration*2;
			UpdateListiners();
		}
		#else
		validTill = iTimeNow + kPeriodDuration*2;
		UpdateListiners();
		#endif

		isUpdating = false;
		
		yield return null;
	}

//**************************************************************************************

	public ulong GetLastEntryTime()
	{
		if(data.Count <= 0)
			return 0;
		else
			return data[data.Count-1].endTime;
	}

//**************************************************************************************

	public bool Is(ushort iPairID)
	{
		return pairID==iPairID;
	}

//**************************************************************************************

	#if MINER
	void SaveData()
	{
		string path = Path.Combine(Application.persistentDataPath, "MinerDataStore");
		string filename = Path.Combine(path, "Candlestick_" + MainController.GetBackstageTick().ToString() + "_" + Pair.ToString(pairID) + "_" + kIntervalName);

		try
		{
			if(!Directory.Exists(path))
				Directory.CreateDirectory (path);

			using(var writer = new BinaryWriter(File.OpenWrite(filename)))
			{
				if(data == null)
					writer.Write((int)0);
				else
				{
					writer.Write(data.Count);
					for(int i=0; i<data.Count; i++)
						data[i].SaveToBinary(writer);
				}
				writer.Close();
			}
		}
		catch (System.Exception ex)
		{
			if(File.Exists(filename))
				File.Delete(filename);

			Debug.Log("Could not save " + filename + ": " + ex.ToString());
		}
	}
	#endif

//**************************************************************************************

	#if BACKSTAGE
	bool LoadData()
	{
		string path = Path.Combine(Application.persistentDataPath, "MinerDataStore");
		string filename = Path.Combine(path, "Candlestick_" + MainController.GetBackstageTick().ToString() + "_" + Pair.ToString(pairID) + "_" + kIntervalName);

		try
		{
			if(!File.Exists(filename))
				return false;
		
			data.Clear();

			using(var reader = new BinaryReader(File.OpenRead(filename)))
			{
				int count = reader.ReadInt32();

				for(int i=0; i<count; i++)
					data.Add(new CandlestickEntry(reader));

				reader.Close();
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("Could not load " + filename + ": " + ex.ToString());
			return false;
		}

		return true;
	}
	#endif

//**************************************************************************************

}
