using UnityEngine;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


public static class Client{
	 
	// Use this for initialization
	static string data="";
	static UdpClient client;
	static int port=5000;
	static byte[] receivedBytes;
	static Thread receiveThread;
	static float[] basseFrequence = new float[4];
	static float[] hauteFrequence = new float[4];
	static float[] thetaFrequence = new float[4];
	static float[] tabTHF = new float[3];
	static float[] moyenneBasse = new float [4];
	static float[] moyenneHaute = new float [4];
	static float[] moyenneTheta = new float [4];
	static float[] moyenneTHF = new float [3];
	static float[] ecartTypeBasse = new float [4];
	static float[] ecartTypeHaute = new float [4];
	static float[] ecartTypeTheta = new float [4];
	static float[] ecartTypeTHF = new float [3];
	//static float[] tabVal = new float[12];
	static float[] tabVal1 = new float[12];
	static float[] THF = new float [3];
	static float[] THF1 = new float [3];

	static float alphaRadius;
	static float alphaTHF;

	static string lastDataReceived = "No Data";
	static float val0 = 0;
	static float val1 = 0;
	static float val2 = 0;

	static long lastTimestampReceived = 0;
	public static int numberValuesReceived = 0;
    public static long timestampStartProgram = 0;

	
	public static float moyenneGaucheDroite = 0;
	public static float valGaucheDroite=0;
	public static float minMoyGD = 0;
	public static float maxMoyGD = 0;
	public static float minGD = 0;
	public static float maxGD = 0;

