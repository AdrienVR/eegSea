using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEditor;

public class SkullMenu : MonoBehaviour
{

    public GameObject[] Toggles;
    public GameObject dropdown, addUI, field, dialog, question;
    public GameObject alpha_m, alpha_M, beta_m, beta_M, gamma1_m, gamma1_M, gamma2_m, gamma2_M, gamma3_m, gamma3_M, theta_m, theta_M;
    private List<int> elecs_on;
    private Dictionary<int, List<int>> groups, freqs;
    string path = "Assets/MerMiroir/Config/groups.txt";

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
        alpha_m.GetComponent<InputField>().text = freqs[nb][0].ToString();
        alpha_M.GetComponent<InputField>().text = freqs[nb][1].ToString();
        beta_m.GetComponent<InputField>().text = freqs[nb][2].ToString();
        beta_M.GetComponent<InputField>().text = freqs[nb][3].ToString();
        gamma1_m.GetComponent<InputField>().text = freqs[nb][4].ToString();
        gamma1_M.GetComponent<InputField>().text = freqs[nb][5].ToString();
        gamma2_m.GetComponent<InputField>().text = freqs[nb][6].ToString();
        gamma2_M.GetComponent<InputField>().text = freqs[nb][7].ToString();
        gamma3_m.GetComponent<InputField>().text = freqs[nb][8].ToString();
        gamma3_M.GetComponent<InputField>().text = freqs[nb][9].ToString();
        theta_m.GetComponent<InputField>().text = freqs[nb][10].ToString();
        theta_M.GetComponent<InputField>().text = freqs[nb][11].ToString();
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

    public void displayDialog()
    {
        dialog.SetActive(true);
    }

    public void hideDialog()
    {
        dialog.SetActive(false);
    }

    // Call function for Add button
    public void addGroup()
    {
        InputField inF = (InputField)field.GetComponent<InputField>();
        string name = inF.text;
        if (elecs_on.Count > 0 && name != "")
        {
            string textQ;
            if(elecs_on.Count == 1)
                textQ= "Ajouter cette electrode et ces fréquences au groupe '" + name + "' ?";
            else
                textQ = "Ajouter ces " + elecs_on.Count + " electrode et ces fréquences au groupe '" + name + "' ?";
            question.GetComponent<Text>().text = textQ;
            displayDialog();
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
        string frequences = "";
        first = true;
        List<int> freqs_list = new List<int>();
        freqs_list.Add(int.Parse(alpha_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(alpha_M.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(beta_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(beta_M.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma1_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma1_M.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma2_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma2_M.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma3_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(gamma3_M.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(theta_m.GetComponent<InputField>().text));
        freqs_list.Add(int.Parse(theta_M.GetComponent<InputField>().text));
        foreach (int f in freqs_list)
        {
            if (!first)
                frequences += ",";
            frequences += f.ToString();
            first = false;
        }
        string sentence = name + ";" + electrodes + ";" + frequences + ";";
        StreamWriter sw = File.AppendText(path);
        sw.Write(sentence);
        sw.Close();
        initGroups();
        dropdown.GetComponent<Dropdown>().value = groups.Count;
    }

    void initGroups()
    {
        groups = new Dictionary<int, List<int>>();
        freqs = new Dictionary<int, List<int>>();
        if (!getGroups())
        {
            Debug.LogError("Error while reading groups file.");
        }
    }

    public void test()
    {
        Debug.Log("test");
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
                for (int i = 0; i < lines.Length - 2; i++)
                {
                    d.options.Add(new Dropdown.OptionData(lines[i]));
                    List<int> electrodes = new List<int>();
                    foreach (string s in lines[i + 1].Split(','))
                        electrodes.Add(int.Parse(s));
                    groups.Add(groups.Count, electrodes);
                    List<int> f = new List<int>();
                    foreach (string s in lines[i+2].Split(','))
                    {
                        f.Add(int.Parse(s));
                    }
                    freqs.Add(groups.Count-1, f);
                    i += 2;
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
