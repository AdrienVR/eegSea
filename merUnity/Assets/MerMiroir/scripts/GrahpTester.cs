using UnityEngine;
using System.Collections;

public class GrahpTester : MonoBehaviour 
{
	public AnimationCurve Curve;
	// Use this for initialization
	void Start () 
	{
		GraphManager.Instance.CreateNCurve(1);
		//GraphManager.Instance.CreateNCurve(1);
		for (int i=0; i<64; i++)
			Curve.AddKey (i, 0);
	}
	
	// Update is called once per frame
	void Update () 
	{
		//ClientBehavior.sensorVal[0]++;
		if (m_index > 63) {
			//fourrier = ClientBehavior.transFourrier.getFourrierTransformMod2 ();
			f=EEGDataManager.Instance.GetFT();
			m_index = 0;
		}
		//GraphManager.Instance.SetCurveValue(0,(float) fourrier[0,0]);
		if (f[m_index]>0.1f) Debug.Log("tests  "+m_index+" ; "+f[m_index]); 
		for (int i=0; i<64; i++)
		{
			Curve.RemoveKey(i);
			Curve.AddKey (i, f [i]);
		}
		GraphManager.Instance.SetCurveValue(0,(float) f[m_index++]);
		
	}
	
	double [,] fourrier;
	float []f;
	
	private int m_index = 64;
}
