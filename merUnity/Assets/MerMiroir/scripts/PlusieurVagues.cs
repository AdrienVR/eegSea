
using UnityEngine;

public class PlusieurVagues : MonoBehaviour
{
    public SeaManager SeaManager;
    public SeaDataManager SeaDataManager;
    //les vagues filles de cet objet sont crees a partir du tableau parametresVagues
    //elles servent pour les objets qui flottent et qui vont venir interroger le calculImage qui va bien pour eux
    //le tableau de parametresVagues sert egalement pour mettre a jour le tableau correspondant dans le shader de vagues
    //les perlin vagues ne sont plus utilisbles : pas implementees dans le shader
    public float windDir = 70f; // d'où vient le vent

    public float alphaRadius; // pourcentage de la période des vagues pour temps moyen de maj des hauteurs
    public float alphaTHF; // temps moyen de maj pour le coeff du bumpmap (texture des vagues de hautes fréquences)
    // public bool debug;

    public float maxTHF = 1f; // coeff pour le bump map
    public string coef1 = "THF1"; // choix des fréquences gamma pour bump map 1
    public string coef2 = "THF2"; // choix des fréquences gamma pour bump map 2
    public string coef3 = "THF3"; // choix des fréquences gamma pour bump map 3

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
        SeaManager.SetWaveDescriptors(SeaDataManager.GetWaveDescriptors());

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
        SeaManager.SetWavesAmount(amount);
        SeaManager.SetWindDirection(windDir);

        //change les coefs des vagues hautes fréquences (texture)
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

    public void CalculTHF(float maxTHF)
    {
        //Calcule les différentes valeurs que prendront les coefs correspondant aux très hautes fréquences (coef modifiant la texture pour donner l'impression de vagues de surface)
        THF1[0] = Mathf.Exp(MathsTool.CenterValue(moyenneTHF[0], ecartTypeTHF[0], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[0], 1f));
        if (THF1[0] < 0.1f)
            THF1[0] = 0.1f;
        THF1[1] = Mathf.Exp(MathsTool.CenterValue(moyenneTHF[1], ecartTypeTHF[1], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[1], 1f));
        if (THF1[1] < 0.1f)
            THF1[1] = 0.1f;
        THF1[2] = Mathf.Exp(MathsTool.CenterValue(moyenneTHF[2], ecartTypeTHF[2], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[2], 1f));
        if (THF1[2] < 0.1f)
            THF1[2] = 0.1f;
        for (int i = 0; i < 3; i++)
        {
            float alpha = Mathf.Min(1, Time.deltaTime / alphaTHF);
            THF[i] = (1f - alpha) * THF[i] + (alpha * THF1[i]); //valeur a t+1 tenant compte de la valeur précédente
        }

    }

    void OnDisable()
    {
        ClientBehavior.Instance.Disable();
    }

    private float[] tabTHF = new float[3];
    private float[] moyenneTHF = new float[3];
    private float[] ecartTypeTHF = new float[3];
    private float[] THF = new float[3];
    private float[] THF1 = new float[3];
}
