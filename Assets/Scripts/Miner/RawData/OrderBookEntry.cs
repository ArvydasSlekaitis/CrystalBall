using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OrderBookEntry 
{
	public float price { get; }
	public float quantity { get; }

//**************************************************************************************

	public OrderBookEntry(float iPrice, float iQuantity)
	{
		price = iPrice;
		quantity = iQuantity;
	}

//**************************************************************************************

	public OrderBookEntry(BinaryReader iReader) 
	{
		price = iReader.ReadSingle();
		quantity = iReader.ReadSingle();
	}

//**************************************************************************************

	public void SaveToBinary(BinaryWriter iWriter)
	{
		iWriter.Write(price);
		iWriter.Write(quantity);
	}

//**************************************************************************************	
}
