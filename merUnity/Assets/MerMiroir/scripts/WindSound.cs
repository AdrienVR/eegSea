using UnityEngine;
using System.Collections;


public class WindSound : MonoBehaviour
{
    public SeaDataManager SeaDataManager;
    public float av;
    public float moyMax;

    void Start()
    {
        av = 0.1f;
        moyMax = 10f;
        m_audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        av = 0.9f * av + 0.1f * SeaDataManager.GetTHF(2);
        if (av > 1)
        {
            moyMax = 0.995f * moyMax + 0.005f * av;
        }
        if (av > moyMax)
            moyMax = av;
        m_audioSource.volume = (av / moyMax) * (av / moyMax);
        m_audioSource.pitch = 0.85f + 0.3f * m_audioSource.volume;
    }

    private AudioSource m_audioSource;
}

