using UnityEngine;
using System.Collections;

public abstract class WaveDescriptor
{
    public abstract void UpdateRadius(float[] frequencies, float[] amplitudes);

    public virtual float GetRadius()
    {
        return m_radius * m_maxRadius;
    }

    protected float m_radius;
    protected float m_maxRadius;
}
