using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMax 
{
	public float max { get; }
	public float min { get; }
	public float maxLocation { get; }
	public float minLocation { get; }

//**************************************************************************************

	public MinMax(float[] iRawData)
	{
		if(iRawData.Length < 1)
			return;

		min = max = iRawData[0];
		minLocation = maxLocation = 0;

		for(int i=1; i<iRawData.Length; i++)
		{
			// Min & location
			if(iRawData[i] < min)
			{
				minLocation = (float)i / (float)(iRawData.Length-1);
				min = iRawData[i];
			}

			// Max location
			if(iRawData[i] > max)
			{
				maxLocation = (float)i / (float)(iRawData.Length-1);
				max = iRawData[i];	
			}		
		}
	}

//**************************************************************************************

	public MinMax(float[] iMin, float[] iMax)
	{
		if(iMin.Length != iMax.Length)
			throw new System.ArgumentException("iMin.Length != iMax.Length");
			
		if(iMin.Length < 1)
			return;

		min = iMin[0];
		max = iMax[0];
		minLocation = maxLocation = 0;

		for(int i=0; i<iMin.Length; i++)
		{
			// Min & location
			if(iMin[i] < min)
			{
				minLocation = (float)i / (float)(iMin.Length-1);
				min = iMin[i];
			}

			// Max location
			if(iMax[i] > max)
			{
				maxLocation = (float)i / (float)(iMin.Length-1);
				max = iMax[i];	
			}		
		}
	}

//**************************************************************************************
}
