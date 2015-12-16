
public abstract class WaveDescriptor
{
    public virtual float GetRadius()
    {
        return m_radius * MaxRadius;
    }

    protected float m_radius;
    public float MaxRadius = 3.5f;
}
