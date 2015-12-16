using UnityEngine;
using System;

public class PlaceboDataManager : SeaDataManager
{
    [Serializable]
    public class RandomDescriptor : WaveDescriptor
    {
        public override float GetRadius()
        {
            return UnityEngine.Random.Range(0f, 3.5f);
        }
    }

    public RandomDescriptor[] Groups;

    public override WaveDescriptor[] GetWaveDescriptors()
    {
        return Groups;
    }

    //rugosité de la texture de la mer
    public override float GetTHF(int index)
    {
        return GetRandomValue();
    }

    public override float GetOscillationLeft()
    {
        return GetRandomValue();
    }

    public override float GetOscillationRight()
    {
        return GetRandomValue();
    }

    public override float GetLightCoefficient(float lastCoef)
    {
        return GetRandomValue();
    }

    private float GetRandomValue()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }
}
