using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class ClientBehavior : MonoBehaviour
{

    public EEGDataManager EEGDataManager;

    // Singleton
    public static ClientBehavior Instance;

    public int numberValuesReceived = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
    }

    public void Initialize()
    {
        timestampStartProgram = GetRealTime();
        lastTimestampReceived = GetRealTime();

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
            Application.runInBackground = true;
            m_receiveThread = new Thread(new ThreadStart(ReceiveDataLoop)); //Créer un thread pour écouter le server UDP
            m_receiveThread.IsBackground = true;
            m_receiveThread.Start();
        }
    }

    public void StopThread()
    {
        m_threadEnabled = false;
    }

    public void Disable() //ferme le thread
    {
        double dateRunProgram = (GetRealTime() - timestampStartProgram);
        Debug.Log("Client have ran for " + dateRunProgram + "seconds and have received " + numberValuesReceived + "dataValues within this time!");
        double frequency = dateRunProgram != 0 ? numberValuesReceived / (dateRunProgram) : 0;
        Debug.Log("Approximate frequency : " + frequency + " Values/seconds");

        if (m_receiveThread != null)
        {
            m_receiveThread.Abort();
        }

        client.Close();
    }

    private float GetRealTime()
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
    }

    private void ReceiveData()
    {
        try
        {
            IPEndPoint receiver = new IPEndPoint(IPAddress.Any, 0); //spÃ©cification du port et de l'adresse IP a Ã©couter
            byte[] receivedBytes = client.Receive(ref receiver); //rÃ©cÃ©ption de la trame UDP
            data1 = Encoding.ASCII.GetString(receivedBytes); //transformer la trame en chaine

            //RÃ©ception des donnÃ©es de capteur brutes
            //Format : 4.391409;e1:0;e2:0;e3:0;e4:0;2:-0.146730474455;e3:-0.146730474455;e4:-0.146730474455;74;
            // temps;capteur1:val1;capteur2:val2;capteur3:val3;
            string[] debuf = data1.Split(new char[] { '|' });

            for (int j = 0; j < debuf.Length; j++)
            {
                string[] sensorStrings = debuf[j].Split(new char[] { ';' }); ;
                bool first = true;
                for (int i = 0; i < sensorStrings.Length - 1; i++)
                {
                    //Information de temps en premiÃ¨re position
                    if (first)
                    {
                        sensorTime = float.Parse(sensorStrings[0], CultureInfo.InvariantCulture.NumberFormat);
                        first = false;
                    }
                    //Noms et valeurs de capteurs ensuite
                    else
                    {
                        string[] sensorValues = sensorStrings[i].Split(new char[] { ':' });
                        sensorName[i - 1] = sensorValues[0];
                        sensorVal[i - 1] = float.Parse(sensorValues[1], CultureInfo.InvariantCulture.NumberFormat);
                        // transFourrier.addSample(sensorVal);
                        //Debug.Log("tests input coucou "+sensorVal[0]);

						EEGDataManager.sendTimeSamples(sensorVal);

                        //Debug.Log(sensorValues[0] + " - ");
                        //Debug.Log(sensorValues[1] + "\n");
                        Debug.Log(sensorTime.ToString() + " - " + sensorName[i - 1] + " : " + sensorVal[i - 1].ToString() + "\n");
                    }
                }
                numberValuesReceived += 1;
            }
        }
        catch (Exception Err)
        {
            Debug.Log(Err.ToString());

            Debug.Log("No date since " + (GetRealTime() - lastTimestampReceived) + " s, Transmiting 0 values ...");
        }
    }

    private void ReceiveDataLoop()
    {
        while (m_threadEnabled == true)
        {
            ReceiveData();
        }
    }

    private bool m_threadEnabled = false;

    // Use this for initialization
    private string data = "";
    private string data1 = "";
    private UdpClient client;
    private int port = 5000;
    private byte[] receivedBytes;
    private static string[] sensorName = new string[14];
    private static double[] sensorVal = new double[14];
    private static float sensorTime = 0.0f;

    private string lastDataReceived = "No Data";

    private float lastTimestampReceived = 0;
    private float timestampStartProgram = 0;
    private Thread m_receiveThread;
}
