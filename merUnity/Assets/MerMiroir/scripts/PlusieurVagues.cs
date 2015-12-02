
using UnityEngine;

public class PlusieurVagues : WaveListener
{

    public SeaManager SeaManager;
    public SeaDataManager SeaDataManager;
    //les vagues filles de cet objet sont crees a partir du tableau parametresVagues
    //elles servent pour les objets qui flottent et qui vont venir interroger le calculImage qui va bien pour eux
    //le tableau de parametresVagues sert egalement pour mettre a jour le tableau correspondant dans le shader de vagues
    //les perlin vagues ne sont plus utilisbles : pas implementees dans le shader
    public float windDir = 70f; // d'où vient le vent

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


    public override void SetWave(WaveDescriptor[] waveDescriptors)
    {
        m_waveDescriptors = waveDescriptors;
    }

    private WaveDescriptor[] m_waveDescriptors;

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
        SeaDataManager.Initialize(alphaRadius, alphaTHF); //Appeler init une seulf fois dans le programme	//Given param: alpha radius & alpha THF

        SeaManager.SetWavesAmount(amount);
        SeaManager.SetWindDirection(windDir);

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
            SeaManager.SetWaveParameters(i, parametres);
        }
        vagues = gameObject.GetComponentsInChildren<Vague>();
    }

    // Update is called once per frame
    void Update()
    {
        if (alphaRadius != memAlphaRadius)
        {
            alphaRadius = Mathf.Max(0.0001f, alphaRadius);
            SeaDataManager.SetAlphaRad(alphaRadius);
            memAlphaRadius = alphaRadius;
        }
        if (alphaTHF != memAlphaTHF)
        {
            alphaTHF = Mathf.Max(0.0001f, alphaTHF);
            SeaDataManager.SetAlphaTHF(alphaTHF);
            memAlphaTHF = alphaTHF;
        }

        SeaDataManager.MAJmoyenneEcartType(Time.deltaTime, TMoyenneBasse, TMoyenneHaute, TMoyenneTheta, TMoyenneTHF, TEcartTypeBasse, TEcartTypeHaute, TEcartTypeTheta, TEcartTypeTHF);

        SeaManager.SetWavesAmount(amount);
        SeaManager.SetWindDirection(windDir);

        //change les coefs des vagues hautes fréquences (texture)
        SeaDataManager.calculTHF(maxTHF);
        Vector4 thfs;
        thfs.y = SeaDataManager.getTHF(coef1);
        thfs.z = SeaDataManager.getTHF(coef2);
        thfs.w = SeaDataManager.getTHF(coef3);
        thfs.x = Mathf.Max(thfs.y, Mathf.Max(thfs.z, thfs.w));
        thfs.y *= 1f / maxTHF; thfs.z *= 0.75f / maxTHF; thfs.w *= 0.5f / maxTHF;
        // for the shader version 3
        SeaManager.SetCoefHF(thfs.x, "");
        SeaManager.SetCoefHF(thfs.y, "1");
        SeaManager.SetCoefHF(thfs.z, "2");
        SeaManager.SetCoefHF(thfs.w, "3");
        // for the shader version 4
        SeaManager.SetCoeffsHF(thfs);
        
        //Client.displayValues();
    }

    void OnDisable()
    {
        ClientBehavior.Instance.Disable();
    }

}
