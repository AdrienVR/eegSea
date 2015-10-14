using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class SkullMenu : MonoBehaviour
{

    public GameObject[] Toggles;
	public GameObject dropdown, dropdown2, addUI, addUI2, field, field2, dialog, dialog2, question, question2;
    public GameObject freq_m, freq_M;
	public Button validBtn;
    private List<int> elecs_on;
    private Dictionary<int, List<int>> groups, freqs;
    string path = "Assets/MerMiroir/Config/groups.txt";
	string pathFreqs = "Assets/MerMiroir/Config/freqs.txt";
	
	// Use this for initialization
    void Start()
    {
        elecs_on = new List<int>();
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
	public void testValid()
	{
		bool result;
		bool cond1 = (freq_m.GetComponent<InputField> ().text != "");
		bool cond2 = (freq_M.GetComponent<InputField> ().text != "");
		bool cond3 = (elecs_on.Count != 0);
		if (cond1 && cond2 && cond3)
		{
			validBtn.interactable=true;
		}
		else
		{
			validBtn.interactable=false;
		}
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
		bool cond1 = (freq_m.GetComponent<InputField> ().text != "");
		bool cond2 = (freq_M.GetComponent<InputField> ().text != "");
		if (cond1 && cond2 && name != "")
		{
			string textQ = "Ajouter ces fréquences sous le nom '" + name + "' ?";
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
        dropdown.GetComponent<Dropdown>().value = groups.Count;
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
		dropdown2.GetComponent<Dropdown>().value = freqs.Count;
	}

	public void addConfig()
	{
		Group g = new Group (elecs_on, int.Parse (freq_m.GetComponent<InputField> ().text), int.Parse (freq_M.GetComponent<InputField> ().text));
		Debug.Log (g.getText());
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
    }


	// Function used to get the frequences that are saved into the config file
	bool getFreqs()
	{
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
