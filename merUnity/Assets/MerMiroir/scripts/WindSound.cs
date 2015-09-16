using UnityEngine;
using System.Collections;


public class WindSound : MonoBehaviour {

	public float av;
	public float moyMax;

	void Start () { 
		av = 0.1f;
		moyMax = 10f;
	}
	
	// Update is called once per frame
	void Update () {
		av = 0.9f * av + 0.1f * Client.getTHF("THF3");
		if (av > 1) {
			moyMax = 0.995f * moyMax + 0.005f * av;
		}
		if (av > moyMax)
						moyMax = av;
		GetComponent<AudioSource>().volume = (av/moyMax)*(av/moyMax);
		GetComponent<AudioSource>().pitch = 0.85f+0.3f*GetComponent<AudioSource>().volume;
	} 
}

