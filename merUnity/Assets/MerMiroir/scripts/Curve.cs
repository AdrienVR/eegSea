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

    public const float FakeMaxValue = 50;
    public const float HighestPositionRatio = 0.85f;

    private float GetHeight(float value)
    {
        if ((GraphManager.Instance.Raws * GraphManager.Instance.Lines) % 2 == 0)
        {
            return value * m_cameraHeight * HighestPositionRatio * m_cameraHeight /
                (2 * m_cameraWidth);
        }
        return value * m_cameraHeight * HighestPositionRatio * m_cameraHeight /
                (m_cameraWidth);
    }

    // Use this for initialization
    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_cameraHeight = Screen.height * m_camera.rect.height;
        m_cameraWidth = Screen.width * m_camera.rect.width;
        Debug.Log(m_cameraHeight);
        m_currentMaxValue = new MaxValue(1, m_valueDuration);
    }

    private float m_cameraHeight;
    private float m_cameraWidth;
    private Camera m_camera;

    // Update is called once per frame
    void Update()
    {
        m_cameraHeight = Screen.height * m_camera.rect.height;
        m_cameraWidth = Screen.width * m_camera.rect.width;

        // MAX
        if (m_currentMaxValue == null || Mathf.Abs(m_value) > Mathf.Abs(m_currentMaxValue.Value))
        {
            m_currentMaxValue = new MaxValue(m_value, m_valueDuration);
        }

        if (m_currentMaxValue.Update(Time.deltaTime) == true)
        {
            m_currentMaxValue = new MaxValue(LookForMaxValue(), m_valueDuration);
        }
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
        float percent = m_value / m_currentMaxValue.AbsValue;

        if (TrailedPixel != null)
			TrailedPixel.localPosition = new Vector3 (TrailedPixel.localPosition.x + 0,
                                                      GetHeight(percent),//m_value,
			                                          TrailedPixel.localPosition.z + 0);
    }

    private float LookForMaxValue()
    {
        float max = 1;
        foreach(float value in m_values)
        {
            if (value != m_currentMaxValue.Value && value>max)
            {
                max = value;
            }
        }
        return max;
    }

    private float m_value;

    private MaxValue m_currentMaxValue;

    private const float m_valueDuration = 10;

    private SlidingBuffer<float> m_values = new SlidingBuffer<float>(10);

    public class MaxValue
    {
        public float Value;
        public float AbsValue;

        public MaxValue(float value, float lifeTime)
        {
            Value = value;
            AbsValue = Mathf.Abs(Value);
            m_lifeTime = lifeTime;
        }

        public bool Update(float deltaTime)
        {
            m_lifeTime -= deltaTime;
            return (m_lifeTime < 0);
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
