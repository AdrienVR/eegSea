
public abstract class WaveDescriptor
{
    public virtual float GetRadius()
    {
        return m_radius * m_maxRadius;
    }

    protected float m_radius;
    protected float m_maxRadius;
}
