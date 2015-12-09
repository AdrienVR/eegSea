
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

    float memAlphaTHF;

    //PerlinVague[] perlinVagues;
    public GameObject targetGameObject;

    public int numberOfWavesToUse = 8;


    void Start()
    {
        SeaManager.SetWaveDescriptors(SeaDataManager.GetWaveDescriptors());
        
        SeaManager.SetWindDirection(windDir);

        for (int i = 0; i < 4; i++)
        {
            if (i < 3)
            {
                tabTHF[i] = -40f;
                moyenneTHF[i] = 0f;
                ecartTypeTHF[i] = 1f;
                THF[i] = 0.1f;
                THF1[i] = 0.1f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        SeaManager.SetWindDirection(windDir);

        for (int i = 0; i < 3; i++)
        {
            tabTHF[i] = SeaDataManager.GetTHF(i);
        }

        UpdateTHF();

        //change les coefs des vagues hautes fréquences (texture)
        Vector4 thfs;
        thfs.y = THF1[0];
        thfs.z = THF1[1];
        thfs.w = THF1[2];
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

    void UpdateTHF()
    {
        for (int i = 0; i < 4; i++)
        {
            //tabTHF[i];
        }

        for (int i = 0; i < 4; i++)
        {
            if (i < 3)
            {
                moyenneTHF[i] = EstimationMoyenne(Time.deltaTime / SeaDataManager.TMoyenne, moyenneTHF[i], tabTHF[i]);
                ecartTypeTHF[i] = EstimationEcartType(Time.deltaTime / SeaDataManager.TEcartType, ecartTypeTHF[i], moyenneTHF[i], tabTHF[i]);
            }
        }
    }

    //estimation de l'écart type à l'instant t en tenant compte de l'écart type précédent
    private float EstimationEcartType(float alpha, float ecartType, float moyenne, float lastValue)
    {
        if (lastValue <= 0)
        {
            val1 = 0;
            val2 = 0;
            return ecartType; // on conserve la dernière valeur calculée
        }
        if (val1 == 0)
        { // lastvalue>0 pour la première fois
            val1 = lastValue;
            return ecartType; // on conserve la dernière valeur calculée
        }
        if (val2 == 0)
        { // lastvalue>0 pour la seconde fois
            val2 = lastValue;
            return Mathf.Abs(val2 - val1); // première estimation possible
        }
        return ((1f - alpha) * ecartType + alpha * Mathf.Abs(lastValue - moyenne));
    }
    
    private float val1 = 0;
    private float val2 = 0;

    private float EstimationMoyenne(float alpha, float moyenne, float lastValue) //estimation d'une moyenne à l'instant t en tenant compte de la moyenne précédente
    {
        if (lastValue <= 0)
        { // mesure non valable
            lastValue = 0f;
            return 0f;
        }
        if (lastValue == 0f)
        { // première valeur acceptable pour lastValue
            return lastValue;
        }
        return ((1f - alpha) * moyenne + alpha * lastValue);
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

    private float[] tabTHF = new float[3];
    private float[] moyenneTHF = new float[3];
    private float[] ecartTypeTHF = new float[3];
    private float[] THF = new float[3];
    private float[] THF1 = new float[3];
}
