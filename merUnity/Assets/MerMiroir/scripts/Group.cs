using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Group 
{
	private List<int> electrodes;
	private int F_min, F_max;

	public Group (List<int> elecs, int f1, int f2)
	{
		electrodes = elecs;
		F_min = Mathf.Min (f1, f2);
		F_max = Mathf.Max (f1, f2);
	}

	// ACCESSEURS
	public int getFMin()
	{
		return F_min;
	}
	
	public int getFMax()
	{
		return F_max;
	}

	public List<int> getElecs()
	{
		return electrodes;
	}

	public string getText()
	{
		string eText = "-";
		bool first = true;
		foreach (int e in electrodes)
		{
			if(!first)
				eText+=", "+e.ToString();
			else
				eText+=e.ToString();
			first=false;
		}
		string fText = "- ("+F_min.ToString () + " Hz->" + F_max.ToString () + " Hz)";
		return eText + fText;
	}


}
