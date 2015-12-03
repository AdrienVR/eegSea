using UnityEngine;
using System.Collections;

public class GrahpTester : MonoBehaviour
{
    public EEGDataManager EEGDataManager;
    public AnimationCurve Curve;
    public AnimationCurve Signal;

    public const float UpdateDelay = 0.1f;

    // Use this for initialization
    void Start()
    {
        GraphManager.Instance.CreateNCurve(1);
        //GraphManager.Instance.CreateNCurve(1);
        
        MakeDeepCopy(EEGDataManager.GetFT(), ref f);
        MakeDeepCopy(EEGDataManager.GetSignal(), ref signal);

        for (int i = 0; i < f[0].Length; i++)
        {
            Curve.AddKey(i, 0);
        }
        for (int i = 0; i < signal[0].Length; i++)
        {
            Signal.AddKey(i, 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (signal == null || signal.Length < 1)
            return;

        m_updateTimer += Time.deltaTime;

        //ClientBehavior.sensorVal[0]++;
        if (m_index > signal[0].Length - 1)
        {
            m_index = 0;
            m_updateTimer = 0;
            MakeDeepCopy(EEGDataManager.GetFT(), ref f);
            MakeDeepCopy(EEGDataManager.GetSignal(), ref signal);

            for (int i = 0; i < f[0].Length; i++)
            {
                Curve.RemoveKey(i);
                Curve.AddKey(i, f[0][i]);
            }
            for (int i = 0; i < signal[0].Length; i++)
            {
                Signal.RemoveKey(i);
                Signal.AddKey(i, signal[0][i]);
            }
        }

        GraphManager.Instance.SetCurveValue(0, (float)signal[0][m_index]);

        if (m_updateTimer > UpdateDelay)
        {
            m_index++;
        }
    }

    private void MakeDeepCopy(float[][] arrayToCopy, ref float[][] arrayTarget)
    {
        arrayTarget = new float[arrayToCopy.Length][];

        for (int i = 0; i < arrayToCopy.Length; i++)
        {
            arrayTarget[i] = new float[arrayToCopy[i].Length];
            for (int j = 0; j < arrayToCopy[i].Length; j++)
            {
                arrayTarget[i][j] =  arrayToCopy[i][j];
            }
        }
    }

    private float m_updateTimer;
    float[][] f;
    float[][] signal;
    private int m_index = 64;
}
