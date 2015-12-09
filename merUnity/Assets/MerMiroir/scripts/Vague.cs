using UnityEngine;
using System.Collections;

public class Vague : MonoBehaviour {
	
	public float waveLenght;
	public float period;
	public float radius;
	public float density;
	public float advance;
	
	float poorRand(float x, float z) { // random in [0,1]
		float n = (Mathf.Abs(x) % 1024) + 983 * (Mathf.Abs(z) % 1024);
		return ((n * 12456 + 1234) % 10000) * 0.0001f;
	}
	
	float instantPhase(float theta)
    {
		theta*=0.159154943f;
		theta+=0.25f;
		return 4*Mathf.Abs((theta)-Mathf.Floor(theta)-0.5f)-1f;
	}

	public Vector3 CalculeImage(Vector3 startPoint,float amount=0.0f) {
		return startPoint + CalculeVecteur(startPoint,amount);
	}

	public Vector3 CalculeVecteur(Vector3 startPoint,float amount=0.0f) {
		// Vector3 localPoint = transform.InverseTransformPoint(startPoint);
		float k = 6.2831853f / waveLenght;
		float w = 6.2831853f / period;
		float cosWaveAngle = Mathf.Cos (transform.eulerAngles.y*3.14159265f/180f);
		float sinWaveAngle = Mathf.Sin (transform.eulerAngles.y*3.14159265f/180f);
		float xIn = cosWaveAngle*startPoint.x + sinWaveAngle*startPoint.z;
		float gs = w * waveLenght * 0.079577472f; // group speed w/(2k)
		float Lg = waveLenght * 6 / density;
		float gx_ref = cosWaveAngle * gs * Time.time; //Mathf.Repeat (cosWaveAngle * gs * Time.time, Lg);
		float gz_ref = sinWaveAngle * gs * Time.time; //Mathf.Repeat (sinWaveAngle * gs * Time.time, Lg);
		int ix = (int)Mathf.Floor (2 * (startPoint.x - gx_ref) / Lg);
		int iz = (int)Mathf.Floor (2 * (startPoint.z - gz_ref) / Lg);
		float phase = k * xIn - w * Time.time;

		// premiere vague (0,0)
		float gx = gx_ref + 0.5f * Lg * (ix);
		float gz = gz_ref + 0.5f * Lg * (iz);
		float index = poorRand (ix, iz); // random in [0,1] (always the same for a given ix and iz)
		float cx = (startPoint.x - gx) * k * 0.1f;
		float cz = (startPoint.z - gz) * k * 0.1f;
		float gauss = Mathf.Exp (-(cx * cx + cz * cz));
		float angle = phase + index * 6.2831853f;
		float radius2 = radius * amount * gauss;
		float addphase=amount * gauss*advance*instantPhase(angle);
		float xw = Mathf.Cos(angle+addphase)*radius2;
		float yw = Mathf.Sin (angle) * radius2;

		// second groupe (1,0)
		gx=gx_ref+0.5f*Lg*(ix+1);
		gz=gz_ref+0.5f*Lg*(iz);
		index=poorRand(ix+1, iz); // random in [0,1] (always the same for a given ix and iz)
		cx=(startPoint.x-gx)*k*0.1f;
		cz=(startPoint.z-gz)*k*0.1f;
		gauss=Mathf.Exp(-(cx*cx+cz*cz));
		angle = phase + index*6.2831853f;
		radius2 = radius * amount * gauss;
		addphase=amount * gauss*advance*instantPhase(angle);
		xw += Mathf.Cos(angle+addphase)*radius2;
		yw += Mathf.Sin(angle)*radius2;

		//troisieme groupe (0,1)
		gx=gx_ref+0.5f*Lg*(ix);
		gz=gz_ref+0.5f*Lg*(iz+1);
		index=poorRand(ix, iz+1); // random in [0,1] (always the same for a given ix and iz)
		cx=(startPoint.x-gx)*k*0.1f;
		cz=(startPoint.z-gz)*k*0.1f;
		gauss=Mathf.Exp(-(cx*cx+cz*cz));
		angle = phase + index*6.2831853f;
		radius2 = radius * amount * gauss;
		addphase=amount * gauss*advance*instantPhase(angle);
		xw += Mathf.Cos(angle+addphase)*radius2;
		yw += Mathf.Sin(angle)*radius2;

		//quatrieme groupe (1,1)
		gx=gx_ref+0.5f*Lg*(ix+1);
		gz=gz_ref+0.5f*Lg*(iz+1);
		index=poorRand(ix+1, iz+1);
		cx=(startPoint.x-gx)*k*0.1f;
		cz=(startPoint.z-gz)*k*0.1f;
		gauss=Mathf.Exp(-(cx*cx+cz*cz));
		angle = phase + index*6.2831853f;
		radius2 = radius * amount * gauss;
		addphase=amount * gauss*advance*instantPhase(angle);
		xw += Mathf.Cos(angle+addphase)*radius2;
		yw += Mathf.Sin(angle)*radius2;

		Vector3 resultVector = new Vector3 (xw*cosWaveAngle, yw, xw*sinWaveAngle);
		return resultVector; 
	}


	public void CalculeImage(Vector3 startPoint, out Vector3 imagePoint, out Vector3 normal, float amount=0.0f) {
		Vector3 vecteur;
		CalculeVecteur(startPoint,out vecteur,out normal, amount);
		imagePoint = startPoint + vecteur;
	}

	public void CalculeVecteur(Vector3 startPoint, out Vector3 imageVector, out Vector3 normal, float amount=0.0f) {
		Vector3 localPoint = transform.InverseTransformPoint(startPoint);
		float k = 2 * Mathf.PI / waveLenght;
		float w = 2 * Mathf.PI / period;
		float angle = w * Time.time + k*localPoint.x;
		float localX = Mathf.Cos(angle)*radius*amount;
		float localY = Mathf.Sin(angle)*radius*amount;
		imageVector = transform.TransformDirection(localX, localY, 0.0f);
		float R = waveLenght / (2*Mathf.PI);
		Vector3 localNormal = new Vector3(-localX, R - localY, 0.0f);
		normal = transform.TransformDirection(localNormal.normalized);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
