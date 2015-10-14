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

    static string[] sensorName = new string[14];
    static float[] sensorVal = new float[14];
    static float sensorTime = 0.0f;

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
                data1 = Encoding.ASCII.GetString(receivedBytes); //transformer la trame en chaine

                //Réception des données de capteur brutes
        				//Format : 4.391409;e1:0;e2:0;e3:0;e4:0;2:-0.146730474455;e3:-0.146730474455;e4:-0.146730474455;74;
        				// temps;capteur1:val1;capteur2:val2;capteur3:val3;
        				string[] debuf = data1.Split(new char[] { '|' });

                for(int j=0 ; j<debuf.Length ; j++)
                {
                  string[] sensorStrings = debuf[j].Split(new char[] { ';' });;
                  bool first = true;
          				for(int i=0 ; i<sensorStrings.Length-1 ; i++)
          				{
          					//Information de temps en première position
          					if(first)
          					{
                      sensorTime = float.Parse(sensorStrings[0], CultureInfo.InvariantCulture.NumberFormat);
          						first = false;
                    }
          					//Noms et valeurs de capteurs ensuite
          					else
          					{
          						string[] sensorValues = sensorStrings[i].Split(new char[] { ':' });
          						sensorName[i-1] = sensorValues[0];
          						sensorVal[i-1] = float.Parse(sensorValues[1], CultureInfo.InvariantCulture.NumberFormat);
                      //Debug.Log(sensorValues[0] + " - ");
                      //Debug.Log(sensorValues[1] + "\n");
          						Debug.Log(sensorTime.ToString() + " - " + sensorName[i-1] + " : " + sensorVal[i-1].ToString() + "\n");
          					}
          				}
                  numberValuesReceived += 1;
                }



                /*if (data.Length > 5)
                {
                    //Debug.Log (data.ToString());
                    string[] split = data.Split(new char[] { ';' });

                    EEGDataManager.Instance.Update(split);


                }*/
                //lastDataReceived = data;

                //lastTimestampReceived = Time.realtimeSinceStartup;
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
    private string data1 = "";
    private UdpClient client;
    private int port = 5000;
    private byte[] receivedBytes;

    private string lastDataReceived = "No Data";

    private float lastTimestampReceived = 0;
    private float timestampStartProgram = 0;
}
