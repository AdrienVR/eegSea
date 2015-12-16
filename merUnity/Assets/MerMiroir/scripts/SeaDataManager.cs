using UnityEngine;
using System.Collections;
using System;

public abstract class SeaDataManager : MonoBehaviour
{
    public float TMoyenne = 10f;
    public float TEcartType = 10f;

    [HideInInspector]
    public float Delta = 0;

    public abstract WaveDescriptor[] GetWaveDescriptors();

    //rugosité de la texture de la mer
    public abstract float GetTHF(int index);

    public abstract float GetOscillationLeft();

    public abstract float GetOscillationRight();

    public abstract float GetLightCoefficient(float lastCoef);
}
