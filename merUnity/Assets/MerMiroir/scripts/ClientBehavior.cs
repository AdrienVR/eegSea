using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class ClientBehavior : MonoBehaviour {

    // Singleton
    public static ClientBehavior Instance;

    public int numberValuesReceived = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            new EEGDataManager();
            Initialize();
        }
    }

    public void Initialize()
    {
        timestampStartProgram = Time.realtimeSinceStartup;
        lastTimestampReceived = Time.realtimeSinceStartup;

        client = new UdpClient(port);
        client.Client.ReceiveTimeout = 5000;

        Debug.Log("Starting program at " + timestampStartProgram);

       LaunchThread();

    }

    public void LaunchThread()
    {
        if (m_threadEnabled == false)
        {
            m_threadEnabled = true;
            StartCoroutine(ReceiveData());
        }
    }

    public void StopThread()
    {
        m_threadEnabled = false;
    }

    public void Disable() //ferme le thread
    {
        double dateRunProgram = ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timestampStartProgram) / 1000;
        Debug.Log("Client have ran for " + dateRunProgram + "seconds and have received " + numberValuesReceived + "dataValues within this time!");
        double frequency = numberValuesReceived / (dateRunProgram);
        Debug.Log("Approximate frequency : " + frequency + " Values/seconds");

        client.Close();
    }

    private IEnumerator ReceiveData()
    {
        while (m_threadEnabled == true)
        {
            try
            {
                IPEndPoint receiver = new IPEndPoint(IPAddress.Any, 0); //spécification du port et de l'adresse IP a écouter
                byte[] receivedBytes = client.Receive(ref receiver); //récéption de la trame UDP
                data = Encoding.ASCII.GetString(receivedBytes); //transformer la trame en chaine 

                if (data.Length > 5)
                {
                    //Debug.Log (data.ToString());
                    string[] split = data.Split(new char[] { ';' });

                    EEGDataManager.Instance.Update(split);

                    lastDataReceived = data;
                    numberValuesReceived += 1;
                    lastTimestampReceived = Time.realtimeSinceStartup;
                }
            }
            catch (Exception Err)
            {
                Debug.Log(Err.ToString());

                Debug.Log("No date since " + ((Time.realtimeSinceStartup) - lastTimestampReceived) + " s, Transmiting 0 values ...");

               // if (((Time.realtimeSinceStartup) - lastTimestampReceived) > 1) 
				//   EEGData.Instance.ResetValues();
            }
            yield return new WaitForSeconds(.05f);
		}
		
		yield break;
    }

    private bool m_threadEnabled = false;

    // Use this for initialization
    private string data = "";
    private UdpClient client;
    private int port = 5000;
    private byte[] receivedBytes;

    private string lastDataReceived = "No Data";

    private float lastTimestampReceived = 0;
    private float timestampStartProgram = 0;
}
