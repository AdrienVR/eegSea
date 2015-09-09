
using UnityEngine;
using System.Collections;
using System;



public class PlusieurVagues : MonoBehaviour {
	//les vagues filles de cet objet sont crees a partir du tableau parametresVagues
	//elles servent pour les objets qui flottent et qui vont venir interroger le calculImage qui va bien pour eux
	//le tableau de parametresVagues sert egalement pour mettre a jour le tableau correspondant dans le shader de vagues
	//les perlin vagues ne sont plus utilisbles : pas implementees dans le shader
	public float windDir=70f; // d'où vient le vent
	public float maxTHF=1f; // coeff pour le bump map
	public string coef1="THF1"; // choix des fréquences gamma pour bump map 1
	public string coef2="THF2"; // choix des fréquences gamma pour bump map 2
	public string coef3="THF3"; // choix des fréquences gamma pour bump map 3

	public float TMoyenneBasse; // temps de maj moyenne alpha
	public float TMoyenneHaute; // temps de maj moyenne beta
	public float TMoyenneTheta; // temps de maj moyenne theta
	public float TMoyenneTHF; // temps de maj moyenne gamma
	public float TEcartTypeBasse; // idem pour l'ecart type
	public float TEcartTypeHaute; // ""
	public float TEcartTypeTheta; // ""
	public float TEcartTypeTHF; // ""

	public float alphaRadius; // pourcentage de la période des vagues pour temps moyen de maj des hauteurs
	public float alphaTHF; // temps moyen de maj pour le coeff du bumpmap (texture des vagues de hautes fréquences)

	// public bool debug;

	float memAlphaRadius;
	float memAlphaTHF;
	
	[Serializable]
	public class ParametresVague {
		public float angle; //radians
		public float period; //seconds
		public float waveLenght; //meters
		public float radius; //meters
		public string position;
		public string frequence;
		public float radiusMax;
		public float density;
		public float advance;
	};
	public ParametresVague[] parametresVagues  = new ParametresVague[8];

	Vague[] vagues;
	//PerlinVague[] perlinVagues;
	public float amount;
	public Vague prefabVague;
	public GameObject targetGameObject;

	public int numberOfWavesToUse = 8;

	public Vector3 CalculeImage(Vector3 startPoint) {

		Vector3 position = startPoint;
		foreach (Vague vague in vagues){
			if (vague.gameObject.activeInHierarchy) {
				position += vague.CalculeVecteur(startPoint,amount);
			}
		}
		//foreach (PerlinVague perlinVague in perlinVagues) {
		//	if (perlinVague.gameObject.activeInHierarchy) {
		//		position.y = position.y + perlinVague.CalculeHauteur(startPoint,amount);
		//	}
		//}
		return position;	
	}

	public void CalculeImage(Vector3 startPoint, out Vector3 imagePoint, out Vector3 normal, out Vector4 tangentX) {
		imagePoint = startPoint;
		Vector3 slopes = new Vector3(); 
		Vector3 vecteur =  new Vector3();
		Vector3 oneNormal = new Vector3();
		Vector3 oneSlope = new Vector3();
		foreach (Vague vague in vagues) {
			if (vague.gameObject.activeInHierarchy) {
				vague.CalculeVecteur(startPoint, out vecteur, out oneNormal, amount);
				imagePoint += vecteur;
				oneSlope.x = -oneNormal.x/oneNormal.y;
				oneSlope.y = 0.0f;
				oneSlope.z = -oneNormal.z/oneNormal.y;
				slopes += oneSlope;
			}
		}
		//TODO : calculer la normale ou la pente des perlinVagues
		//foreach (PerlinVague perlinVague in perlinVagues) {
		//	if (perlinVague.gameObject.activeInHierarchy) {
		//		imagePoint.y = imagePoint.y + perlinVague.CalculeHauteur(startPoint,amount);
		//	}
		//}

		normal.x = -slopes.x;
		normal.y = 1.0f;
		normal.z = -slopes.z;
		normal.Normalize();

		//tangentX.x = 1.0f;
		//tangentX.y = slopes.z+slopes.x;
		//tangentX.z = 1.0f;

		tangentX.x = 1.0f;
		tangentX.y = slopes.x;
		tangentX.z = 0.0f;
		tangentX.w = 1.0f;
		tangentX.Normalize();
	}

	void setShaderWaveParameters(Material material, int i, ParametresVague parametres)
	{
		string propertyName = "_Wave" + (i+1).ToString() + "Parameters";
		if (material.HasProperty(propertyName))
		{
			Vector4 propertyValue = new Vector4(parametres.angle*Mathf.Deg2Rad,
			                                    parametres.radius,
			                                    parametres.period,
			                                    parametres.waveLenght);
			material.SetVector(propertyName,propertyValue);
		}
		propertyName = "_Wave" + (i + 1).ToString () + "GroupParam"; // OK for seaShader version 4
		if (material.HasProperty (propertyName)) {
			Vector4 propValue = new Vector4(parametres.density, parametres.advance);
			material.SetVector (propertyName, propValue);
		}
		propertyName = "_Wave" + (i + 1).ToString () + "density"; // OK for seaShader version 3
		if (material.HasProperty (propertyName)) {
			float density = parametres.density;
			material.SetFloat (propertyName, density);
		}
		propertyName = "_Wave" + (i + 1).ToString () + "advance"; // OK for seaShader version 3
		if (material.HasProperty (propertyName)) {
			float advance = parametres.advance;
			material.SetFloat (propertyName, advance);
		}


	}
	void setShaderCoefParameters(Material material,float coef, string i)
	{
		string propertyName = "_coefHF" + i;
		if (material.HasProperty (propertyName)) {
			material.SetFloat(propertyName,coef);
		}
	}
	
