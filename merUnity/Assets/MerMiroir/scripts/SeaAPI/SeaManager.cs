using UnityEngine;
using System;

public class SeaManager : MonoBehaviour
{
    public MeshRenderer SeaRenderer;
    public WaveParameter[] WaveParameters;

    public float AlphaRadius;

    public GameObject WavePrefab;

    public float amount = 1;

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

        if (m_seaMaterial.HasProperty("_WavesAmount")) m_seaMaterial.SetFloat("_WavesAmount", amount);

        for (int i = 0; i < WaveParameters.Length; ++i)
        {
            WaveParameter parametres = WaveParameters[i];
            //new child vague game object
            GameObject go = Instantiate(WavePrefab);
            vague = go.GetComponent<Vague>();
            vague.transform.Rotate(Vector3.up * parametres.angle);
            vague.waveLenght = parametres.waveLenght;
            parametres.period = Mathf.Sqrt(vague.waveLenght / 1.6f);
            vague.period = parametres.period;
            vague.radius = parametres.radius;
            if (parametres.density > 1) parametres.density = 1f;
            if (parametres.density <= 0) parametres.density = 0.001f;
            vague.density = parametres.density;
            if (parametres.advance > Mathf.PI / 2) parametres.advance = Mathf.PI / 2;
            if (parametres.advance < 0) parametres.advance = 0;
            vague.advance = parametres.advance;
            vague.transform.parent = transform;

        }

        vagues = gameObject.GetComponentsInChildren<Vague>();
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

        for (int i = 0; i < WaveParameters.Length; ++i)
        {
            vagues[i].radius = WaveParameters[i].radius;
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

    public void SetWavesAmount()
    {
    }

    public void SetWindDirection(float windDir)
    {
        if (m_seaMaterial.HasProperty("_windDir")) m_seaMaterial.SetFloat("_windDir", windDir * Mathf.Deg2Rad);
    }

    public void SetWaveDescriptors(WaveDescriptor[] waveDescriptors)
    {
        m_waveDescriptors = waveDescriptors;
    }

    public Vector3 CalculeImage(Vector3 startPoint)
    {

        Vector3 position = startPoint;
        foreach (Vague vague in vagues)
        {
            if (vague.gameObject.activeInHierarchy)
            {
                position += vague.CalculeVecteur(startPoint, amount);
            }
        }
        //foreach (PerlinVague perlinVague in perlinVagues) {
        //	if (perlinVague.gameObject.activeInHierarchy) {
        //		position.y = position.y + perlinVague.CalculeHauteur(startPoint,amount);
        //	}
        //}
        return position;
    }

    public void CalculeImage(Vector3 startPoint, out Vector3 imagePoint, out Vector3 normal, out Vector4 tangentX)
    {
        imagePoint = startPoint;
        Vector3 slopes = new Vector3();
        Vector3 vecteur = new Vector3();
        Vector3 oneNormal = new Vector3();
        Vector3 oneSlope = new Vector3();
        foreach (Vague vague in vagues)
        {
            if (vague.gameObject.activeInHierarchy)
            {
                vague.CalculeVecteur(startPoint, out vecteur, out oneNormal, amount);
                imagePoint += vecteur;
                oneSlope.x = -oneNormal.x / oneNormal.y;
                oneSlope.y = 0.0f;
                oneSlope.z = -oneNormal.z / oneNormal.y;
                slopes += oneSlope;
            }
        }
        //TODO : calculer la normale ou la pente des perlinVagues
        //foreach (PerlinVague perlinVague in perlinVagues) {
        //	if (perlinVague.gameObject.activeInHierarchy) {
        //		imagePoint.y = imagePoint.y + perlinVague.CalculeHauteur(startPoint,amount);
        //	}
        //}

        normal.x = -slopes.x;
        normal.y = 1.0f;
        normal.z = -slopes.z;
        normal.Normalize();

        //tangentX.x = 1.0f;
        //tangentX.y = slopes.z+slopes.x;
        //tangentX.z = 1.0f;

        tangentX.x = 1.0f;
        tangentX.y = slopes.x;
        tangentX.z = 0.0f;
        tangentX.w = 1.0f;
        tangentX.Normalize();
    }

    // Use this for initialization
    Vague vague;

    private WaveDescriptor[] m_waveDescriptors;

    private Vague[] vagues;

    private Material m_seaMaterial;
}
