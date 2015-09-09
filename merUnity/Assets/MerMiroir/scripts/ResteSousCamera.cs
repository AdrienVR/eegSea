using UnityEngine;
using System.Collections;

public class ResteSousCamera : MonoBehaviour {
	// pour que la grille suive la caméra sans provoquer d'artéfacts de mouvement
	public float step = 0.125f;
	public bool useStep = true;
	public bool hexagon = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (useStep)
		{
			if (hexagon)
			{
				float xStep = step * Mathf.Cos (30.0f * Mathf.Deg2Rad);
				float numberOfX = Mathf.Round (Camera.main.transform.position.x / xStep);
				float numberOfZ = Mathf.Round (Camera.main.transform.position.z / step);
				if (numberOfX % 2 == 0)
				{
					numberOfZ += 0.5f;
				}
				transform.position = new Vector3(numberOfX *  xStep, 0.0f, numberOfZ * step);
			}
			else
			{
				float x = step * Mathf.Round(Camera.main.transform.position.x / step);
				float z = step * Mathf.Round(Camera.main.transform.position.z / step);
				transform.position = new Vector3(x, 0.0f, z);
			}
		}
		else
		{
			transform.position = new Vector3(Camera.main.transform.position.x, 0.0f, Camera.main.transform.position.z);
		}

	}
}
