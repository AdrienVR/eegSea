
using UnityEngine;
using System.Collections;
using System;



public class PlusieurVagues : MonoBehaviour
{

    public SeaManager SeaManager;
    //les vagues filles de cet objet sont crees a partir du tableau parametresVagues
    //elles servent pour les objets qui flottent et qui vont venir interroger le calculImage qui va bien pour eux
    //le tableau de parametresVagues sert egalement pour mettre a jour le tableau correspondant dans le shader de vagues
    //les perlin vagues ne sont plus utilisbles : pas implementees dans le shader
    public float windDir = 70f; // d'où vient le vent
    public float maxTHF = 1f; // coeff pour le bump map
    public string coef1 = "THF1"; // choix des fréquences gamma pour bump map 1
    public string coef2 = "THF2"; // choix des fréquences gamma pour bump map 2
    public string coef3 = "THF3"; // choix des fréquences gamma pour bump map 3

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

    public WaveParameter[] parametresVagues = new WaveParameter[8];

    Vague[] vagues;
    //PerlinVague[] perlinVagues;
    public float amount;
    public Vague prefabVague;
    public GameObject targetGameObject;

    public int numberOfWavesToUse = 8;

    public Vector3 CalculeImage(Vector3 startPoint)
    {

        Vector3 position = startPoint;
        foreach (Vague vague in vagues)
        {
            if (vague.gameObject.activeInHierarchy)
            {
                position += vague.CalculeVecteur(startPoint, amount);
            }
        }
        //foreach (PerlinVague perlinVague in perlinVagues) {
        //	if (perlinVague.gameObject.activeInHierarchy) {
        //		position.y = position.y + perlinVague.CalculeHauteur(startPoint,amount);
        //	}
        //}
        return position;
    }

    public void CalculeImage(Vector3 startPoint, out Vector3 imagePoint, out Vector3 normal, out Vector4 tangentX)
    {
        imagePoint = startPoint;
        Vector3 slopes = new Vector3();
        Vector3 vecteur = new Vector3();
        Vector3 oneNormal = new Vector3();
        Vector3 oneSlope = new Vector3();
        foreach (Vague vague in vagues)
        {
            if (vague.gameObject.activeInHierarchy)
            {
                vague.CalculeVecteur(startPoint, out vecteur, out oneNormal, amount);
                imagePoint += vecteur;
                oneSlope.x = -oneNormal.x / oneNormal.y;
                oneSlope.y = 0.0f;
                oneSlope.z = -oneNormal.z / oneNormal.y;
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

    // Use this for initialization
    Vague vague;

    void Start()
    {
        //		perlinVagues = gameObject.GetComponentsInChildren<PerlinVague>();
        EEGDataManager.Instance.Initialize(alphaRadius, alphaTHF); //Appeler init une seulf fois dans le programme	//Given param: alpha radius & alpha THF

        SeaManager.SetAmount(amount);
        SeaManager.SetWind(windDir);

        for (int i = 0; i < Mathf.Min(parametresVagues.Length, numberOfWavesToUse); ++i)
        {
            WaveParameter parametres = parametresVagues[i];
            //new child vague game object
            vague = Instantiate(prefabVague, Vector3.zero, Quaternion.identity) as Vague;
            vague.transform.Rotate(Vector3.up * parametres.angle);
            vague.waveLenght = parametres.waveLenght;
            parametres.period = Mathf.Sqrt(vague.waveLenght / 1.6f);
            vague.period = parametres.period;
            vague.radius = parametres.radius;
            if (parametres.density > 1) parametres.density = 1f;
            if (parametres.density <= 0) parametres.density = 0.001f;
            vague.density = parametres.density;
            if (parametres.advance > Mathf.PI / 2) parametres.advance = Mathf.PI / 2;
            if (parametres.advance < 0) parametres.advance = 0;
            vague.advance = parametres.advance;
            vague.transform.parent = transform;
            SeaManager.SetShaderWaveParameters(i, parametres);
        }
        vagues = gameObject.GetComponentsInChildren<Vague>();
    }

    // Update is called once per frame
    void Update()
    {
        if (alphaRadius != memAlphaRadius)
        {
            alphaRadius = Mathf.Max(0.0001f, alphaRadius);
            EEGDataManager.Instance.SetAlphaRad(alphaRadius);
            memAlphaRadius = alphaRadius;
        }
        if (alphaTHF != memAlphaTHF)
        {
            alphaTHF = Mathf.Max(0.0001f, alphaTHF);
            EEGDataManager.Instance.SetAlphaTHF(alphaTHF);
            memAlphaTHF = alphaTHF;
        }

        EEGDataManager.Instance.MAJmoyenneEcartType(Time.deltaTime, TMoyenneBasse, TMoyenneHaute, TMoyenneTheta, TMoyenneTHF, TEcartTypeBasse, TEcartTypeHaute, TEcartTypeTheta, TEcartTypeTHF);

        SeaManager.SetAmount(amount);
        SeaManager.SetWind(windDir);

        //change les coefs des vagues hautes fréquences (texture)
        EEGDataManager.Instance.calculTHF(maxTHF);
        Vector4 thfs;
        thfs.y = EEGDataManager.Instance.getTHF(coef1);
        thfs.z = EEGDataManager.Instance.getTHF(coef2);
        thfs.w = EEGDataManager.Instance.getTHF(coef3);
        thfs.x = Mathf.Max(thfs.y, Mathf.Max(thfs.z, thfs.w));
        thfs.y *= 1f / maxTHF; thfs.z *= 0.75f / maxTHF; thfs.w *= 0.5f / maxTHF;
        // for the shader version 3
        SeaManager.SetShaderCoefParameters(thfs.x, "");
        SeaManager.SetShaderCoefParameters(thfs.y, "1");
        SeaManager.SetShaderCoefParameters(thfs.z, "2");
        SeaManager.SetShaderCoefParameters(thfs.w, "3");
        // for the shader version 4
        SeaManager.SetShaderCoeffsParameters(thfs);

        EEGDataManager.Instance.calculRadius();
        for (int i = 0; i < parametresVagues.Length; i++)
        {
            float r = EEGDataManager.Instance.GetRadius(parametresVagues[i].frequence, parametresVagues[i].position) * parametresVagues[i].radiusMax;
            float alpha = Mathf.Min(1, Time.deltaTime / (alphaRadius * parametresVagues[i].period));
            parametresVagues[i].radius *= (1 - alpha);
            parametresVagues[i].radius += alpha * r;
        }

        for (int i = 0; i < Mathf.Min(parametresVagues.Length, numberOfWavesToUse); ++i)
        {
            SeaManager.SetShaderWaveParameters(i, parametresVagues[i]);
            vagues[i].radius = parametresVagues[i].radius;
        }
        //Client.displayValues();
    }

    void OnDisable()
    {
        ClientBehavior.Instance.Disable();
    }

}
