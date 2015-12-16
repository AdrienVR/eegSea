using UnityEngine;
using System.Collections;

public class MoodLight : MonoBehaviour
{
    public PlusieurVagues PlusieurVagues;
    public CameraOscillations cam;
    public Color leftColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color rightColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

    // Use this for initialization
    void Start()
    {
        m_light = GetComponent<Light>();
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
        //transform.eulerAngles.Set(transform.eulerAngles.x, cam.transform.eulerAngles.y,transform.eulerAngles.z);
        m_coeff = 0.5f;
        m_light.color = (leftColor * m_coeff + rightColor * (1f - m_coeff)) * cam.speed / cam.maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        m_coeff = PlusieurVagues.SeaDataManager.GetLightCoefficient(m_coeff);

        float speed = cam.speed / cam.maxSpeed;

        if (PlusieurVagues.SeaDataManager.Delta != 0)
        {
            speed = Mathf.Sqrt(speed);
        }

        m_light.color = (leftColor * m_coeff + rightColor * (1f - m_coeff)) * speed;
    }

    private Light m_light;
    private float m_coeff = 0.5f;
}
