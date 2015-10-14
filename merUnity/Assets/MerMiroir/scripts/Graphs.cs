using UnityEngine;
using System.Collections;

public class Graphs : MonoBehaviour {

    public float speed = 0.1f;

    public float val1;
    public AnimationCurve courbe1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        currentTimer+=Time.deltaTime;
        courbe1.AddKey(currentTimer, val1);
	
	}
    private float currentTimer;
}
