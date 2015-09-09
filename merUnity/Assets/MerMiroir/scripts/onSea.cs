using UnityEngine;
using System.Collections;

public class onSea : MonoBehaviour {
	
	public PlusieurVagues vagues;
	public Vector3 seaPos;
	Vector3 restPos;

	// Use this for initialization
	void Start () {
		restPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		seaPos = vagues.CalculeImage (restPos);
		transform.position = seaPos;
	}
}
