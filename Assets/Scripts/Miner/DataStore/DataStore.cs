using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataStore 
{
	protected MonoBehaviour mono;
	List<Consolidator> consolidators;
	
	public ushort pairID { get; }
	public ulong validTill { get; protected set; }

	public abstract IEnumerator Update(ulong iTimeNow); 
	public int GetListinerCount() { return consolidators.Count; }

//**************************************************************************************

	public DataStore(MonoBehaviour iMono, ushort iPairID)
	{
		if(iMono == null)
			throw new System.ArgumentException("Parameter cannot be null", "iMono");

		mono = iMono;
		pairID = iPairID;
		consolidators = new List<Consolidator>();
	}

//**************************************************************************************

	public void AddListiner(Consolidator iConsolidator)
	{
		if(iConsolidator == null)
			throw new System.ArgumentException("Parameter cannot be null", "iConsolidator");

		if(validTill != 0)
			iConsolidator.Consolidate();

		consolidators.Add(iConsolidator);
	}

//**************************************************************************************

	public virtual void RemoveListiner(Consolidator iConsolidator)
	{
		consolidators.Remove(iConsolidator);
	}

//**************************************************************************************	

	protected void UpdateListiners()
	{
			for(int i=0; i<consolidators.Count; i++)
			{
				try
				{
					consolidators[i].Consolidate();
				}
				catch (System.Exception ex)
				{
					Debug.Log("Could not update listener: " + ex.ToString());
				}		
			}	
	}

//**************************************************************************************

	public bool IsValid(ulong iTimeNow)
	{
		return iTimeNow < validTill;
	}

//**************************************************************************************
}
