using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour
{
    // Singleton
    public static GraphManager Instance { get { return s_instance; } }

    public GameObject CurvesParent;
    public float CurveSpeed = 1;

    public GameObject CurveReference;

	public Vector2 CornerOffset;
	public Vector2 BetweenEachGraphOffset;
    public float MaxLength;

	public int NGraphs = 4;

    void Awake()
    {
        s_instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurvesParent.activeSelf)
        {
            CurvesParent.transform.localPosition += Vector3.right * CurveSpeed * Time.deltaTime;

            int index = 0;
            foreach (Curve curve in m_curves)
            {
				curve.UpdatePositions();
                index++;
            }
        }
	}
	
	public void CreateNCurve(int n)
	{
		foreach(GameObject go in m_graphs)
		{
			Destroy(go);
		}
		m_curves = new List<Curve>();
		m_graphs = new List<GameObject>();
		for (int i = 0; i < n; i++)
		{
			GameObject newGraph = Instantiate<GameObject>(CurveReference);
			newGraph.SetActive(true);
			newGraph.transform.parent = CurvesParent.transform;
			newGraph.transform.localPosition = Vector3.up * -50 * i;
			m_graphs.Add(newGraph);
		}

		ComputeRects(n);
	}
	
	public void SetCurveValue(int curveIndex, float value)
	{
		m_curves[curveIndex].SetValue(value);
	}
	
	public void SwitchGraph()
	{
		CurvesParent.SetActive(!CurvesParent.activeSelf);
	}

    private void ComputeRects(int maxGraphs)
    {
        int cameraIndex = 0;
        foreach(GameObject newGraph in m_graphs)
        {
            Camera graphCam = newGraph.GetComponent<Camera>();
            if (graphCam != null)
            {
                graphCam.rect = ComputeRect(cameraIndex, maxGraphs);
			}
			Curve curve = newGraph.GetComponent<Curve>();
			m_curves.Add(curve);
			cameraIndex++;
			curve.GraphName.text = cameraIndex.ToString();
        }
    }

    private Rect ComputeRect(int currentIndex, int maxGraphs)
    {
        int lines = Mathf.RoundToInt(Mathf.Sqrt(maxGraphs));
		int raws = (int)Mathf.Round(maxGraphs / lines);

		m_curveSize = new Vector2((1 - 2 * CornerOffset.x - (raws - 1) * BetweenEachGraphOffset.x) / ((float)raws), 
		                          (1 - 2 * CornerOffset.y - (lines - 1) * BetweenEachGraphOffset.y) / ((float)lines));

		int lineIndex = Mathf.RoundToInt(currentIndex / raws);
		int rawIndex = currentIndex % raws;

		int spaceX = Mathf.Max(0, rawIndex);
		int spaceY = Mathf.Max(0, lineIndex);

		return new Rect(CornerOffset.x + rawIndex * m_curveSize.x + spaceX * BetweenEachGraphOffset.x,
            1 - CornerOffset.y - (lineIndex + 1) * m_curveSize.y - spaceY * BetweenEachGraphOffset.y,
            m_curveSize.x,
            m_curveSize.y);
    }

    private Vector2 m_curveSize;

	private List<GameObject> m_graphs = new List<GameObject>();
    private List<Curve> m_curves = new List<Curve>();

    private static GraphManager s_instance;
}