	public static void init(float alphaRadiusValue, float alphaTHFValue)
	{
		alphaRadius = alphaRadiusValue;
		alphaTHF = Mathf.Max (0.001f,alphaTHFValue);



		timestampStartProgram = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		lastTimestampReceived = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		Debug.Log("Starting program at " + timestampStartProgram);

		client = new UdpClient(port);
		client.Client.ReceiveTimeout = 5000;

		Application.runInBackground = true;
		receiveThread=new Thread(new ThreadStart(ReceiveData)); //Créer un thread pour écouter le server UDP
		receiveThread.IsBackground = true;
		receiveThread.Start();
		for (int i=0; i<4; i++) {
			basseFrequence[i]=-40f;
			moyenneBasse[i]=0f;
			ecartTypeBasse[i]=1f;
			hauteFrequence[i]=-40f;
			moyenneHaute[i]=0f;
			ecartTypeHaute[i]=1f;
			thetaFrequence[i]=-40f;
			moyenneTheta[i]=0f;
			ecartTypeTheta[i]=1;
			if (i<3)
			{
				tabTHF[i]=-40f;
				moyenneTHF[i]=0f;
				ecartTypeTHF[i]=1f;
				THF[i]=0.1f;
				THF1[i]=0.1f;
			}
		}
		for (int i=0; i<12; i++) { // valeur à multiplier par rmax pour le rayon des vagues
			//tabVal[i]=0f;
			tabVal1[i]=0f;
		}
	}
	private static void ReceiveData()
	{
		while (true)
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
					switch (split[0])
					{
						case "Basse":				//Alpha
						{
							for(int i=0;i<4;i++) //0 : avant gauche, 1 : avant droite, 2 : arriere gauche, 3 : arriere droite 
							{
								if(split[i+1]!=null){
									basseFrequence[i]=float.Parse(split[i+1],CultureInfo.InvariantCulture.NumberFormat);
									if (basseFrequence[i]>0) {
										basseFrequence[i] = Mathf.Log (basseFrequence[i]);
									}
									else {
										basseFrequence[i]=-50f;
									}
								}
							}
							break;
						}
						case "Haute":				//Beta		-- mouvement oscillant caméra
						{
							for(int i=0;i<4;i++)
							{
								if(split[i+1]!=null){
									hauteFrequence[i]=float.Parse(split[i+1],CultureInfo.InvariantCulture.NumberFormat);
									if (hauteFrequence[i]>0) {
										hauteFrequence[i] = Mathf.Log (hauteFrequence[i]);
									}
									else {
										hauteFrequence[i]=-50f;
									}
								}
							}
							break;
						}
						case "Theta":
						{
							for(int i=0;i<4;i++)
							{
								if(split[i+1]!=null){
									thetaFrequence[i]=float.Parse(split[i+1],CultureInfo.InvariantCulture.NumberFormat);
									if (thetaFrequence[i]>0) {
										thetaFrequence[i] = Mathf.Log (thetaFrequence[i]);
									}
									else {
										thetaFrequence[i]=-50f;
									}
								}
							}
							break;
						}
						case "THF":
						{
							for(int i=0;i<3;i++)
							{
								if(split[i+1]!=null){
									tabTHF[i]=float.Parse(split[i+1],CultureInfo.InvariantCulture.NumberFormat);
									if (tabTHF[i]>0) {
										tabTHF[i] = Mathf.Log (tabTHF[i]);
									}
									else {
										tabTHF[i]=-50f;
									}
								}
							}
							break;
						}
						default:
						{
							break;
						}
					}
					lastDataReceived = data;
					numberValuesReceived +=1;
					lastTimestampReceived = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
				}
			}

			catch(Exception Err)
			{
				Debug.Log (Err.ToString());
				if ( ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastTimestampReceived ) >  1000 ) resetValues();
			}


		}
	}

	public static void calculTHF(float maxTHF)
	{
		//Calcul les différentes valeurs que prendront les coefs correspondant aux très hautes fréquences (coef modifiant la texture pour donner l'impression de vagues de surface)
		THF1 [0] = Mathf.Exp (valeur_centre (moyenneTHF[0], ecartTypeTHF[0], Mathf.Log (0.1f), Mathf.Log (maxTHF), tabTHF [0], 1f));
		if (THF1 [0] < 0.1f)
						THF1 [0] = 0.1f;
		THF1 [1] = Mathf.Exp (valeur_centre (moyenneTHF[1], ecartTypeTHF[1], Mathf.Log (0.1f), Mathf.Log (maxTHF), tabTHF [1], 1f));
		if (THF1 [1] < 0.1f)
						THF1 [1] = 0.1f;
		THF1 [2] = Mathf.Exp (valeur_centre (moyenneTHF[2], ecartTypeTHF[2], Mathf.Log (0.1f), Mathf.Log (maxTHF), tabTHF [2], 1f));
		if (THF1 [2] < 0.1f)
						THF1 [2] = 0.1f;
		for (int i=0; i<3; i++)
		{
			float alpha=Mathf.Min(1,Time.deltaTime/alphaTHF);
			THF[i] = (1f - alpha) * THF[i] + (alpha * THF1[i]); //valeur a t+1 tenant compte de la valeur précédente
		}

	}

	public static float getTHF(string coef)
	{
		if (coef == "THF1")
						return THF [0];
		if (coef == "THF2")
						return THF [1];
		if (coef == "THF3")
						return THF [2];
		return 1f; //valeur par défaut
	
	}
	public static void calculRadius()
	{
		for(int i=0;i<4;i++)
		{
			//Calcul les différentes valeurs des radius des vagues (correspont à un rayon)
			tabVal1[i]=valeur_centre(moyenneBasse[i],ecartTypeBasse[i],0f,1f,basseFrequence[i],2f);
			tabVal1[i+4]=valeur_centre(moyenneHaute[i],ecartTypeHaute[i],0f,1f,hauteFrequence[i],2f);
			tabVal1[i+8]=valeur_centre(moyenneTheta[i],ecartTypeTheta[i],0f,1f,thetaFrequence[i],2f);
			//tabVal[i]=((1-alphaRadius)*tabVal[i])+(alphaRadius*tabVal1[i]);
			//tabVal[i+4]=((1-alphaRadius)*tabVal[i+4])+(alphaRadius*tabVal1[i+4]);
			//tabVal[i+8]=((1-alphaRadius)*tabVal[i+8])+(alphaRadius*tabVal1[i+8]);		
		}
	}
	private static float valeur_centre(float moyenne,float ecartType,float valMin,float valMax, float valCapteur,float tolerance)
	{
		//centre la valeur en fonction de la moyenne et l'écart type
		if (valCapteur <= 0)
						return valMin;
		if(valCapteur<=(moyenne-tolerance*ecartType)) 
		{
			return valMin;
		}
		else if(valCapteur>(moyenne+tolerance*ecartType))
		{
			return valMax;
		}
		else
		{
			if (ecartType!=0) return (valMin+valMax)/2f + (valMax - valMin) * (valCapteur-moyenne)/(ecartType*2f*tolerance);
		}
		return (valMin+valMax)/2f;
	}

	private static float funMoyenne(float[] tab) //moyenne réel en fonction d'un tableau de valeurs
	{
		if (tab.Length < 1)
						return 0f;
		float moyenne = 0f;
		for (int i = 0; i < tab.Length; i++){
			moyenne += tab[i];
		}
		return (moyenne / tab.Length);
	}
	private static float estimationMoyenne(float alpha,float moyenne, float lastValue) //estimation d'une moyenne à l'instant t en tenant compte de la moyenne précédente
	{
		if (lastValue <= 0) { // mesure non valable
			val0 = 0f;
			return 0f;
		}
		if (val0 == 0f) { // première valeur acceptable pour lastValue
			val0=lastValue;
			return lastValue;
		}
		return ((1f - alpha) * moyenne + alpha * lastValue);
	}
	
	private static float funEcartType(float[] tab) //écart type réel en fonction d'un tableau de valeurs
	{
		if (tab.Length < 2)
						return 0f;
		float moyenne = funMoyenne (tab);
		float ecartType = 0;
		for (int i = 0; i < tab.Length; i++)
		{
			ecartType = ecartType + ((tab[i] - moyenne) * (tab[i] - moyenne));
		}
		ecartType=ecartType/(tab.Length-1);
		return Mathf.Sqrt(ecartType);
	}

	private static float estimationEcartType(float alpha,float ecartType,float moyenne, float lastValue) //estimation de l'écart type à l'instant t en tenant compte de l'écart type précédent
	{	
		if (lastValue <= 0) 
		{
			val1=0;
			val2=0;
			return ecartType; // on conserve la dernière valeur calculée
		}
		if (val1 == 0) { // lastvalue>0 pour la première fois
			val1 = lastValue;
			return ecartType; // on conserve la dernière valeur calculée
		}
		if (val2 == 0) { // lastvalue>0 pour la seconde fois
			val2=lastValue;
			return Mathf.Abs (val2-val1); // première estimation possible
		}
		return ((1f - alpha) * ecartType + alpha * Mathf.Abs (lastValue - moyenne));
	}

	public static void MAJmoyenneEcartType(float dt,float TMbasse,float TMhaute,float TMtheta,float TMthf,float TETbasse,float TEThaute,float TETtheta,float TETthf)
	{
		//mise à jour des moyennes et des écarts type
		//dt(s) : intervalle de temps entre deux frames 
		//T(s) : L'estimation de la moyenne/ecart type tient compte de toutes les valeurs recu durant la période T 
		for (int i=0; i<4; i++)
		{
			moyenneBasse[i]= estimationMoyenne (dt/TMbasse,moyenneBasse[i],basseFrequence[i]);
			ecartTypeBasse[i]= estimationEcartType (dt/TETbasse,ecartTypeBasse[i],moyenneBasse[i],basseFrequence[i]);
			moyenneHaute[i]= estimationMoyenne (dt/TMhaute,moyenneHaute[i],hauteFrequence[i]);
			ecartTypeHaute[i]=estimationEcartType(dt/TEThaute,ecartTypeHaute[i],moyenneHaute[i],hauteFrequence[i]);
			moyenneTheta[i]= estimationMoyenne (dt/TMtheta,moyenneTheta[i],thetaFrequence[i]);
			ecartTypeTheta[i]=estimationEcartType(dt/TETtheta,ecartTypeTheta[i],moyenneTheta[i],thetaFrequence[i]);
			if (i<3)
			{
				moyenneTHF[i]= estimationMoyenne (dt/TMthf,moyenneTHF[i],tabTHF[i]);
				ecartTypeTHF[i]=estimationEcartType(dt/TETthf,ecartTypeTHF[i],moyenneTHF[i],tabTHF[i]);
			}
		}
	}

	public static float getAmount()
	{
		return 1f;
	}

	public static float getRadius(string frequence,string position)
	{
		switch (frequence)
		{
			case "basse":
				if (position == "avantG")
			{
				return tabVal1[0];
				//return tabVal[0];
				}
				else if(position == "avantD")
			{
				return tabVal1[1];
				//return tabVal[1];
				}
				else if (position == "arriereG")
			{
				return tabVal1[2];
				//return tabVal[2];
				}
				else if(position=="arriereD")
			{
				return tabVal1[3];
				//return tabVal[3];
				}
				else
				{
					return 0;
				}
			case "haute":
				if (position=="avantG")
			{
				return tabVal1[4];
				//return tabVal[4];
				}
				else if(position == "avantD")
			{
				return tabVal1[5];
				//return tabVal[5];
				}
				else if (position == "arriereG")
			{
				return tabVal1[6];
				//return tabVal[6];
				}
				else if(position=="arriereD")
			{
				return tabVal1[7];
				//return tabVal[7];
				}
				else
				{
					return 0;
				}
			case "theta":
				if (position=="avantG")
			{
				return tabVal1[8];
				//return tabVal[8];
				}
				else if(position == "avantD")
			{
				return tabVal1[9];
				//return tabVal[9];
				}
				else if (position == "arriereG")
			{
				return tabVal1[10];
				//return tabVal[10];
				}
				else if(position=="arriereD")
			{
				return tabVal1[11];
				//return tabVal[11];
				}
				else
				{
					return 0;
				}
			default:
				return 0;
		}
	}
	public static void moyenneGD() //différence gauche droite
	{
		float moyenneGauche;
		float moyenneDroite;
		float valGauche;
		float valDroite;

		moyenneGauche = moyenneBasse [0] + moyenneBasse [2] + moyenneHaute [0] + moyenneHaute [2] + moyenneTheta [0] + moyenneTheta [2];
		moyenneDroite = moyenneBasse [1] + moyenneBasse [3] + moyenneHaute [1] + moyenneHaute [3] + moyenneTheta [1] + moyenneTheta [3];
		valGauche = basseFrequence [0] + basseFrequence [2] + hauteFrequence [0] + hauteFrequence [2] + thetaFrequence [0] + thetaFrequence [2];
		valDroite = basseFrequence [1] + basseFrequence [3] + hauteFrequence [1] + hauteFrequence [3] + thetaFrequence [1] + thetaFrequence [3];
		if (moyenneGauche <= 0 || moyenneDroite <= 0 || valGauche <= 0 || valDroite <= 0) {
			moyenneGaucheDroite = valGaucheDroite = minMoyGD = minGD = 0f;
			maxMoyGD = maxGD = 0f;
			return;
		}

		moyenneGaucheDroite = moyenneDroite-moyenneGauche;
		minMoyGD = 0.99f * minMoyGD + 0.01f * moyenneGaucheDroite;
		maxMoyGD = 0.99f * maxMoyGD + 0.01f * moyenneGaucheDroite;
		if (moyenneGaucheDroite < minMoyGD)
			minMoyGD = moyenneGaucheDroite;
		if (moyenneGaucheDroite > maxMoyGD)
			maxMoyGD = moyenneGaucheDroite;

		valGaucheDroite = valDroite - valGauche;
		minGD = 0.99f * minGD + 0.01f * valGaucheDroite;
		maxGD = 0.99f * maxGD + 0.01f * valGaucheDroite;
		if (valGaucheDroite < minGD)
			minGD = valGaucheDroite;
		if (valGaucheDroite > maxGD)
			maxGD = valGaucheDroite;
	}

	public static float[] getBetaValues ()
	{
		return moyenneHaute;
	}

	public static void Disable() //ferme le thread
	{
		double dateRunProgram = ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timestampStartProgram) / 1000;
		Debug.Log("Client have ran for " + dateRunProgram + "seconds and have received " + numberValuesReceived + "dataValues within this time!");
		double frequency = numberValuesReceived / (dateRunProgram);
		Debug.Log("Approximate frequency : " + frequency + " Values/seconds" );
		
		if (receiveThread != null)
		{
			receiveThread.Abort();
		}
		client.Close();
	}

	/*
	 * 	Alpha radius setters
	 */
	public static void setAlphaRad (float value)
	{
		alphaRadius = value;
		Debug.Log(" - AlphaRadius :" + alphaRadius);
	}

	public static void setAlphaTHF (float value)
	{
		alphaTHF = Mathf.Max (0.0001f, value);
		Debug.Log(" - Alpha THF :" + alphaTHF);
	}

	/*
	 * Display debug values
	 * */
	public static void displayValues ()
	{
		String toDisplay = lastDataReceived;
		for (int i=0; i<4; i++)
		{
			toDisplay += basseFrequence[i];
			toDisplay += "-";
			toDisplay += hauteFrequence[i];
			toDisplay += "-";
			toDisplay += thetaFrequence[i];
			toDisplay += "/";
		}
		Debug.Log(toDisplay);
	}


	/**
	 *	Reset Values if we don't receive anything
	 * 
	 * */
	public static void resetValues ()
	{
		Debug.Log ("No date since " + ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastTimestampReceived ) + " ms, Transmiting 0 values ...");
		for (int i=0; i<4; i++) {
			basseFrequence[i]=-40f;
			hauteFrequence[i]=-40f;
			thetaFrequence[i]=-40f;
			if (i<3)
			{
				tabTHF[i]=-40f;
			}
			
		}
	}
}
