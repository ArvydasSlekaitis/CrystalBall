using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class OrderBookDataStore : DataStore 
{
	public List<OrderBookEntry> bids { get; private set; }
	public List<OrderBookEntry> asks { get; private set; }

	bool isUpdating = false;

//**************************************************************************************

	public OrderBookDataStore(MonoBehaviour iMono, ushort iPairID) : base(iMono, iPairID)
	{			
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
		BinanceOrderBookRequest dataRequest = new BinanceOrderBookRequest(Pair.ToString(pairID));
		yield return mono.StartCoroutine(dataRequest.Request());

		if(dataRequest.IsError())
		{
			isUpdating = false;
			yield break;
		}

		bids = dataRequest.GetBids();
		asks = dataRequest.GetAsks();

		#endif
		
		#if MINER 
		SaveData();
		#endif

		#if BACKSTAGE
		if(LoadData())
		{
			validTill = iTimeNow + 30000; // 30 seconds
			UpdateListiners();
		}
		#else
		validTill = iTimeNow + 30000; // 30 seconds
		UpdateListiners();
		#endif


		isUpdating = false;
		
		yield return null;
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
		string filename = Path.Combine(path, "OrderBook_" + MainController.GetBackstageTick().ToString() + "_" + Pair.ToString(pairID));

		try
		{
			if(!Directory.Exists(path))
				Directory.CreateDirectory (path);

			using(var writer = new BinaryWriter(File.OpenWrite(filename)))
			{
				if(bids == null || asks == null)
				{
					writer.Write((int)0);
					writer.Write((int)0);
				}
				else
				{
					writer.Write(bids.Count);
					for(int i=0; i<bids.Count; i++)
						bids[i].SaveToBinary(writer);
					writer.Write(asks.Count);
					for(int i=0; i<asks.Count; i++)
						asks[i].SaveToBinary(writer);	
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
		string filename = Path.Combine(path, "OrderBook_" + MainController.GetBackstageTick().ToString() + "_" + Pair.ToString(pairID));

		try
		{
			if(!File.Exists(filename))
				return false;
		
			if(bids==null)
				bids = new List<OrderBookEntry>();
			else
				bids.Clear();

			if(asks==null)
				asks = new List<OrderBookEntry>();
			else
				asks.Clear();	

			using(var reader = new BinaryReader(File.OpenRead(filename)))
			{
				int bidsCount = reader.ReadInt32();
				for(int i=0; i<bidsCount; i++)
					bids.Add(new OrderBookEntry(reader));

				int asksCount = reader.ReadInt32();
				for(int i=0; i<asksCount; i++)
					asks.Add(new OrderBookEntry(reader));

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
