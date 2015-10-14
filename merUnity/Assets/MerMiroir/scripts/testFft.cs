using UnityEngine;
using System.Collections;

public class testFft : MonoBehaviour {
    private Fft fft;
    public double entree;
	// Use this for initialization
	void Start () {
	this.fft=new Fft(); 
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            fft.update(entree);
            for (int i = 0; i < 16;i++ )
                Debug.Log(fft.output[i]);
        }
	
	}
}
