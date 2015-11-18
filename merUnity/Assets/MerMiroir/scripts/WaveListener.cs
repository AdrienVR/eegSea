
using System;
using UnityEngine;

[Serializable]
// INTERFACE NOT SERIALIZABLE
//public interface IWaveListener
//{
  //  void SetWave(WaveDescriptor[] waveDescriptors);
//}
public abstract class WaveListener : MonoBehaviour
{
    public abstract void SetWave(WaveDescriptor[] waveDescriptors);
}
