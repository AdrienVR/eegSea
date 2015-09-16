using UnityEngine;
using System.Collections;

public class openingSpecular : MonoBehaviour {
	public CameraOscillations cam;
	Color targetColor;

	// Use this for initialization
	void Start () {
		transform.eulerAngles=new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y+180f,transform.eulerAngles.z);
		targetColor = GetComponent<Light>().color;
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Light>().color = Mathf.Sqrt(cam.speed / cam.maxSpeed) * targetColor;
	}
}
