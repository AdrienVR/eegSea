using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour
{
    // Singleton
    public static GraphManager Instance { get { return s_instance; } }

    public GameObject Curve;
    public float CurveSpeed = 1;

    public GameObject CurveReference;

    public Vector2 CornerOffset;
    public float MaxLength;

    void Awake()
    {
        s_instance = this;
    }

    public void CreateNCurve(int n)
    {

    }

    public void SetCurveValue(int curveIndex, float value)
    {
        m_curveNextValues[curveIndex] = value;
    }

    public void SwitchGraph()
    {
        Curve.SetActive(!Curve.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        if (Curve.activeSelf)
        {
            Curve.transform.localPosition += Vector3.right * CurveSpeed * Time.deltaTime;

            int index = 0;
            foreach (Curve curve in m_curves)
            {
                curve.transform.localPosition = Vector3.up * m_curveNextValues[index];
                index++;
            }
        }
    }

    private List<Rect> ComputeRects(int maxGraphs)
    {
        for (int i = 0; i < maxGraphs; i++)
        {

        }
        return null;
    }

    private Rect ComputeRect(int currentIndex, int maxGraphs)
    {
        int lines = Mathf.RoundToInt(Mathf.Sqrt(maxGraphs));
        int raw = lines + 1;

        m_curveSize = new Vector2(1 - 2 * CornerOffset.x, 1 - 2 * CornerOffset.y);

        int lineIndex = Mathf.RoundToInt(currentIndex / lines);
        int rawIndex = currentIndex % lines;

        return new Rect(CornerOffset.x + rawIndex * m_curveSize.x,
            CornerOffset.y + lineIndex * m_curveSize.y,
            m_curveSize.x,
            m_curveSize.y);
    }

    private Vector2 m_curveSize;

    private List<Curve> m_curves = new List<Curve>();
    private List<float> m_curveNextValues = new List<float>();

    private static GraphManager s_instance;

    private static Dictionary<int, List<Rect>> s_cameraRects = new Dictionary<int, List<Rect>>
    {
        {4, new List<Rect>{}}
    };
}
