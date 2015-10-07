using UnityEngine;
using System.Collections;

public class GrahpTester : MonoBehaviour 
{
	public float[] sensors;

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (sensors.Length > 1)
			sensors [0] = Mathf.Cos (Time.time * sensors [1]) * 50;

		UpdateCurveCounts();
		for(int i = 0;i < sensors.Length ; i++)
		{
			//Debug.Log(sensors[i]);
			GraphManager.Instance.SetCurveValue(i, sensors[i]);
		}
	}

	private void UpdateCurveCounts()
	{
		if (m_currentLength != sensors.Length) 
		{
			m_currentLength = sensors.Length;
			if (m_currentLength > 0)
				GraphManager.Instance.CreateNCurve (m_currentLength);
		}
	}

	private int m_currentLength = -1;
}
