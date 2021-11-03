using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortfolioManager 
{
    MonoBehaviour mono;
 
    // string <= asset
    Dictionary<string, List<Order>> orders;

    // string <= asset name; float <= quantity
    Dictionary<string, float> assets;

    BinanceTradingRulesRequest tradingRules;

//**************************************************************************************

    public PortfolioManager(MonoBehaviour iMono)
    {
        if(iMono == null)
			throw new System.ArgumentException("Parameter cannot be null", "iMono");

        mono = iMono;
        orders = new Dictionary<string, List<Order>>();
        assets = new Dictionary<string, float>(); 
    }

//**************************************************************************************

    public void PlaceBuyOrder(Bot iBot, float iPrice)
    {
        Debug.Log("PlaceBuyOrder. PairID:" + iBot.GetPairID() + " OrderType:Buy Price:" + iPrice);

        if(iBot.GetLifetimePerformance()<=0.0f)
            return;

        string asset = Pair.ToSourceAsset(iBot.GetPairID());

        // We do not have currency to fulfill such order
        if(!assets.ContainsKey(asset) || assets[asset]<=0.0f)
            return;

        Order order = new Order(Order.Type.Buy, iBot, iPrice);
  
        if(!orders.ContainsKey(asset))
            orders.Add(asset, new List<Order>());

        Debug.Log("Order placed");

        orders[asset].Add(order);
    }

//**************************************************************************************

    public void PlaceSellOrder(Bot iBot, float iPrice)
    {
        Debug.Log("PlaceSellOrder. PairID:" + iBot.GetPairID() + " OrderType:Sell Price:" + iPrice);

        if(iBot.GetLifetimePerformance()<=0.0f)
            return;

        string asset = Pair.ToDestinationAsset(iBot.GetPairID());

        // We do not have currency to fulfill such order
        if(!assets.ContainsKey(asset) || assets[asset]<=0.0f)
            return;

        Order order = new Order(Order.Type.Sell, iBot, iPrice);

        if(!orders.ContainsKey(asset))
            orders.Add(asset, new List<Order>());

        Debug.Log("Order placed");

        orders[asset].Add(order);
    }    

//**************************************************************************************

    public IEnumerator UpdateAssets()
    {
        assets.Clear();

        #if !BACKSTAGE
        BinanceAssetsRequest assetRequest = new BinanceAssetsRequest();

        yield return mono.StartCoroutine(assetRequest.Request(GetTimeNow()));

        if(!assetRequest.IsError() && assetRequest.GetAssets()!=null)
        {
            assets = assetRequest.GetAssets();
            Debug.Log("Assets updated");
        }
        #endif

        if(Application.isEditor)
        {
            if(assets.ContainsKey("USDT"))
                assets["USDT"] = 500.0f;  

            if(assets.ContainsKey("BTC"))    
                assets["BTC"] = 0.01f;  

            if(assets.ContainsKey("BNB"))    
                assets["BNB"] = 10.0f;  
        }

        yield return null;
    }

//**************************************************************************************

    public IEnumerator Update(BotManager iBotManager, ulong iServerTime)
    {
        if(iBotManager == null)
			throw new System.ArgumentException("Parameter cannot be null", "botManager");

        // Trading Rules
        if(tradingRules == null)
        {
            tradingRules = new BinanceTradingRulesRequest();
            yield return mono.StartCoroutine(tradingRules.Request());

            if(tradingRules.IsError())
                throw new System.Exception("Trading Rules could not be retrieved.");  
        }

        // Proccess Orders
        foreach(KeyValuePair<string, List<Order>> assetOrders in orders)
        {
            if(assetOrders.Value==null)
                continue;

            Dictionary<ushort, float> aggregatedToPairs = AggregateToPairs(iBotManager, assetOrders.Key, assetOrders.Value);

            foreach(KeyValuePair<ushort, float> aggregatedPair in aggregatedToPairs)
            {
                try
                {
                    if(aggregatedPair.Value == 0.0f)
                        continue;

                    bool isBuy = aggregatedPair.Value > 0;
                    float aggregatedPercentage = isBuy ? aggregatedPair.Value : -aggregatedPair.Value;

                    Debug.Log("PairID: " + aggregatedPair.Key + (isBuy ? " Buy:" : " Sell:") + aggregatedPercentage);
                    Debug.Log("Using Asset:" + assetOrders.Key + " with balance:" + assets[assetOrders.Key]);
                    float orderValue = assets[assetOrders.Key]*aggregatedPercentage;
                    string orderPairName = Pair.ToString(aggregatedPair.Key);
                    float minOrderValue = tradingRules.GetMinValue()[orderPairName];
                    float minOrderQuantity = tradingRules.GetMinQuantity()[orderPairName];                  
                    float orderQuantity = orderValue/assetOrders.Value[0].price;
                    float orderPrice = assetOrders.Value[0].price;

                    Debug.Log("Dedicated money:" + orderValue);
                    Debug.Log("Min Order Value:" + minOrderValue);
                    Debug.Log("Min Quantity:" + minOrderQuantity);
                    Debug.Log("Order type: " + (isBuy ? "BUY" : "SELL"));
                    Debug.Log("Pair name: " + orderPairName);
                    Debug.Log("Price:" + orderPrice);
                    Debug.Log("Quantity: " + orderQuantity);  

                    if(orderValue <= minOrderValue)
                    {
                        Debug.Log("Order value too small");
                        continue;
                    }

                    if(orderQuantity < minOrderQuantity)
                    {
                        Debug.Log("Order quantity too small");
                        continue;
                    }

                    if(assets[assetOrders.Key]-orderValue <= minOrderValue || (assets[assetOrders.Key]-orderValue)/orderPrice <= minOrderQuantity)
                    {
                        orderValue = assets[assetOrders.Key];               
                        orderQuantity = orderValue/orderPrice;

                        Debug.Log("Final sell off");
                    }

                    BinanceOrderRequest r = new BinanceOrderRequest();
                    mono.StartCoroutine(r.Request(GetTimeNow(), orderPairName, isBuy, orderQuantity, orderPrice));
                }
                catch(System.Exception ex)
                {
                    Debug.Log("Could not precompute order:" + ex.ToString());
                    continue;
                }
            }
        }

        orders.Clear();

        yield break;
    }

//**************************************************************************************

    // ushort <= PairID; float <= Percentage of asset
    Dictionary<ushort, float> AggregateToPairs(BotManager iBotManager, string iAssetName, List<Order> iOrders)
    {
        Dictionary<ushort, float> aggregated = new Dictionary<ushort, float>();

        try
        {
            float sumOfPerformance = -1.0f;

            if(sumOfPerformance < 0.0f)
                sumOfPerformance = iBotManager.GetLifetimePerformanceSum(iAssetName);

            if(sumOfPerformance == 0)
                throw new System.ArithmeticException("SumOfPerformance can't be equal to zero");

            for(int i=0; i<iOrders.Count; i++)
            {
                if(!aggregated.ContainsKey(iOrders[i].bot.GetPairID()))
                    aggregated.Add(iOrders[i].bot.GetPairID(), 0.0f);

                switch (iOrders[i].type)
                {
                    case Order.Type.Buy:
                        aggregated[iOrders[i].bot.GetPairID()] += iOrders[i].bot.GetLifetimePerformance() / sumOfPerformance;    
                        break;

                    case Order.Type.Sell:
                        aggregated[iOrders[i].bot.GetPairID()] -= iOrders[i].bot.GetLifetimePerformance() / sumOfPerformance;       
                        break;    
                }  
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Could not AggregateToPairs:" + ex.ToString());
            aggregated.Clear();
        }
    
        return aggregated;
    }

//**************************************************************************************

    public ulong GetTimeNow()
    {
		return (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
    }

//**************************************************************************************
}
