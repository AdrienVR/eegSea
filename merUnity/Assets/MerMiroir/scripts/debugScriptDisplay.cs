using UnityEngine;
using System.Collections;
using System;

public class debugScriptDisplay : MonoBehaviour 
{
	public long timeStartProgramDisplay = 0;
	public int numberValuesReceivedDisplay = 0;
	public double frequencyUDP = 0.0;
	public double frequencyTrame = 0.0;
	public double startSince = 0.0;

	// Use this for initialization
	void Start () 
	{
		timeStartProgramDisplay = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
	}
	
	// Update is called once per frame
	void Update () 
	{
		numberValuesReceivedDisplay =  unchecked((int)ClientBehavior.Instance.numberValuesReceived);

		startSince = ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeStartProgramDisplay) / 1000;

		frequencyUDP = numberValuesReceivedDisplay / (startSince)  ;
		frequencyTrame = frequencyUDP / 4.0;
	}
}
