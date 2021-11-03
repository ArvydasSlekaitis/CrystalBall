using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
	public enum Type{Buy, Sell};

	public Type type { get; }
	public Bot bot { get; }
	public float price { get; }

//**************************************************************************************

	public Order(Type iType, Bot iBot, float iPrice)
	{
		if(iBot == null)
			throw new System.ArgumentNullException("Parameter must be not null", "iBot");

		if(iPrice <= 0)
			throw new System.ArgumentException("Parameter must be greater than one", "iPrice:" + iPrice);

		type = iType;
		bot = iBot;
		price = iPrice;
	}

//**************************************************************************************	
}
