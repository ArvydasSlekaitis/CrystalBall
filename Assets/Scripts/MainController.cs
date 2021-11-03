using System.Collections;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class MainController : MonoBehaviour 
{
	Miner miner;
	BotManager botManager;
	PortfolioManager portfolioManager;

	#if MINER || BACKSTAGE
	static int backstageTick = 0;
	static int kMaxBackstageTick = 80000;
	public static int GetBackstageTick() { return backstageTick; }
	#endif

//**************************************************************************************

	void Start () 
	{
		#if BACKSTAGE && MINER
		Debug.Log("Can't run BACKSTAGE and MINER together");
		Application.Quit();
		return;
		#endif

		#if BACKSTAGE
		if(kMaxBackstageTick <= 0 || !Application.isEditor) 
			Debug.Log("kMaxBackstageTick <= 0 || !Application.isEditor");
			Application.Quit();		
		#endif

		if(!Application.isEditor)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 3;
		}	

	/*	miner = new Miner(this);
		portfolioManager = new PortfolioManager(this);
		botManager = new BotManager(miner, portfolioManager);

		StartCoroutine(MasterUpdate());*/

		StartCoroutine(DataParsing());
	}

//**************************************************************************************

	IEnumerator MasterUpdate()
	{
		while(true)
		{
			Debug.Log("********************"+ System.DateTime.UtcNow.ToString() + "********************");

			if(IsStopped())
			{
				botManager.Save();

				Debug.Log("IsStopped: quiting.");
				Application.Quit();
				yield break;
			}

			if(IsPaused())
			{
				botManager.Save();
				
				Debug.Log("IsPaused: waiting");
				yield return new WaitForSeconds(60.0f); 
			}

			float updateStartTime = Time.realtimeSinceStartup;
			ulong timeNow = (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
		
			Coroutine pmUpdateAssetsCoroutine = StartCoroutine(portfolioManager.UpdateAssets());
			Coroutine minerCoroutine = StartCoroutine(miner.Update(timeNow));	

			yield return minerCoroutine;
			yield return pmUpdateAssetsCoroutine;

			botManager.Update(timeNow);

			yield return StartCoroutine(portfolioManager.Update(botManager, 0));

			Debug.Log("Update complete in: " + (Time.realtimeSinceStartup - updateStartTime).ToString());

			float timeLeft = 15.0f - (Time.realtimeSinceStartup - updateStartTime);

			#if !BACKSTAGE
			if(timeLeft > 0)
				yield return new WaitForSeconds(timeLeft); 
			#endif

			#if MINER || BACKSTAGE
			backstageTick++;
			#endif	

			#if BACKSTAGE
			if(backstageTick > kMaxBackstageTick)
			{
				Debug.Log("Backstage testing complete");
				yield break;
			}
			#endif
		}
	}

//**************************************************************************************

	bool IsPaused()
	{
		string fileName = Path.Combine(Application.persistentDataPath, "PAUSE.order");
		return File.Exists(fileName);		
	}

//**************************************************************************************

	bool IsStopped()
	{
		string fileName = Path.Combine(Application.persistentDataPath, "STOP.order");
		return File.Exists(fileName);		
	}	

//**************************************************************************************

	IEnumerator DataParsing()
	{
		const short kMaxProfitTime = 90;

		float updateStartTime = Time.realtimeSinceStartup;
		ulong timeNow = SharedFunctions.DateTimeToUnix(System.DateTime.UtcNow);
		
		//const string inputPath = "Assets/RawData/BTC-USD.csv";
		const string inputPath = "Assets/RawData/EUR-USD.csv";
	    StreamReader reader = new StreamReader(inputPath); 

		List<CandlestickEntry> candlestickData = new List<CandlestickEntry>();

		// read first line
		reader.ReadLine();

		while(!reader.EndOfStream)
		{
			string line = reader.ReadLine();

			string[] contents = line.Split(',');
			DateTime date = DateTime.Parse(contents[0]);
			DateTime dateBegin = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
			DateTime dateEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
			// BTC - USD
			//CandlestickEntry entry = new CandlestickEntry(SharedFunctions.DateTimeToUnix(dateBegin), SharedFunctions.DateTimeToUnix(dateEnd), float.Parse(contents[1]), float.Parse(contents[4]), float.Parse(contents[2]), float.Parse(contents[3]), float.Parse(contents[6]), 1);
			// EUR - USD
			CandlestickEntry entry = new CandlestickEntry(SharedFunctions.DateTimeToUnix(dateBegin), SharedFunctions.DateTimeToUnix(dateEnd), float.Parse(contents[2]), float.Parse(contents[1]), float.Parse(contents[3]), float.Parse(contents[4]), 1, 1);
			candlestickData.Add(entry);
		}

		// Only for EUR / USD
		candlestickData.Reverse();

		// Cacluate close price
		float[] closePrice = new float[candlestickData.Count];
		for (int i = 0; i < candlestickData.Count; i++)
			closePrice[i] = candlestickData[i].closePrice;

		// Calcluate close price change -> Will be used to calculate discount rate
		float[] closePriceChange = new float[candlestickData.Count];
		for (int i = 1; i < candlestickData.Count-1; i++)
			closePriceChange[i] = candlestickData[i].closePrice / candlestickData[i-1].closePrice;
		float closePriceChangeStdDev = SharedFunctions.CalculateStandardDeviation(closePriceChange, SharedFunctions.CalculateArithmeticMean(closePriceChange));
		
		// Calculate cost of capital
		const float kAverageTradeMargin = 0.1f;
		float kDailyCostOfCapital = closePriceChangeStdDev / kAverageTradeMargin; 
	//	Debug.Log("Close price change std dev: " + closePriceChangeStdDev + " Cost of capital: "+ kDailyCostOfCapital);

		// Calulcate profits		
		float[] profits = new float[candlestickData.Count];
		for (int i = 0; i < candlestickData.Count - kMaxProfitTime; i++)
			profits[i] = FinanceFunctions.CalculateFutureProfits(closePrice, kDailyCostOfCapital, (short)i, kMaxProfitTime);
		float profitsStdDev = SharedFunctions.CalculateStandardDeviation(profits, SharedFunctions.CalculateArithmeticMean(profits));

		// Calculate indicators for defines period
		const int periods = 30;
		float[] meanToStdDevIndicator = new float[candlestickData.Count];
		float[] rsiIndicator = new float[candlestickData.Count];
		for (int i = periods; i < candlestickData.Count; i++)
		{
			CandlestickEntry[] rawData = new CandlestickEntry[periods];
			candlestickData.CopyTo(i-periods, rawData, 0, periods);
			ConsolidatedCandlestick consolidatedCandlestick = new ConsolidatedCandlestick(rawData);
			
			meanToStdDevIndicator[i] = FinanceFunctions.GetIndicatorValue_MeanToStdDev(consolidatedCandlestick.averagePrice.GetArithmeticMean().mean, consolidatedCandlestick.averagePrice.GetArithmeticMean().standardDeviation, (candlestickData[i].highPrice + candlestickData[i].lowPrice) / 2.0f );
			rsiIndicator[i] = consolidatedCandlestick.GetRSI().last;
		}

		// Output all results
		const string outputPath = "Assets/Output/Output.csv";
	    StreamWriter writer = new StreamWriter(outputPath); 

		writer.WriteLine("RSI,MeanToStdDev,PL");

		for(int i=periods; i<candlestickData.Count - kMaxProfitTime; i++)
		{
			writer.WriteLine(rsiIndicator[i].ToString() + "," + meanToStdDevIndicator[i].ToString() + "," + GetDecision(profits[i], profitsStdDev));
		}
		Debug.Log(candlestickData.Count);

		yield return null;
	}

 //**************************************************************************************

	string GetDecision(float iProfitLoss, float iProfitStdDev)
	{
		float kBuySellTreshold = iProfitStdDev;
		float kStronBuySellTreshold = iProfitStdDev*2.0f;

		if(iProfitLoss <= -kStronBuySellTreshold)
			return "StrongSell";

		if(iProfitLoss >= kStronBuySellTreshold)
			return "StrongBuy";

		if(iProfitLoss <= -kBuySellTreshold)
			return "Sell";

		if(iProfitLoss >= kBuySellTreshold)
			return "Buy";

		if(iProfitLoss >= 0)
			return "WeakBuy";

		if(iProfitLoss < 0)
			return "WeakSell";

		return "Undefined";
	}

//**************************************************************************************
}
