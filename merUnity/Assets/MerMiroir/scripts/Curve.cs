using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Curve : MonoBehaviour
{

    public RectTransform TrailObjectTransform;
    public Text ObjectValue;

    public float MaxValueDuration = 1;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentMaxValue == null || m_value > m_currentMaxValue.Value)
        {
            m_currentMaxValue = new MaxValue(m_value, MaxValueDuration);
            m_currentMaxValue.Update(Time.deltaTime);
        }
    }

    public void UpdateValue(float value)
    {
        ObjectValue.text = value.ToString();
    }

    private float m_value;
    private float m_maxValueEver;

    private TrailRenderer m_trailRenderer;

    private MaxValue m_currentMaxValue;

    public class MaxValue : Object
    {
        public float Value;

        public MaxValue(float value, float lifeTime)
        {
            Value = value;
            m_lifeTime = lifeTime;
        }

        public void Update(float deltaTime)
        {
            m_lifeTime -= deltaTime;
            if (m_lifeTime < 0)
                Object.Destroy(this);
        }

        private float m_lifeTime;
    }
}
