using UnityEngine;
using System;

public class SeaManager : MonoBehaviour
{
    public MeshRenderer SeaRenderer;
    public WaveParameter[] WaveParameters;

    public float AlphaRadius;

    // Use this for initialization
    void Awake ()
    {
        m_seaMaterial = SeaRenderer.material;
        if (m_seaMaterial.HasProperty("_WavesAmount") == false ||
            m_seaMaterial.HasProperty("_windDir") == false)
        {
            Debug.LogError("The SeaRenderer associated doesn't have the right Material Shader");
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_waveDescriptors.Length && m_waveDescriptors[i] != null; ++i)
        {
            WaveParameter waveParameter = WaveParameters[i];
            float r = m_waveDescriptors[i].GetRadius();
            float alpha = Mathf.Min(1, Time.deltaTime / (AlphaRadius * waveParameter.period));
            waveParameter.radius *= (1 - alpha);
            waveParameter.radius += alpha * r;
            SetWaveParameters(i, waveParameter);
        }
    }

    public void SetWaveParameters(int i, WaveParameter parametres)
    {
        string propertyName = "_Wave" + (i + 1).ToString() + "Parameters";
        if (m_seaMaterial.HasProperty(propertyName))
        {
            Vector4 propertyValue = new Vector4(parametres.angle * Mathf.Deg2Rad,
                                                parametres.radius,
                                                parametres.period,
                                                parametres.waveLenght);
            m_seaMaterial.SetVector(propertyName, propertyValue);
        }
        propertyName = "_Wave" + (i + 1).ToString() + "GroupParam"; // OK for seaShader version 4
        if (m_seaMaterial.HasProperty(propertyName))
        {
            Vector4 propValue = new Vector4(parametres.density, parametres.advance);
            m_seaMaterial.SetVector(propertyName, propValue);
        }
        propertyName = "_Wave" + (i + 1).ToString() + "density"; // OK for seaShader version 3
        if (m_seaMaterial.HasProperty(propertyName))
        {
            float density = parametres.density;
            m_seaMaterial.SetFloat(propertyName, density);
        }
        propertyName = "_Wave" + (i + 1).ToString() + "advance"; // OK for seaShader version 3
        if (m_seaMaterial.HasProperty(propertyName))
        {
            float advance = parametres.advance;
            m_seaMaterial.SetFloat(propertyName, advance);
        }


    }

    // Shader Version 3
    public void SetCoefHF(float coef, string i)
    {
        string propertyName = "_coefHF" + i;
        if (m_seaMaterial.HasProperty(propertyName))
        {
            m_seaMaterial.SetFloat(propertyName, coef);
        }
    }

    // Shader Version 4
    public void SetCoeffsHF(Vector4 coeffs)
    {
        string propertyName = "_coeffsHF";
        if (m_seaMaterial.HasProperty(propertyName))
        {
            m_seaMaterial.SetVector(propertyName, coeffs);
        }
    }

    public void SetWavesAmount(float amount)
    {
        if (m_seaMaterial.HasProperty("_WavesAmount")) m_seaMaterial.SetFloat("_WavesAmount", amount);
    }

    public void SetWindDirection(float windDir)
    {
        if (m_seaMaterial.HasProperty("_windDir")) m_seaMaterial.SetFloat("_windDir", windDir * Mathf.Deg2Rad);
    }

    public void SetWaveDescriptors(WaveDescriptor[] waveDescriptors)
    {
        m_waveDescriptors = waveDescriptors;
    }

    private WaveDescriptor[] m_waveDescriptors;

    private Material m_seaMaterial;
}
