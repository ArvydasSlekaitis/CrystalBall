using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BotManager
{
	List<Bot> bots;
	Miner miner;
	PortfolioManager portfolioManager;
	ushort saveTicksDelay;
	const ushort kSaveTicksDelay = 14400;

//**************************************************************************************

	public BotManager(Miner iMiner, PortfolioManager iPortfolioManager)
	{
		if(iMiner == null)
			throw new System.ArgumentNullException("Parameter cannot be NULL", "iMiner");

		if(iPortfolioManager == null)
			throw new System.ArgumentNullException("Parameter cannot be NULL", "iPortfolioManager");

		miner = iMiner;	
		portfolioManager = iPortfolioManager;		
		bots = new List<Bot>();
		SpawnBots();
	}

//**************************************************************************************	

	public void Update(ulong iTimeNow)
	{
		// Execute bots
		int executed = Execute(iTimeNow);

		// Save BotsData
		if(saveTicksDelay > 0)
			saveTicksDelay--;
		else
			if(Save())
				saveTicksDelay = kSaveTicksDelay;

		#if CSV
		ExportBotsToCSV();
		ExportIndicatorsToCSV();
		#endif
	
		Debug.Log("BotManager update complete (total:" + bots.Count + "; Executed:" + executed.ToString() + ")");	
	}

//**************************************************************************************

	int Execute(ulong iTimeNow)
	{
		int executed = 0;

		for(int i=0; i<bots.Count; i++)
		{
			try
			{
				if(bots[i].Execute(iTimeNow))
					executed++;
			}
			catch (System.Exception ex)
			{
				Debug.Log("Error executing bot:" + ex.ToString());
			}
		}

		return executed;
	}

//**************************************************************************************

	void SpawnBots()
	{
		for(ushort pairID=0; pairID<=Pair.GetMaxID(); pairID++)
			if(Pair.IsEnabled(pairID))
			{
				try
				{
					Bot b = new Bot(miner, portfolioManager, pairID);	
					if(b!=null)
						bots.Add(b);
				}
				catch (System.Exception ex) { Debug.Log("Could not create new Bot_" + Pair.ToString(pairID).ToString() + ":" + ex.ToString()); }
			}
	}

//**************************************************************************************

	public bool Save()
	{
		for(int i=0; i<bots.Count; i++)
		{
			try
			{
				bots[i].Save();
			}
			catch (System.Exception ex)
			{
				Debug.Log("Error saving bot internal data: " + ex.ToString());
				return false;
			}
		}	

		return true;
	}

//**************************************************************************************

	public float GetLifetimePerformanceSum(string iAsset)
	{
		float sum = 0.0f;

		for(int i=0; i<bots.Count; i++)
		{
			try
			{
				if(bots[i].GetCurrentAsset() == iAsset)
					sum += bots[i].GetLifetimePerformance();
			}
			catch (System.Exception ex)
			{
				Debug.Log("Could not calculate LifetimePerformanceSum:" + ex.ToString());
			}
		}
	
		return sum;
	}

//**************************************************************************************

	#if CSV
	public void ExportBotsToCSV()
	{
		string path = Path.Combine(Application.persistentDataPath, "CSV");
		string csvFile = Path.Combine(path, "Bots.csv");

		try
		{
			if(!Directory.Exists(path))
				Directory.CreateDirectory (path);

			using(var writer = new StreamWriter(File.OpenWrite(csvFile)))
			{
				writer.WriteLine("PairID,NumberOfTrades,ProductOfPerformance,LifetimePerformance");
				for(int i=0; i<bots.Count; i++)
					bots[i].SaveToCSV(writer);
				
				writer.Close();
			}
		}
		catch (System.Exception ex)
		{
			if(File.Exists(csvFile))
				File.Delete(csvFile);

			Debug.Log("Could not save Bots.csv:" + ex.ToString());
		}
	}
	#endif
//**************************************************************************************

	#if CSV
	public void ExportIndicatorsToCSV()
	{
		string path = Path.Combine(Application.persistentDataPath, "CSV");
		string csvFile = Path.Combine(path, "Indicators.csv");

		try
		{
			if(!Directory.Exists(path))
				Directory.CreateDirectory (path);

			using(var writer = new StreamWriter(File.OpenWrite(csvFile)))
			{
				writer.WriteLine("PairID,IndicatorID,ConfigurationID,NumberOfTrades,ProductOfPerformance,LifetimePerformance");
				for(int i=0; i<bots.Count; i++)
					bots[i].SaveIndicatorsToCSV(writer);
				
				writer.Close();
			}
		}
		catch (System.Exception ex)
		{
			if(File.Exists(csvFile))
				File.Delete(csvFile);

			Debug.Log("Could not save Indicators.csv:" + ex.ToString());
		}
	}
	#endif

//**************************************************************************************

	
}
