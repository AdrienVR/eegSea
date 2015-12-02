using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Group : WaveDescriptor
{
	private string name;
	private List<int> electrodes;
	private int F_min, F_max;

	public static Dictionary<int, string> ElecNames = new Dictionary<int, string>
	{
		{1, "O2"},
		{2, "P8"},
		{3, "T8"},
		{4, "FC6"},
		{5, "F8"},
		{6, "F4"},
		{7, "AF4"},
		{8, "AF3"},
		{9, "F3"},
		{10, "F7"},
		{11, "FC5"},
		{12, "T7"},
		{13, "P7"},
		{14, "O1"}
	};

	public Group (string n, List<int> elecs, int f1, int f2)
	{
		name = n;
		electrodes = new List<int>(elecs);
		F_min = Mathf.Min (f1, f2);
		F_max = Mathf.Max (f1, f2);
	}

    public override void UpdateRadius(float[] frequencies, float[] amplitudes)
    {
        m_radius = 0;//CenterValue(moyenneBasse[i], ecartTypeBasse[i], 0f, 1f, basseFrequence[i], 2f);
    }

    public float GetRadius()
    {
        return 0;
    }

	// ACCESSEURS
	public int getFMin()
	{
		return F_min;
	}

	public string getName()
	{
		return name;
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
		string eText = "";
		bool first = true;
		foreach (int e in electrodes)
		{
			if(!first)
				eText+=", "+ElecNames[e];
			else
				eText+=ElecNames[e];
			first=false;
		}
		string fText = "("+F_min.ToString () + " Hz->" + F_max.ToString () + " Hz)";
		return getName().ToUpper()+"\n"+ eText + "\n" + fText;
	}


}
