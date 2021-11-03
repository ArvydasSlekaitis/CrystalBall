using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHA
{
	public float value { get; }

//**************************************************************************************

	// LHA - Angle between low and high. value [-1 = strong buy; 1 = strong sell]
	public LHA(float[] iLows, float[] iHigh)
	{
		if (iLows == null || iHigh == null || iLows.Length < 2 || iLows.Length != iHigh.Length)
			throw new System.ArgumentException("Invalid array");

		LinearRegression rLow = new LinearRegression(iLows);
		LinearRegression rHigh = new LinearRegression(iHigh);

		Vector2 vLow = new Vector2(1, rLow.slope);
		Vector2 vHigh = new Vector2(1, rHigh.slope);

		float angle = Vector2.Angle(vLow.normalized, vHigh.normalized);

		if(rHigh.slope < rLow.slope)
			angle = -angle;

		value = Mathf.Clamp(-angle/180.0f, -1.0f, 1.0f);
	}

//**************************************************************************************
}
