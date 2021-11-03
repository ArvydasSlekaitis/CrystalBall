using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Trade
{
	public float buyPrice;
	public float sellPrice;
	public ulong buyTime;
	public ulong sellTime;

//**************************************************************************************

	public Trade(float iBuyPrice, float iSellPrice, ulong iBuyTime, ulong iSellTime) 
	{ 
		buyPrice = iBuyPrice; 
		sellPrice = iSellPrice; 
		buyTime = iBuyTime; 
		sellTime = iSellTime; 
	}

//**************************************************************************************

	public static void SaveToBinary(string iPath, string iFileName, List<Trade> iTrades)
	{
		string fullFileName = Path.Combine(iPath, iFileName);

		try
		{
			if(!Directory.Exists(iPath))
				Directory.CreateDirectory (iPath);

			using(var writer = new BinaryWriter(File.OpenWrite(fullFileName)))
			{
				writer.Write((uint)iTrades.Count);
                for(int i=0; i<iTrades.Count; i++)
                {
                    writer.Write(iTrades[i].buyPrice);
                    writer.Write(iTrades[i].sellPrice);	
                    writer.Write(iTrades[i].buyTime);
                    writer.Write(iTrades[i].sellTime);
                }       

				writer.Close();
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("Could not save " + fullFileName + ": " + ex.ToString());
		}
	}

//**************************************************************************************

	public static List<Trade> LoadFromBinary(string iPath, string iFileName)
	{
		List<Trade> trades = new List<Trade>();

		string fullFileName = Path.Combine(iPath, iFileName);
		
		try
		{
			if(!Directory.Exists(iPath))
				Directory.CreateDirectory (iPath);

            if(!File.Exists(fullFileName))
			{
				Debug.Log("Could not load trades from: " + fullFileName);
               	return trades;
			}

			using(var reader = new BinaryReader(File.OpenRead(fullFileName)))
			{
				uint numberOfTrades = reader.ReadUInt32();
				Debug.Log("Loading " + numberOfTrades.ToString() + " trades from: " + fullFileName);

				for(uint i=0; i<numberOfTrades; i++)
				{
					float buyPrice = reader.ReadSingle();
                    float sellPrice = reader.ReadSingle();
                    ulong buyTime = reader.ReadUInt64();
                    ulong sellTime = reader.ReadUInt64();
                    trades.Add(new Trade(buyPrice, sellPrice, buyTime, sellTime));
                }
				
				reader.Close();
			}

			return trades;
		}
		catch (System.Exception ex)
		{
			Debug.Log("Could not parse " + fullFileName + ": " + ex.ToString());
			return trades;
		}
	}	

//**************************************************************************************
}
