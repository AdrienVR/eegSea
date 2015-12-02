using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Group : WaveDescriptor
{
	public string Name;
	public List<int> Electrodes;
	private int m_FreqMin, m_FreqMax;
	private float m_TMoyenne, m_TEcartType;

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
		Name = n;
		Electrodes = new List<int>(elecs);
		m_FreqMin = Mathf.Min (f1, f2);
		m_FreqMax = Mathf.Max (f1, f2);
	}

	public override void UpdateRadius(float[][] sensorVal)
    {
		float totalEnergy;
		foreach (int electrodeIndex in Electrodes)
		{
			float electrodeEnergy;
			for(int frequency=m_FreqMin;frequency<=m_FreqMax;frequency++)
			{
				electrodeEnergy+=sensorVal[electrodeIndex][frequency];
			}
			totalEnergy+=electrodeEnergy;
		}
		m_radius = totalEnergy;
    }

    public float GetRadius()
    {
		return CenterValue(m_TMoyenne, m_TEcartType, 0f, 1f, m_radius, 2f);
    }

	private float CenterValue(float moyenne, float ecartType, float valMin, float valMax, float valCapteur, float tolerance)
	{
		//centre la valeur en fonction de la moyenne et l'écart type
		if (valCapteur <= 0)
			return valMin;
		if (valCapteur <= (moyenne - tolerance * ecartType))
		{
			return valMin;
		}
		else if (valCapteur > (moyenne + tolerance * ecartType))
		{
			return valMax;
		}
		else
		{
			if (ecartType != 0) return (valMin + valMax) / 2f + (valMax - valMin) * (valCapteur - moyenne) / (ecartType * 2f * tolerance);
		}
		return (valMin + valMax) / 2f;
	}

	public void setMoyenne(float m)
	{
		m_TMoyenne = m;
	}

	public void setEcartType(float e)
	{
		m_TEcartType = e;
	}

	// ACCESSEURS
	public int getFMin()
	{
		return m_FreqMin;
	}

	public string getName()
	{
		return Name;
	}
	
	public int getFMax()
	{
		return m_FreqMax;
	}

	public List<int> getElecs()
	{
		return Electrodes;
	}

	public string getText()
	{
		string eText = "";
		bool first = true;
		foreach (int e in Electrodes)
		{
			if(!first)
				eText+=", "+ElecNames[e];
			else
				eText+=ElecNames[e];
			first=false;
		}
		string fText = "("+m_FreqMin.ToString () + " Hz->" + m_FreqMax.ToString () + " Hz)";
		return getName().ToUpper()+"\n"+ eText + "\n" + fText;
	}


}
