using UnityEngine;
using System.Collections;

public class openingSpecular : MonoBehaviour {
	public CameraOscillations cam;
	Color targetColor;

	// Use this for initialization
	void Start () {
        m_light = GetComponent<Light>();
        transform.eulerAngles=new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y+180f,transform.eulerAngles.z);
		targetColor = m_light.color;
	}
	
	// Update is called once per frame
	void Update ()
    {
        m_light.color = Mathf.Sqrt(cam.speed / cam.maxSpeed) * targetColor;
	}

    private Light m_light;
}
