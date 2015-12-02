using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour {

	public float speed = 10.0F;
	public float rotationSpeed = 100.0F;

    void Update()
    {
        NormalMotion();
    }

    void NormalMotion()
	{
		float translation = Input.GetAxis("Vertical") * speed;
		float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
		translation *= Time.deltaTime;
		rotation *= Time.deltaTime;
		if (Input.GetAxis("Fire2")>0)
		{
			if (Input.GetAxis("Fire1")>0)
			{
				transform.Rotate (Input.GetAxis ("Vertical")* rotationSpeed * -0.01f,0,0);
			}
			else
			{
				transform.Translate(0,translation,0,Space.World);
			}
			float straff = Input.GetAxis("Horizontal") * 0.1f * speed;
			transform.Translate(straff,0,0);		
		}
		else
		{
			transform.Translate(0, 0, translation);
			transform.Rotate(0, rotation, 0, Space.World);
		}
	}
}
