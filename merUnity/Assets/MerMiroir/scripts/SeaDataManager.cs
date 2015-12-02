using UnityEngine;
using System.Collections;
using System;

public abstract class SeaDataManager : MonoBehaviour
{

    //rugosité de la texture de la mer
    public abstract float getTHF(string coef1);

    public abstract float GetOscillationLeft();

    public abstract float GetOscillationRight();
}