	void setShaderCoeffsParameters(Material material,Vector4 coeffs)
	{
		string propertyName = "_coeffsHF";
		if (material.HasProperty (propertyName)) {
			material.SetVector(propertyName,coeffs);
		}
	}

	// Use this for initialization
	Vague vague;

	void Start () {
//		perlinVagues = gameObject.GetComponentsInChildren<PerlinVague>();
		Client.init(alphaRadius,alphaTHF); //Appeler init une seulf fois dans le programme	//Given param: alpha radius & alpha THF

		Material targetMaterial = targetGameObject.renderer.material;
	    if (targetMaterial.HasProperty("_WavesAmount"))
		{
			targetMaterial.SetFloat("_WavesAmount",amount);
		}
		if(targetMaterial.HasProperty("_windDir")) targetMaterial.SetFloat("_windDir",windDir*Mathf.Deg2Rad);

		for (int i=0; i<Mathf.Min(parametresVagues.Length,numberOfWavesToUse); ++i) {
			ParametresVague parametres = parametresVagues [i];
			//new child vague game object
			vague = Instantiate (prefabVague, Vector3.zero, Quaternion.identity) as Vague;
			vague.transform.Rotate (Vector3.up * parametres.angle);
			vague.waveLenght = parametres.waveLenght;
			parametres.period = Mathf.Sqrt (vague.waveLenght / 1.6f);
			vague.period = parametres.period;
			vague.radius = parametres.radius;
			if (parametres.density > 1) parametres.density = 1f;
			if (parametres.density <= 0) parametres.density = 0.001f;
			vague.density = parametres.density;
			if (parametres.advance > Mathf.PI / 2) parametres.advance = Mathf.PI / 2;
			if (parametres.advance < 0) parametres.advance = 0;
			vague.advance = parametres.advance;
			vague.transform.parent = transform;
			setShaderWaveParameters (targetMaterial, i, parametres);
		}
		vagues = gameObject.GetComponentsInChildren<Vague>();
	}

	// Update is called once per frame
	void Update () 
	{
		if (alphaRadius != memAlphaRadius) {
			alphaRadius=Mathf.Max (0.0001f, alphaRadius);
			Client.setAlphaRad(alphaRadius); 
			memAlphaRadius = alphaRadius; 
		}
		if (alphaTHF != memAlphaTHF) {
			alphaTHF=Mathf.Max (0.0001f,alphaTHF);
			Client.setAlphaTHF(alphaTHF);
			memAlphaTHF = alphaTHF; 
		}

		Client.MAJmoyenneEcartType (Time.deltaTime,TMoyenneBasse,TMoyenneHaute,TMoyenneTheta,TMoyenneTHF,TEcartTypeBasse,TEcartTypeHaute,TEcartTypeTheta,TEcartTypeTHF);
		
		Material targetMaterial = targetGameObject.renderer.material;
		if (targetMaterial.HasProperty("_WavesAmount")) targetMaterial.SetFloat("_WavesAmount",amount);
		if(targetMaterial.HasProperty("_windDir")) targetMaterial.SetFloat("_windDir",windDir*Mathf.Deg2Rad);

		//change les coefs des vagues hautes fréquences (texture)
		Client.calculTHF (maxTHF);
		Vector4 thfs;
		thfs.y = Client.getTHF (coef1);
		thfs.z = Client.getTHF (coef2);
		thfs.w = Client.getTHF (coef3);
		thfs.x=Mathf.Max(thfs.y,Mathf.Max(thfs.z,thfs.w));
		thfs.y *= 1f / maxTHF; thfs.z *= 0.75f / maxTHF; thfs.w *= 0.5f / maxTHF;
		// for the shader version 3
		setShaderCoefParameters(targetMaterial,thfs.x,"");
		setShaderCoefParameters(targetMaterial,thfs.y,"1");
		setShaderCoefParameters(targetMaterial,thfs.z,"2");
		setShaderCoefParameters(targetMaterial,thfs.w,"3");
		// for the shader version 4
		setShaderCoeffsParameters (targetMaterial, thfs);

		Client.calculRadius ();
		for (int i =0; i<parametresVagues.Length; i++) {
			float r=Client.getRadius(parametresVagues[i].frequence,parametresVagues[i].position)*parametresVagues[i].radiusMax;
			float alpha=Mathf.Min(1,Time.deltaTime/(alphaRadius*parametresVagues[i].period));
			parametresVagues[i].radius*=(1-alpha);
			parametresVagues[i].radius+=alpha*r;
		}

		for (int i=0;i<Mathf.Min(parametresVagues.Length,numberOfWavesToUse);++i) {
			setShaderWaveParameters(targetMaterial,i,parametresVagues[i]);
			vagues[i].radius=parametresVagues[i].radius;
		}
		//Client.displayValues();
	}

	void OnDisable() {
		Client.Disable ();//fermeture du programme
	}

}
