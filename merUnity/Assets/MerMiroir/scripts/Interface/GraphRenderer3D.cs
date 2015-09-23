using UnityEngine;
using System.Collections;

public class GraphRenderer3D : MonoBehaviour
{
    public float RealtimeValue;
    public Transform child;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Speed * Time.deltaTime * Vector3.right;
        child.transform.localPosition = Vector3.up * RealtimeValue;
    }
    public float Speed;
}
