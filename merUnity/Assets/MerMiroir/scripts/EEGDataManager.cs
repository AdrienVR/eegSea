using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class EEGDataManager : SeaDataManager
{

    public float alphaRadius; // pourcentage de la période des vagues pour temps moyen de maj des hauteurs
    public float alphaTHF; // temps moyen de maj pour le coeff du bumpmap (texture des vagues de hautes fréquences)

    public float Delta { get { return maxGD - minGD; } }
	
	public string[] GroupsForWaves = new string[8];
	public string GroupForTextureW;
	public string GroupForTextureY;
	public string GroupForTextureZ;

    public float memAlphaRadius;

	//[HideInInspector]
	public Group[] ElectrodeGroups = new Group[8];
	public Group[] ThfElectrodes = new Group[3];


    void Awake()
    {
        m_transFourrier = new Fft();
    }

    void Start()
    {

        //		perlinVagues = gameObject.GetComponentsInChildren<PerlinVague>();
        Initialize(alphaRadius, alphaTHF); //Appeler init une seulf fois dans le programme	//Given param: alpha radius & alpha THF

        // Initialisation des groupes depuis les fichiers .xml
		m_allGroups = new List<Group> ();
        InitGroups();

		// Selection des groupes suivant les infos saisies par l'utilisateur dans UnityEditor
		SortGroups ();
    }

    public override WaveDescriptor[] GetWaveDescriptors()
    {
        return ElectrodeGroups;
    }

    public void sendTimeSamples(float[] sensorValues)
    {
        m_transFourrier.addSample(sensorValues);
    }

    public void UpdateFFT()
    {
        float[][] fft = m_transFourrier.GetFT();
        foreach (Group group in ElectrodeGroups)
        {
            group.UpdateRadius(fft);
        }
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

    public override float GetLightCoefficient(float lastCoef)
    {

        MoyenneGD();
        float delta = Delta;

        if (delta == 0)
        {
            return lastCoef;
        }

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
        return ThfElectrodes[0].GetRadius();
    }

    public override float GetOscillationRight()
    {
        return ThfElectrodes[1].GetRadius();
    }

    public void MoyenneGD() //différence gauche droite
    {
        float moyenneGauche;
        float moyenneDroite;
        float valGauche;
        float valDroite;

        moyenneGauche = ThfElectrodes[0].GetRadius();// moyenneBasse[0] + moyenneBasse[2] + moyenneHaute[0] + moyenneHaute[2] + moyenneTheta[0] + moyenneTheta[2];
        moyenneDroite = ThfElectrodes[1].GetRadius();// moyenneBasse[1] + moyenneBasse[3] + moyenneHaute[1] + moyenneHaute[3] + moyenneTheta[1] + moyenneTheta[3];
        valGauche = ThfElectrodes[0].GetRadius();// basseFrequence[0] + basseFrequence[2] + hauteFrequence[0] + hauteFrequence[2] + thetaFrequence[0] + thetaFrequence[2];
        valDroite = ThfElectrodes[2].GetRadius();//basseFrequence[1] + basseFrequence[3] + hauteFrequence[1] + hauteFrequence[3] + thetaFrequence[1] + thetaFrequence[3];
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

    public override float GetTHF(int coef)
    {
        return ThfElectrodes[coef].GetRadius();
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
				m_allGroups.Add(g);
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
    }

	public void SortGroups()
	{
		Dictionary<string, int> groups = new Dictionary<string, int> ();
		int index = 0;
		foreach(Group g in m_allGroups)
		{
			groups.Add(g.getName().ToLower(), index);
			index++;
		}
		List<string> names = new List<string> ();
		foreach(string key in groups.Keys)
		{
			names.Add(key.ToString());
		}
		index = 0;
		int lastMatching = -1;
		foreach(string s in GroupsForWaves)
		{
			if(index<8)
			{
				if(names.Contains(s.ToLower()))
				{
					int indexGroup = groups[s.ToLower()];
					lastMatching = indexGroup;
					Group matchingGroup = m_allGroups[indexGroup];
					ElectrodeGroups[index] = matchingGroup;
				}
				else
				{
					Debug.LogError("Group '"+s+"' is not present in Xml files.");
					if(lastMatching>=0)
					{
						ElectrodeGroups[index] = m_allGroups[lastMatching];
					}
					else
					{
						ElectrodeGroups[index] = m_allGroups[0];
					}
					Debug.LogError("'"+s+"' have been replaced by '"+ElectrodeGroups[index].getName()+"' for wave "+index+".");
				}
				index++;
			}
		}
		for(int i=index; i < 8; i++)
		{
			if(lastMatching>=0)
			{
				ElectrodeGroups[i] = m_allGroups[lastMatching];
			}
			else
			{
				ElectrodeGroups[i] = m_allGroups[0];
			}
		}

		// GROUPS FOR TEXTURES :
		lastMatching = -1;
		if (GroupForTextureW == null)
			GroupForTextureW = "default";
		if (GroupForTextureY == null)
			GroupForTextureY = "default";
		if (GroupForTextureZ == null)
			GroupForTextureZ = "default";
		if(names.Contains(GroupForTextureW.ToLower()))
		{
			int indexGroup = groups[GroupForTextureW.ToLower()];
			lastMatching = indexGroup;
			ThfElectrodes[0] = m_allGroups[indexGroup];;
		}
		else
		{
			Debug.LogError("Group '"+GroupForTextureW+"' is not present in Xml files.");
			if(lastMatching>=0)
				ThfElectrodes[0] = m_allGroups[lastMatching];
			else
			{
				ThfElectrodes[0] = m_allGroups[0];
				Debug.LogError("'"+GroupForTextureW+"' have been replaced by '"+ThfElectrodes[0].getName()+"' for texture W");
			}
		}
		if(names.Contains(GroupForTextureY.ToLower()))
		{
			int indexGroup = groups[GroupForTextureY.ToLower()];
			lastMatching = indexGroup;
			ThfElectrodes[1] = m_allGroups[indexGroup];;
		}
		else
		{
			Debug.LogError("Group '"+GroupForTextureY+"' is not present in Xml files.");
			if(lastMatching>=0)
				ThfElectrodes[1] = m_allGroups[lastMatching];
			else
			{
				ThfElectrodes[1] = m_allGroups[0];
				Debug.LogError("'"+GroupForTextureY+"' have been replaced by '"+ThfElectrodes[1].getName()+"' for texture Y");
			}
		}
		if(names.Contains(GroupForTextureZ.ToLower()))
		{
			int indexGroup = groups[GroupForTextureZ.ToLower()];
			lastMatching = indexGroup;
			ThfElectrodes[2] = m_allGroups[indexGroup];;
		}
		else
		{
			Debug.LogError("Group '"+GroupForTextureZ+"' is not present in Xml files.");
			if(lastMatching>=0)
				ThfElectrodes[2] = m_allGroups[lastMatching];
			else
			{
				ThfElectrodes[2] = m_allGroups[0];
				Debug.LogError("'"+GroupForTextureZ+"' have been replaced by '"+ThfElectrodes[2].getName()+"' for texture Z");
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

    public float[][] GetFT()
    {
        return m_transFourrier.GetFT();
    }

	private List<Group> m_allGroups;
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
