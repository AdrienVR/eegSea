using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Curve : MonoBehaviour
{

    public TrailRenderer TrailRenderer;

	public Text Value;
	public Text GraphName;

	public RectTransform TrailedPixel;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // MAX
        if (m_currentMaxValue == null || m_value > m_currentMaxValue.Value)
        {
            m_currentMaxValue = new MinMaxValue(m_value, m_valueDuration);
        }

        m_maxValue = m_currentMaxValue.Value;
        m_currentMaxValue.Update(Time.deltaTime);

        if (m_currentMaxValue == null)
            m_currentMaxValue = new MinMaxValue(LookForMaxValue(), m_valueDuration);

        // MIN
        if (m_currentMinValue == null || m_value < m_currentMinValue.Value)
        {
            if (m_currentMaxValue.Value != m_value)
                m_currentMinValue = new MinMaxValue(m_value, m_valueDuration);
            else
                m_currentMinValue = new MinMaxValue(0, m_valueDuration);
        }

        m_minValue = m_currentMinValue.Value;
        m_currentMinValue.Update(Time.deltaTime);

        if (m_currentMinValue == null)
            m_currentMinValue = new MinMaxValue(LookForMinValue(), m_valueDuration);
    }
	
	void LateUpdate()
	{
		if (TrailRenderer.enabled == false)
			TrailRenderer.enabled = true;
	}

    public void SetValue(float value)
    {
		m_value = value;
        Value.text = value.ToString();
        m_values.Add(value);
    }

    public void UpdatePositions()
    {
		Debug.Log (m_value);
		if (TrailedPixel != null)
			TrailedPixel.localPosition = new Vector3 (TrailedPixel.localPosition.x + 0, 
			                                          m_value,//TrailedPixel.localPosition.y + m_value,
			                                          TrailedPixel.localPosition.z + 0);
		// * m_value;// + Vector3.right * 248;//(m_value - m_minValue)/(m_maxValue);
    }

    private float LookForMaxValue()
    {
        float max = 0;
        foreach(float value in m_values)
        {
            if (value != m_maxValue && value>max)
            {
                max = value;
            }
        }
        return max;
    }

    private float LookForMinValue()
    {
        float min = 0;
        foreach (float value in m_values)
        {
            if (value != m_minValue && value < min)
            {
                min = value;
            }
        }
        return min;
    }

    private float m_value;
    private float m_maxValueEver;

    private MinMaxValue m_currentMaxValue;

    private float m_valueDuration = 1;

    private float m_minValue;
    private float m_maxValue;

    private MinMaxValue m_currentMinValue;

    private SlidingBuffer<float> m_values = new SlidingBuffer<float>(10);

    public class MinMaxValue : Object
    {
        public float Value;

        public MinMaxValue(float value, float lifeTime)
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

    class SlidingBuffer<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _maxCount;

        public SlidingBuffer(int maxCount)
        {
            _maxCount = maxCount;
            _queue = new Queue<T>(maxCount);
        }

        public void Add(T item)
        {
            if (_queue.Count == _maxCount)
                _queue.Dequeue();
            _queue.Enqueue(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
