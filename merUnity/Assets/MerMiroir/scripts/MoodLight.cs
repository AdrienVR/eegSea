using UnityEngine;
using System.Collections;

public class MoodLight : MonoBehaviour {
	float coeff=0.5f;
	public CameraOscillations cam;
	public float halftime=1f;
	public Color leftColor = new Color(0.2f, 0.2f, 0.2f, 1f);
	public Color rightColor = new Color(0.8f , 0.8f, 0.8f, 0.8f);
	// Use this for initialization
	void Start () {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
		//transform.eulerAngles.Set(transform.eulerAngles.x, cam.transform.eulerAngles.y,transform.eulerAngles.z);
		coeff = 0.5f;
		light.color = (leftColor * coeff + rightColor * (1f-coeff))*cam.speed/cam.maxSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		Client.moyenneGD ();
		float delta = Client.maxGD - Client.minGD;
		if (delta == 0) {
			light.color = (leftColor * coeff + rightColor * (1f-coeff))*cam.speed/cam.maxSpeed;
			return;
		}
		float alpha = Mathf.Min(Time.deltaTime / halftime, 1f);
		coeff = (1f-alpha)*coeff+alpha*(Client.maxGD - Client.valGaucheDroite) / delta;
		light.color = (leftColor * coeff + rightColor * (1f-coeff))*Mathf.Sqrt (cam.speed/cam.maxSpeed);
	}
}
