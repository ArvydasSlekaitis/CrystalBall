using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consolidator 
{
	protected ulong validTill;

	public int listeners { get; private set; }

	public abstract void Consolidate();
	public abstract void Release();

	public void AddListiner() { listeners++; }
	public void RemoveListiner() { listeners--; }

//**************************************************************************************

	public bool IsValid(ulong iTimeNow)
	{
		return iTimeNow < validTill;
	}

//**************************************************************************************	
}
