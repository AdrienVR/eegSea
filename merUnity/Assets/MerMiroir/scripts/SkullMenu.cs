using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SkullMenu : MonoBehaviour
{

    public GameObject[] Toggles;
    private List<int> elecs_on;
    private Dictionary<int, List<int>> groups;

    // Use this for initialization
    void Start()
    {
        elecs_on = new List<int>();
        groups = new Dictionary<int, List<int>>
        {
            {0,new List<int> {7, 8,9,10}},
            {1,new List<int> {3, 4,5,6}},
            {2,new List<int> {13,12,11}},
            {3,new List<int> {0,1,2}}
        };
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
