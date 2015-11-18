using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class EEGDataManager : MonoBehaviour
{
    // Singleton
    public static EEGDataManager Instance;

    public WaveListener[] WaveListeners;

    public float Delta { get { return maxGD - minGD; } }

    void Awake()
    {
        Instance = this;
		m_transFourrier = new Fft();
    }

    void Start()
    {
        // Initialisation des groupes depuis les fichiers .xml
        InitGroups();
        foreach(WaveListener waveListener in WaveListeners)
        {
            waveListener.SetWave(m_electrodeGroups);
        }
    }


    void InitGroups()
    {
        string[] listOfXMLFiles = Directory.GetFiles("Assets/MerMiroir/Config/", "*.xml");

        int index = 0;

        foreach (string path in listOfXMLFiles)
        {
            XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
            string content = System.IO.File.ReadAllText(path);
            xmlDoc.LoadXml(content);
            XmlNodeList levelsList = xmlDoc.GetElementsByTagName("group"); // array of the level nodes.
            foreach (XmlNode levelInfo in levelsList)
            {
                string name = "error";
                string f_min = "error";
                string f_max = "error";
                List<string> electrodes = new List<string>();
                XmlNodeList levelcontent = levelInfo.ChildNodes;
                foreach (XmlNode levelsItems in levelcontent)
                {
                    if (levelsItems.Name == "name")
                    {
                        name = levelsItems.InnerText;
                    }
                    else if (levelsItems.Name == "f_min")
                    {
                        f_min = levelsItems.InnerText;
                    }
                    else if (levelsItems.Name == "f_max")
                    {
                        f_max = levelsItems.InnerText;
                    }
                    else if (levelsItems.Name == "electrodes_list")
                    {
                        XmlNodeList subLevelcontent = levelsItems.ChildNodes;
                        foreach (XmlNode subLevelsItems in subLevelcontent)
                        {
                            if (subLevelsItems.Name == "electrode")
                            {
                                electrodes.Add(subLevelsItems.InnerText);
                            }
                            else
                            {
                                Debug.LogError("ERROR : invalid content in Xml file, marke '" + subLevelsItems.Name + "' have been found");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("ERROR : invalid content in Xml file, marke '" + levelsItems.Name + "' have been found");
                    }
                }
                List<int> elecsInt = new List<int>();
                foreach (string s in electrodes)
                {
                    elecsInt.Add(reverseDict(Group.ElecNames)[s]);
                }
                Group g = new Group(name, elecsInt, int.Parse(f_min), int.Parse(f_max));
                Debug.Log("Import group : " + g.getText());
                m_electrodeGroups[index] = g;
                /*
				Debug.Log ("__________________________");
				Debug.Log ("name is "+name);
				Debug.Log ("f_min is "+f_min);
				Debug.Log ("f_max is "+f_max);
				Debug.Log("electrodes :");
				foreach(String s in electrodes)
				{
					Debug.Log ("elec is "+s);
				}
				*/
            }
        }

    }

    Dictionary<string, int> reverseDict(Dictionary<int, string> input)
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        foreach (int i in input.Keys)
        {
            output.Add(input[i], i);
        }
        return output;
    }

	public float[] GetFT()
	{
		return m_transFourrier.GetFT();
	}

	public void UpdateValues(double[] sensorVal)
    {
		m_transFourrier.addSample(sensorVal);
        foreach (Group group in m_electrodeGroups)
        {
            float[] frequencies = null;
            float[] amplitudes = null;
            group.UpdateRadius(frequencies, amplitudes);
        }
		/*
        switch (splitValues[0])
        {
            case "Basse":				//Alpha
                {
                    for (int i = 0; i < 4; i++) //0 : avant gauche, 1 : avant droite, 2 : arriere gauche, 3 : arriere droite 
                    {
                        if (splitValues[i + 1] != null)
                        {
                            basseFrequence[i] = float.Parse(splitValues[i + 1], CultureInfo.InvariantCulture.NumberFormat);
                            if (basseFrequence[i] > 0)
                            {
                                basseFrequence[i] = Mathf.Log(basseFrequence[i]);
                            }
                            else
                            {
                                basseFrequence[i] = -50f;
                            }
                        }
                    }
                    break;
                }
        }
                */
    }

    // Use this for initialization
    public void Initialize(float alphaRadiusValue, float alphaTHFValue)
    {

        alphaRadius = alphaRadiusValue;
        alphaTHF = Mathf.Max(0.001f, alphaTHFValue);

        for (int i = 0; i < 4; i++)
        {
            basseFrequence[i] = -40f;
            moyenneBasse[i] = 0f;
            ecartTypeBasse[i] = 1f;
            hauteFrequence[i] = -40f;
            moyenneHaute[i] = 0f;
            ecartTypeHaute[i] = 1f;
            thetaFrequence[i] = -40f;
            moyenneTheta[i] = 0f;
            ecartTypeTheta[i] = 1;
            if (i < 3)
            {
                tabTHF[i] = -40f;
                moyenneTHF[i] = 0f;
                ecartTypeTHF[i] = 1f;
                THF[i] = 0.1f;
                THF1[i] = 0.1f;
            }
        }
        for (int i = 0; i < 12; i++)
        { // valeur à multiplier par rmax pour le rayon des vagues
            //tabVal[i]=0f;
            tabVal1[i] = 0f;
        }
    }

    public float GetAlpha(float coeff, float alpha, float delta)
    {
		return (1f-alpha)*coeff+alpha*(maxGD - valGaucheDroite) / delta;
    }

    public float GetRadius(string frequence, string position)
    {
        switch (frequence)
        {
            case "basse":
                if (position == "avantG")
                {
                    return tabVal1[0];
                    //return tabVal[0];
                }
                else if (position == "avantD")
                {
                    return tabVal1[1];
                    //return tabVal[1];
                }
                else if (position == "arriereG")
                {
                    return tabVal1[2];
                    //return tabVal[2];
                }
                else if (position == "arriereD")
                {
                    return tabVal1[3];
                    //return tabVal[3];
                }
                else
                {
                    return 0;
                }
            case "haute":
                if (position == "avantG")
                {
                    return tabVal1[4];
                    //return tabVal[4];
                }
                else if (position == "avantD")
                {
                    return tabVal1[5];
                    //return tabVal[5];
                }
                else if (position == "arriereG")
                {
                    return tabVal1[6];
                    //return tabVal[6];
                }
                else if (position == "arriereD")
                {
                    return tabVal1[7];
                    //return tabVal[7];
                }
                else
                {
                    return 0;
                }
            case "theta":
                if (position == "avantG")
                {
                    return tabVal1[8];
                    //return tabVal[8];
                }
                else if (position == "avantD")
                {
                    return tabVal1[9];
                    //return tabVal[9];
                }
                else if (position == "arriereG")
                {
                    return tabVal1[10];
                    //return tabVal[10];
                }
                else if (position == "arriereD")
                {
                    return tabVal1[11];
                    //return tabVal[11];
                }
                else
                {
                    return 0;
                }
            default:
                return 0;
        }
    }

    private float FunMoyenne(float[] tab) //moyenne réel en fonction d'un tableau de valeurs
    {
        if (tab.Length < 1)
            return 0f;
        float moyenne = 0f;
        for (int i = 0; i < tab.Length; i++)
        {
            moyenne += tab[i];
        }
        return (moyenne / tab.Length);
    }
    private float EstimationMoyenne(float alpha, float moyenne, float lastValue) //estimation d'une moyenne à l'instant t en tenant compte de la moyenne précédente
    {
        if (lastValue <= 0)
        { // mesure non valable
            val0 = 0f;
            return 0f;
        }
        if (val0 == 0f)
        { // première valeur acceptable pour lastValue
            val0 = lastValue;
            return lastValue;
        }
        return ((1f - alpha) * moyenne + alpha * lastValue);
    }

    private float FunEcartType(float[] tab) //écart type réel en fonction d'un tableau de valeurs
    {
        if (tab.Length < 2)
            return 0f;
        float moyenne = FunMoyenne(tab);
        float ecartType = 0;
        for (int i = 0; i < tab.Length; i++)
        {
            ecartType = ecartType + ((tab[i] - moyenne) * (tab[i] - moyenne));
        }
        ecartType = ecartType / (tab.Length - 1);
        return Mathf.Sqrt(ecartType);
    }

    private float EstimationEcartType(float alpha, float ecartType, float moyenne, float lastValue) //estimation de l'écart type à l'instant t en tenant compte de l'écart type précédent
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

    public void MAJmoyenneEcartType(float dt, float TMbasse, float TMhaute, float TMtheta, float TMthf, float TETbasse, float TEThaute, float TETtheta, float TETthf)
    {
        //mise à jour des moyennes et des écarts type
        //dt(s) : intervalle de temps entre deux frames 
        //T(s) : L'estimation de la moyenne/ecart type tient compte de toutes les valeurs recu durant la période T 
        for (int i = 0; i < 4; i++)
        {
            moyenneBasse[i] = EstimationMoyenne(dt / TMbasse, moyenneBasse[i], basseFrequence[i]);
            ecartTypeBasse[i] = EstimationEcartType(dt / TETbasse, ecartTypeBasse[i], moyenneBasse[i], basseFrequence[i]);
            moyenneHaute[i] = EstimationMoyenne(dt / TMhaute, moyenneHaute[i], hauteFrequence[i]);
            ecartTypeHaute[i] = EstimationEcartType(dt / TEThaute, ecartTypeHaute[i], moyenneHaute[i], hauteFrequence[i]);
            moyenneTheta[i] = EstimationMoyenne(dt / TMtheta, moyenneTheta[i], thetaFrequence[i]);
            ecartTypeTheta[i] = EstimationEcartType(dt / TETtheta, ecartTypeTheta[i], moyenneTheta[i], thetaFrequence[i]);
            if (i < 3)
            {
                moyenneTHF[i] = EstimationMoyenne(dt / TMthf, moyenneTHF[i], tabTHF[i]);
                ecartTypeTHF[i] = EstimationEcartType(dt / TETthf, ecartTypeTHF[i], moyenneTHF[i], tabTHF[i]);
            }
        }
    }

    public static float getAmount()
    {
        return 1f;
    }

    /*
     * 	Alpha radius setters
     */
    public void SetAlphaRad(float value)
    {
        alphaRadius = value;
        Debug.Log(" - AlphaRadius :" + alphaRadius);
    }

    public void SetAlphaTHF(float value)
    {
        alphaTHF = Mathf.Max(0.0001f, value);
        Debug.Log(" - Alpha THF :" + alphaTHF);
    }

    public void MoyenneGD() //différence gauche droite
    {
        float moyenneGauche;
        float moyenneDroite;
        float valGauche;
        float valDroite;

        moyenneGauche = moyenneBasse[0] + moyenneBasse[2] + moyenneHaute[0] + moyenneHaute[2] + moyenneTheta[0] + moyenneTheta[2];
        moyenneDroite = moyenneBasse[1] + moyenneBasse[3] + moyenneHaute[1] + moyenneHaute[3] + moyenneTheta[1] + moyenneTheta[3];
        valGauche = basseFrequence[0] + basseFrequence[2] + hauteFrequence[0] + hauteFrequence[2] + thetaFrequence[0] + thetaFrequence[2];
        valDroite = basseFrequence[1] + basseFrequence[3] + hauteFrequence[1] + hauteFrequence[3] + thetaFrequence[1] + thetaFrequence[3];
        if (moyenneGauche <= 0 || moyenneDroite <= 0 || valGauche <= 0 || valDroite <= 0)
        {
            moyenneGaucheDroite = valGaucheDroite = minMoyGD = minGD = 0f;
            maxMoyGD = maxGD = 0f;
            return;
        }

        moyenneGaucheDroite = moyenneDroite - moyenneGauche;
        minMoyGD = 0.99f * minMoyGD + 0.01f * moyenneGaucheDroite;
        maxMoyGD = 0.99f * maxMoyGD + 0.01f * moyenneGaucheDroite;
        if (moyenneGaucheDroite < minMoyGD)
            minMoyGD = moyenneGaucheDroite;
        if (moyenneGaucheDroite > maxMoyGD)
            maxMoyGD = moyenneGaucheDroite;

        valGaucheDroite = valDroite - valGauche;
        minGD = 0.99f * minGD + 0.01f * valGaucheDroite;
        maxGD = 0.99f * maxGD + 0.01f * valGaucheDroite;
        if (valGaucheDroite < minGD)
            minGD = valGaucheDroite;
        if (valGaucheDroite > maxGD)
            maxGD = valGaucheDroite;
    }

    public float[] GetBetaValues()
    {
        return moyenneHaute;
    }


    public void calculTHF(float maxTHF)
    {
        //Calcul les différentes valeurs que prendront les coefs correspondant aux très hautes fréquences (coef modifiant la texture pour donner l'impression de vagues de surface)
        THF1[0] = Mathf.Exp(CenterValue(moyenneTHF[0], ecartTypeTHF[0], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[0], 1f));
        if (THF1[0] < 0.1f)
            THF1[0] = 0.1f;
        THF1[1] = Mathf.Exp(CenterValue(moyenneTHF[1], ecartTypeTHF[1], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[1], 1f));
        if (THF1[1] < 0.1f)
            THF1[1] = 0.1f;
        THF1[2] = Mathf.Exp(CenterValue(moyenneTHF[2], ecartTypeTHF[2], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[2], 1f));
        if (THF1[2] < 0.1f)
            THF1[2] = 0.1f;
        for (int i = 0; i < 3; i++)
        {
            float alpha = Mathf.Min(1, Time.deltaTime / alphaTHF);
            THF[i] = (1f - alpha) * THF[i] + (alpha * THF1[i]); //valeur a t+1 tenant compte de la valeur précédente
        }

    }

    public float getTHF(string coef)
    {
        if (coef == "THF1")
            return THF[0];
        if (coef == "THF2")
            return THF[1];
        if (coef == "THF3")
            return THF[2];
        return 1f; //valeur par défaut

    }
    public void calculRadius()
    {
        for (int i = 0; i < 4; i++)
        {
            //Calcul les différentes valeurs des radius des vagues (correspont à un rayon)
            tabVal1[i] = CenterValue(moyenneBasse[i], ecartTypeBasse[i], 0f, 1f, basseFrequence[i], 2f);
            tabVal1[i + 4] = CenterValue(moyenneHaute[i], ecartTypeHaute[i], 0f, 1f, hauteFrequence[i], 2f);
            tabVal1[i + 8] = CenterValue(moyenneTheta[i], ecartTypeTheta[i], 0f, 1f, thetaFrequence[i], 2f);
            //tabVal[i]=((1-alphaRadius)*tabVal[i])+(alphaRadius*tabVal1[i]);
            //tabVal[i+4]=((1-alphaRadius)*tabVal[i+4])+(alphaRadius*tabVal1[i+4]);
            //tabVal[i+8]=((1-alphaRadius)*tabVal[i+8])+(alphaRadius*tabVal1[i+8]);		
        }
    }

    public void ResetValues()
    {
        for (int i = 0; i < 4; i++)
        {
            basseFrequence[i] = -40f;
            hauteFrequence[i] = -40f;
            thetaFrequence[i] = -40f;
            if (i < 3)
            {
                tabTHF[i] = -40f;
            }

        }
    }

    public void CalculRadius()
    {
        for (int i = 0; i < 4; i++)
        {
            //Calcule les différentes valeurs des radius des vagues (correspont à un rayon)
            tabVal1[i] = CenterValue(moyenneBasse[i], ecartTypeBasse[i], 0f, 1f, basseFrequence[i], 2f);
            tabVal1[i + 4] = CenterValue(moyenneHaute[i], ecartTypeHaute[i], 0f, 1f, hauteFrequence[i], 2f);
            tabVal1[i + 8] = CenterValue(moyenneTheta[i], ecartTypeTheta[i], 0f, 1f, thetaFrequence[i], 2f);
            //tabVal[i]=((1-alphaRadius)*tabVal[i])+(alphaRadius*tabVal1[i]);
            //tabVal[i+4]=((1-alphaRadius)*tabVal[i+4])+(alphaRadius*tabVal1[i+4]);
            //tabVal[i+8]=((1-alphaRadius)*tabVal[i+8])+(alphaRadius*tabVal1[i+8]);		
        }
    }

    public void CalculTHF(float maxTHF)
    {
        //Calcule les différentes valeurs que prendront les coefs correspondant aux très hautes fréquences (coef modifiant la texture pour donner l'impression de vagues de surface)
        THF1[0] = Mathf.Exp(CenterValue(moyenneTHF[0], ecartTypeTHF[0], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[0], 1f));
        if (THF1[0] < 0.1f)
            THF1[0] = 0.1f;
        THF1[1] = Mathf.Exp(CenterValue(moyenneTHF[1], ecartTypeTHF[1], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[1], 1f));
        if (THF1[1] < 0.1f)
            THF1[1] = 0.1f;
        THF1[2] = Mathf.Exp(CenterValue(moyenneTHF[2], ecartTypeTHF[2], Mathf.Log(0.1f), Mathf.Log(maxTHF), tabTHF[2], 1f));
        if (THF1[2] < 0.1f)
            THF1[2] = 0.1f;
        for (int i = 0; i < 3; i++)
        {
            float alpha = Mathf.Min(1, Time.deltaTime / alphaTHF);
            THF[i] = (1f - alpha) * THF[i] + (alpha * THF1[i]); //valeur a t+1 tenant compte de la valeur précédente
        }

    }
    private float CenterValue(float moyenne, float ecartType, float valMin, float valMax, float valCapteur, float tolerance)
    {
        //centre la valeur en fonction de la moyenne et l'écart type
        if (valCapteur <= 0)
            return valMin;
        if (valCapteur <= (moyenne - tolerance * ecartType))
        {
            return valMin;
        }
        else if (valCapteur > (moyenne + tolerance * ecartType))
        {
            return valMax;
        }
        else
        {
            if (ecartType != 0) return (valMin + valMax) / 2f + (valMax - valMin) * (valCapteur - moyenne) / (ecartType * 2f * tolerance);
        }
        return (valMin + valMax) / 2f;
    }

    private float[] basseFrequence = new float[4];
    private float[] hauteFrequence = new float[4];
    private float[] thetaFrequence = new float[4];
    private float[] tabTHF = new float[3];
    private float[] moyenneBasse = new float[4];
    private float[] moyenneHaute = new float[4];
    private float[] moyenneTheta = new float[4];
    private float[] moyenneTHF = new float[3];
    private float[] ecartTypeBasse = new float[4];
    private float[] ecartTypeHaute = new float[4];
    private float[] ecartTypeTheta = new float[4];
    private float[] ecartTypeTHF = new float[3];
    //private float[] tabVal = new float[12];
    private float[] tabVal1 = new float[12];
    private float[] THF = new float[3];
    private float[] THF1 = new float[3];

    private float alphaRadius;
    private float alphaTHF;
    private float val0 = 0;
    private float val1 = 0;
    private float val2 = 0;

    private float moyenneGaucheDroite = 0;
    private float valGaucheDroite = 0;
    private float minMoyGD = 0;
    private float maxMoyGD = 0;
    private float minGD = 0;
    private float maxGD = 0;

	private Group[] m_electrodeGroups = new Group[8];
	private Fft m_transFourrier;
}
