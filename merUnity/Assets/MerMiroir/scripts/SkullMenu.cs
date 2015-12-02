using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class SkullMenu : MonoBehaviour
{

    public GameObject[] Toggles;
	public GameObject dropdown, dropdown2, addUI, addUI2, field, field2, dialog, dialog2, dialog3, question, question2, rightPanel;
    public GameObject freq_m, freq_M, configName;
	public Button validBtn, validBtn2;
    private List<int> elecs_on;
    private Dictionary<int, List<int>> groups, freqs;
	public List<Group> config_list;
	string folder;
	string path, pathFreqs;

	// Use this for initialization
    void Start()
    {
		folder = Path.Combine (Application.dataPath, "Config");
		path = Path.Combine(folder, "groups.txt");
		pathFreqs = Path.Combine(folder, "freqs.txt");
        elecs_on = new List<int>();
		config_list = new List<Group> ();
        initGroups();
    }

    // Callback function for electrode buttons 1 to 14
    public void changeElec(int nb)
    {
        if (!elecs_on.Contains(nb))
        {
            elecs_on.Add(nb);
            //Debug.Log("Electrode " + nb + " ajoutée.");
        }
        else
        {
            elecs_on.Remove(nb);
            //Debug.Log("Electrode " + nb + " retirée.");
        }
		testValid ();
    }

    // Callback function for group dropdown
    public void selectGroup(System.Int32 nb)
    {
        //Debug.Log("Groupe "+ nb +" selectionné. ");
        for(int i=0; i<14;i++)
        {
            switchOff(i);
        }
        foreach(int i in groups[nb])
        {
            switchOn(i);
        }
    }

	// Callback function for frequences dropdown
	public void selectFrequence(System.Int32 nb)
	{
		freq_m.GetComponent<InputField> ().text = freqs [nb][0].ToString();
		freq_M.GetComponent<InputField> ().text = freqs [nb][1].ToString();
	}
	
	// Callback function for plus button
	public void displayAddUI()
	{
        addUI.SetActive(true);
    }

    public void hideAddUI()
    {
        addUI.SetActive(false);
    }
	public void displayAddUI2()
	{
		addUI2.SetActive(true);
	}
	
	public void hideAddUI2()
	{
		addUI2.SetActive(false);
	}

    public void displayDialog()
    {
        dialog.SetActive(true);
    }

    public void hideDialog()
    {
        dialog.SetActive(false);
    }
	public void displayDialog2()
	{
		dialog2.SetActive(true);
	}
	
	public void hideDialog2()
	{
		dialog2.SetActive(false);
	}
	public void displayDialog3()
	{
		dialog3.SetActive(true);
	}
	
	public void hideDialog3()
	{
		dialog3.SetActive(false);
	}
	public void testValid()
	{
		bool result;
		bool cond1 = (freq_m.GetComponent<InputField> ().text != "");
		bool cond2 = (freq_M.GetComponent<InputField> ().text != "");
		bool cond3 = (elecs_on.Count != 0);
		bool cond4 = (config_list.Count < 8);
		bool cond5 = (config_list.Count > 0 && config_list.Count <9);
		if (cond1 && cond2 && cond3 && cond4)
		{
			validBtn.interactable=true;
		}
		else
		{
			validBtn.interactable=false;
		}
		if (cond5)
		{
			validBtn2.interactable = true;
		}
		else
		{
			validBtn2.interactable=false;
		}

	}

	public void Valid()
	{
		// Ecriture du fichier XML correspondant
		string path = "Assets/MerMiroir/Config/config" + System.DateTime.Now.ToString ().Replace ('/', '_').Replace (' ', '_').Replace (':', '_') + ".xml";
		XmlWriter writer = new XmlWriter (path);
		writer.WriteStartDocument ();
		writer.WriteStartElement ("config");
			foreach(Group g in config_list)
			{
				writer.WriteStartElement ("group");
					writer.WriteElementString("name", g.getName());
					writer.WriteElementString("f_min", g.getFMin().ToString());
					writer.WriteElementString("f_max", g.getFMax().ToString());
					writer.WriteStartElement ("electrodes_list");
						foreach(int e in g.getElecs())
						{
							writer.WriteElementString("electrode", Group.ElecNames[e]);
						}
					writer.WriteEndElement("electrodes_list");
				writer.WriteEndElement("group");
			}
		writer.WriteEndElement("config");
		writer.WriteEndDocument ();
		// fin de l'application
		Application.Quit ();
		Debug.Log ("The configuration have been correctly exported to XML file.");
	}

    // Callback function for Add button
    public void addGroup()
    {
        InputField inF = (InputField)field.GetComponent<InputField>();
        string name = inF.text;
        if (elecs_on.Count > 0 && name != "")
        {
            string textQ;
            if(elecs_on.Count == 1)
                textQ= "Ajouter cette electrode au groupe '" + name + "' ?";
            else
                textQ = "Ajouter ces " + elecs_on.Count + " electrodes au groupe '" + name + "' ?";
            question.GetComponent<Text>().text = textQ;
            displayDialog();
        }
    }

	// Callback function for Add button 2
	public void addFrequence()
	{
		InputField inF = (InputField)field2.GetComponent<InputField>();
		string name = inF.text;
		string f1 = freq_m.GetComponent<InputField> ().text;
		string f2 = freq_M.GetComponent<InputField> ().text;
		bool cond1 = (f1 != "");
		bool cond2 = (f2 != "");
		if (cond1 && cond2 && name != "")
		{
			string textQ = "Ajouter la plage de fréquences ("+f1+" -> "+f2+" Hz) sous le nom '" + name + "' ?";
			question2.GetComponent<Text>().text = textQ;
			displayDialog2();
		}
	}

    public void saveGroup()
    {
        hideDialog();
        hideAddUI();
        InputField inF = (InputField)field.GetComponent<InputField>();
        string name = inF.text;
        string electrodes = "";
        bool first = true;
        foreach(int e in elecs_on)
        {
            if (!first)
                electrodes += ",";
            electrodes += (e-1).ToString();
            first = false;
        }
        string sentence = name + ";" + electrodes + ";";
        StreamWriter sw = File.AppendText(path);
        sw.Write(sentence);
        sw.Close();
        initGroups();
#if UNITY_5_2
        dropdown.GetComponent<Dropdown>().value = groups.Count;
#endif
    }

	public void saveFrequence()
	{
		hideDialog2();
		hideAddUI2();
		InputField inF = (InputField)field2.GetComponent<InputField>();
		string name = inF.text;
		string frequences = freq_m.GetComponent<InputField> ().text+","+freq_M.GetComponent<InputField> ().text;
		string sentence = name + ";" + frequences + ";";
		StreamWriter sw = File.AppendText(pathFreqs);
		sw.Write(sentence);
		sw.Close();
		initGroups();
#if UNITY_5_2
        dropdown2.GetComponent<Dropdown>().value = freqs.Count;
#endif
    }

    public void addConfig()
	{
		if (config_list.Count < 8)
		{
			bool sameName = false;
			string n = configName.GetComponent<InputField> ().text;
			foreach(Group g in config_list)
			{
				if(n == g.getName())
				{
					sameName = true;
				}
			}
			if(!sameName)
			{
				configName.GetComponent<InputField> ().text="";
				Group g = new Group (n, elecs_on, int.Parse (freq_m.GetComponent<InputField> ().text), int.Parse (freq_M.GetComponent<InputField> ().text));
				config_list.Add (g);
				reDrawConfig ();
				hideDialog3 ();
			}
		}
		testValid ();

	}

	public void reDrawConfig()
	{
		foreach (Transform t in rightPanel.transform)
		{
			Destroy(t.gameObject);
		}
		for (int i =0; i <config_list.Count; i++)
		{
			GameObject configUI = (GameObject)Instantiate(Resources.Load ("Config"));
			configUI.transform.SetParent (rightPanel.transform);
			Vector3 delta = new Vector3 (0, -80*i, 0);
			configUI.transform.position = rightPanel.transform.position+delta;
			ConfigUI conf = configUI.GetComponent<ConfigUI> ();
			conf.init (config_list[i], config_list, this);
		}
	}

    void initGroups()
    {
        groups = new Dictionary<int, List<int>>();
        freqs = new Dictionary<int, List<int>>();
        if (!getGroups())
        {
            Debug.LogError("Error while reading electrode groups file.");
        }
		if (!getFreqs())
		{
			Debug.LogError("Error while reading freqs file.");
		}
    }

    // Function used to get the groups that are saved into the config file
    bool getGroups()
    {
#if UNITY_5_2
        bool state = true;
        if (File.Exists(path))
        {
            try
            {
                StreamReader file = File.OpenText(path);
                string[] lines = file.ReadToEnd().Split(';');
                Dropdown d = (Dropdown)dropdown.GetComponent<Dropdown>();
                int size = d.options.Count;
                for (int j = size - 1; j >0 - 1; j--)
                {
                    d.options.RemoveAt(j);
                }
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    d.options.Add(new Dropdown.OptionData(lines[i]));
                    List<int> electrodes = new List<int>();
                    foreach (string s in lines[i + 1].Split(','))
                        electrodes.Add(int.Parse(s));
                    groups.Add(groups.Count, electrodes);
                    i ++;
                }
                file.Close();
            }
            catch(IOException e)
            {
                state = false;
                Debug.LogError(e.Message);
            }
        }
        else
        {
            state = false;
            Debug.LogError("Cannot open file : " + path);
        }
        return state;
#else
        return false;
#endif
    }


    // Function used to get the frequences that are saved into the config file
    bool getFreqs()
    {
#if UNITY_5_2
        bool state = true;
		if (File.Exists(pathFreqs))
		{
			try
			{
				StreamReader file = File.OpenText(pathFreqs);
				string[] lines = file.ReadToEnd().Split(';');
				Dropdown d = (Dropdown)dropdown2.GetComponent<Dropdown>();
				int size = d.options.Count;
				for (int j = size - 1; j >0 - 1; j--)
				{
					d.options.RemoveAt(j);
				}
				for (int i = 0; i < lines.Length - 1; i++)
				{
					d.options.Add(new Dropdown.OptionData(lines[i]));
					List<int> f = new List<int>();
					foreach (string s in lines[i + 1].Split(','))
						f.Add(int.Parse(s));
					freqs.Add(freqs.Count, f);
					i ++;
				}
				file.Close();
			}
			catch(IOException e)
			{
				state = false;
				Debug.LogError(e.Message);
			}
		}
		else
		{
			state = false;
			Debug.LogError("Cannot open file : " + path);
		}
		return state;
#else
        return false;
#endif
    }

    void switchOn(int i)
	{
		Toggle t = (Toggle) Toggles[i].GetComponent<Toggle>();
		t.isOn = true;
	}
	void switchOff(int i)
	{
		Toggle t = (Toggle)Toggles[i].GetComponent<Toggle>();
		t.isOn = false;
	}
	
}