
using System;
using UnityEngine;

[Serializable]
public abstract class WaveListener : MonoBehaviour
{
    public abstract void SetWave(WaveDescriptor[] waveDescriptors);
}
