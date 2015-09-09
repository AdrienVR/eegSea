using UnityEngine;
using System.Collections;

public class CameraOscillations : MonoBehaviour 
{
	float[] betaValues = new float[4];
	public float maxSpeed=10f;
	public float rotationMaxSpeed=10f;
	public float deltaRotationMax=20f;
	public float speed=0;
	public float openingTime=5f;
	public float closingTime=30f;
	public float oscillingTime=2f;
	float rotationYinit=0f;
	public float precision=1f; // à combien de degrés près on cherche à maintenir la direction
	float maxGauche=0f;
	float maxDroite=0f;
	float sgn=0f;
	float diff=0f;

	// Use this for initialization
	void Start () 
	{
		rotationYinit = transform.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float translation = speed;
		float rotation = rotationMaxSpeed;
		translation *= Time.deltaTime;
		rotation *= Time.deltaTime;
		float oldy = transform.position.y;
		transform.Translate (0f, 0f, translation);
		transform.Translate (0f, oldy - transform.position.y, 0f, Space.World);
		betaValues = Client.getBetaValues ();
		float gauche = betaValues [0]; // + betaValues [2];
		float droite = betaValues [1]; // + betaValues [3];
		if (gauche <= 0 || droite <= 0) {
			maxGauche=maxDroite=0f;
			speed=Mathf.Max(speed*(1f-Time.deltaTime/closingTime),0f);
			if ((Mathf.Abs(transform.position.x)+Mathf.Abs (transform.position.z)>1e3f) && (speed<0.05f*maxSpeed)) transform.position=new Vector3(0f,transform.position.y,0f); // désolé mais sinon ça risque de boguer
			float ddir = Mathf.Repeat ((transform.eulerAngles.y) - rotationYinit + 180f+360f, 360f)- 180f;
			if (Mathf.Abs (ddir)>precision)
				transform.Rotate (0f, 
				                  0.1f*rotationMaxSpeed * Time.deltaTime * (((ddir <0) ? 1f : 0f) + ((ddir > 0) ? -1f : 0f)),
				                  0f,
				                  Space.World);
			return;
		} 
		float alpha = Time.deltaTime/openingTime;
		speed = speed * (1f - alpha) + alpha * maxSpeed;
		alpha = Time.deltaTime / oscillingTime;
		maxGauche = (1f-alpha) * maxGauche + alpha * gauche;
		maxDroite = (1f-alpha) * maxDroite + alpha * droite;
		if (maxGauche < gauche)
						maxGauche = gauche;
		if (maxDroite < droite)
						maxDroite = droite;
		diff = droite / maxDroite - gauche / maxGauche;
		sgn = (diff == 0f ? 0f : Mathf.Sign (diff));
		rotation *= sgn;

		float tmp = Mathf.Repeat ((transform.eulerAngles.y + rotation) - rotationYinit + 180f+360f, 360f)- 180f;
		if (sgn != 0) {
			if (Mathf.Abs (tmp)<deltaRotationMax) {
				transform.Rotate (0f, rotation, 0f, Space.World);
			}
			else if (Mathf.Abs (tmp)>deltaRotationMax+precision) 
				transform.Rotate (0f, 
			                      rotationMaxSpeed * Time.deltaTime * (((tmp <0) ? 1f : 0f) + ((tmp > 0) ? -1f : 0f)),
			                      0f,
			                      Space.World);
			else transform.Rotate (0f, 
			                       0.01f*rotationMaxSpeed * Time.deltaTime * (((tmp <0) ? 1f : 0f) + ((tmp > 0) ? -1f : 0f)),
			                       0f,
			                       Space.World);
		} 
		else {
			if (Mathf.Abs (tmp)>precision)
				transform.Rotate (0f, 
					              0.1f*rotationMaxSpeed * Time.deltaTime * (((tmp <0) ? 1f : 0f) + ((tmp > 0) ? -1f : 0f)),
				    	          0f,
			            	      Space.World);
		}
	}
}
