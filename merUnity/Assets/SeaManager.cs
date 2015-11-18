using UnityEngine;
using System;

public class SeaManager : MonoBehaviour
{
    public MeshRenderer SeaRenderer;

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
	void Update ()
    {

    }

    public void SetShaderWaveParameters(int i, WaveParameter parametres)
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
    public void SetShaderCoefParameters(float coef, string i)
    {
        string propertyName = "_coefHF" + i;
        if (m_seaMaterial.HasProperty(propertyName))
        {
            m_seaMaterial.SetFloat(propertyName, coef);
        }
    }

    public void SetShaderCoeffsParameters(Vector4 coeffs)
    {
        string propertyName = "_coeffsHF";
        if (m_seaMaterial.HasProperty(propertyName))
        {
            m_seaMaterial.SetVector(propertyName, coeffs);
        }
    }

    public void SetAmount(float amount)
    {
        if (m_seaMaterial.HasProperty("_WavesAmount")) m_seaMaterial.SetFloat("_WavesAmount", amount);
    }

    public void SetWind(float windDir)
    {
        if (m_seaMaterial.HasProperty("_windDir")) m_seaMaterial.SetFloat("_windDir", windDir * Mathf.Deg2Rad);
    }

    private Material m_seaMaterial;
}
