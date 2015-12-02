using UnityEngine;
using System.Collections;

public class GrahpTester : MonoBehaviour 
{
	public AnimationCurve Curve;
	public AnimationCurve Signal;

	// Use this for initialization
	void Start () 
	{
		GraphManager.Instance.CreateNCurve(1);
		//GraphManager.Instance.CreateNCurve(1);
		for (int i=0; i<64; i++) {
			Curve.AddKey (i, 0);
			Signal.AddKey (i, 0);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{

		//ClientBehavior.sensorVal[0]++;
		if (m_index > 63) {
			f=EEGDataManager.Instance.GetFT();
			signal=EEGDataManager.Instance.GetSignal();
			m_index = 0;
		}

		//if (f[m_index]>0.1f) Debug.Log("tests  "+m_index+" ; "+f[m_index]); 
		for (int i=0; i<64; i++)
		{
			Curve.RemoveKey(i);
			Curve.AddKey (i, f [0][i]);
			Signal.RemoveKey(i);
			Signal.AddKey (i, signal [0][i]);
		}
		GraphManager.Instance.SetCurveValue(0,(float) f[0][m_index++]);
		
	}
	float [][]f;
	float [][]signal;
	private int m_index = 64;
}
