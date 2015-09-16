using UnityEngine;
using System.Collections;

public class moodSpecular : MonoBehaviour {
	float coeff=0.5f;
	public float halftime=1f;
	public float leftIntensity = 1f;
	public float rightIntensity = 1f;

	// Use this for initialization
	void Start () {
		coeff = 0.5f;
		GetComponent<Light>().intensity = (leftIntensity * coeff + rightIntensity * (1f-coeff));
	}
	
	// Update is called once per frame
	void Update () {
		Client.moyenneGD ();
		float delta = Client.maxGD - Client.minGD;
		float alpha = Mathf.Min(Time.deltaTime / halftime, 1f);
		if (delta == 0) {
			coeff=(1f-alpha)*coeff +alpha*0.5f;
			GetComponent<Light>().intensity = (leftIntensity * coeff + rightIntensity * (1f-coeff));
			return;
		}
		coeff = (1f-alpha)*coeff+alpha*(Client.maxGD - Client.valGaucheDroite) / delta;
		GetComponent<Light>().intensity = (leftIntensity * coeff + rightIntensity * (1f-coeff));
	
	}
}
