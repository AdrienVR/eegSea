using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class EEGDataManager : SeaDataManager
{
    public float TMoyenne, TEcartType;

    public float alphaRadius; // pourcentage de la période des vagues pour temps moyen de maj des hauteurs
    public float alphaTHF; // temps moyen de maj pour le coeff du bumpmap (texture des vagues de hautes fréquences)

    public float Delta { get { return maxGD - minGD; } }

    public Group[] ElectrodeGroups = new Group[8];

    float memAlphaRadius;

    void Awake()
    {
        m_transFourrier = new Fft();
    }

    void Start()
    {

        //		perlinVagues = gameObject.GetComponentsInChildren<PerlinVague>();
        Initialize(alphaRadius, alphaTHF); //Appeler init une seulf fois dans le programme	//Given param: alpha radius & alpha THF

        // Initialisation des groupes depuis les fichiers .xml
        InitGroups();
    }

    public override WaveDescriptor[] GetWaveDescriptors()
    {
        return ElectrodeGroups;
    }

    public void sendTimeSamples(int[] sensorIndexes, float[] sensorValues)
    {
        m_transFourrier.addSample(sensorValues);
    }

    public float[][] GetSignal()
    {
        return m_transFourrier.getSignal();
    }

    void Update()
    {
        if (alphaRadius != memAlphaRadius)
        {
            alphaRadius = Mathf.Max(0.0001f, alphaRadius);
            SetAlphaRad(alphaRadius);
            memAlphaRadius = alphaRadius;
        }

        //change les coefs des vagues hautes fréquences (texture)
        //calculTHF(maxTHF);

        foreach (Group group in ElectrodeGroups)
        {
            group.MAJmoyenneEcartType(Time.deltaTime);
        }
    }

    public void UpdateValues()
    {
        float[][] amplitudes = new float[14][];

        amplitudes = m_transFourrier.getSignal();

        foreach (Group group in ElectrodeGroups)
        {
            group.UpdateRadius(amplitudes);
        }
    }

    // Use this for initialization
    public void Initialize(float alphaRadiusValue, float alphaTHFValue)
    {

        alphaRadius = alphaRadiusValue;
        alphaTHF = Mathf.Max(0.001f, alphaTHFValue);
    }

    public override bool DeltaLight()
    {
        return Delta > 0;
    }

    public override float GetLightCoefficient(float lastCoef)
    {
        MoyenneGD();
        float delta = Delta;
        float alpha = Mathf.Min(Time.deltaTime, 1f);

        return GetAlpha(lastCoef, alpha, delta);
    }

    public float GetAlpha(float coeff, float alpha, float delta)
    {
        return (1f - alpha) * coeff + alpha * (maxGD - valGaucheDroite) / delta;
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

    public override float GetOscillationLeft()
    {
        return GetBetaValues()[0];
    }

    public override float GetOscillationRight()
    {
        return GetBetaValues()[1];
    }

    public void MoyenneGD() //différence gauche droite
    {
        float moyenneGauche;
        float moyenneDroite;
        float valGauche;
        float valDroite;

        moyenneGauche = 0;// moyenneBasse[0] + moyenneBasse[2] + moyenneHaute[0] + moyenneHaute[2] + moyenneTheta[0] + moyenneTheta[2];
        moyenneDroite = 0;// moyenneBasse[1] + moyenneBasse[3] + moyenneHaute[1] + moyenneHaute[3] + moyenneTheta[1] + moyenneTheta[3];
        valGauche = 0;// basseFrequence[0] + basseFrequence[2] + hauteFrequence[0] + hauteFrequence[2] + thetaFrequence[0] + thetaFrequence[2];
        valDroite = 0;//basseFrequence[1] + basseFrequence[3] + hauteFrequence[1] + hauteFrequence[3] + thetaFrequence[1] + thetaFrequence[3];
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
        return new float[] { 0, 0 };// moyenneHaute;
    }

    public override float getTHF(string coef)
    {
        if (coef == "THF1")
            return THF[0];
        if (coef == "THF2")
            return THF[1];
        if (coef == "THF3")
            return THF[2];
        return 1f; //valeur par défaut

    }

    public void ResetValues()
    {
        for (int i = 0; i < 4; i++)
        {
            //basseFrequence[i] = -40f;
            //hauteFrequence[i] = -40f;
            //thetaFrequence[i] = -40f;
            if (i < 3)
            {
                //  tabTHF[i] = -40f;
            }

        }
    }

    void InitGroups()
    {
        string[] listOfXMLFiles = Directory.GetFiles(Path.Combine(Application.dataPath, "Config"), "*.xml");

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
                g.setMoyenne(TMoyenne);
                g.setEcartType(TEcartType);
                Debug.Log("Import group : " + g.getText());
                if (index < 8)
                {
                    ElectrodeGroups[index] = g;
                    index++;
                }
                /*
				Debug.Log ("__________________________");
				Debug.Log ("name is "+name);
				Debug.Log ("f_min is "+f_min);
				Debug.Log ("f_max is "+f_max);
				Debug.Log("electrodes :");
				foreach(string s in electrodes)
				{
					Debug.Log ("elec is "+s);
				}
				*/
            }
        }
        Debug.Log("Nombre de groupe : " + index);
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

    public float[][] GetFT()
    {
        return m_transFourrier.GetFT();
    }

    private float[] THF = new float[3];
    private float[] THF1 = new float[3];

    private float val0 = 0;
    private float val1 = 0;
    private float val2 = 0;

    private float moyenneGaucheDroite = 0;
    private float valGaucheDroite = 0;
    private float minMoyGD = 0;
    private float maxMoyGD = 0;
    private float minGD = 0;
    private float maxGD = 0;

    private Fft m_transFourrier;
}
