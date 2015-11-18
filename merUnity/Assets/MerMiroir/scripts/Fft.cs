using UnityEngine;
using System.Collections;


public class Fft {

	public float []testArray=new float[64];


    const double PI = 3.141592653589793;
    static string  []ELECTRODS=new string [14]{"F3","AF3","FC5","F7",
                                           "F4","AF4","FC6","F8",
                                           "T7","P7","O1",
                                           "T8","P8","O2"} ; // list of EEG electrods names

    static int EEG_FREQ = 128; // fréquense d'acquisition de l'EEG
    int MINQ = 0;        // qualité minimale du signal exigée (1 c'est bien, avec 0 tout passe même si le casque n'est pas sur la tête) 
    double _2pi_inv_eeg_freq = 2 * PI / EEG_FREQ;

    double alphaOSC = 1.0/30;       // fréquence pour la maj des max (1.0/30 -> 60 secondes de vie)
    //initialisation des données
        //trames
    double [,]datas =new double[ELECTRODS.Length,EEG_FREQ]; // tableau des trames eeg 1 seconde
    double []current =new double[ELECTRODS.Length];// trame eeg en cours de traitement
    double []tmp=new double[ELECTRODS.Length];
    int nt = 0;                    // numéro de colonne pour stocker la prochaine mesure eeg
    //fft
    double [,]datasfftRe =  new double[ELECTRODS.Length,64]; // partie réelle de la fft réalisée sur chaque électrode
    double [,]fftIm =       new double[ELECTRODS.Length,64]; // partie imaginaire de la fft réalisée sur chaque électrode
    double [,]fftMod2 =     new double[ELECTRODS.Length,64]; // energie pour chaque électrode (module fft)
    double [] ech_freq =    new double[64];
    // cerebral activities
    double []thetaWaves=new double[4]; // 4 à 8 Hz
    double []alphaWaves=new double[4]; // 8 à 12 Hz
    double []betaWaves=new double[4];  // 12 à 20 Hz
    double []gammaWaves = new double[4]; // 20-28, 28-36 et 36-42 Hz
    int[][] zones = new int[4][] 
    {
        new int[2]{0,4},                    // premier et dernier index des electrodes avant gauche
        new int[2]{4,8},                    // ...................index des electrodes avant droite
        new int[2]{8,11},                   // ..................index des electrodes arrière gauche
        new int[2] { 11, 14 },              // .................index des electrodes arrière droite
    };

	int[] indexes_freq_theta = new int[2];
	bool  indexes_freq_theta_first=true;

	int[] indexes_freq_alpha = new int[2];
	bool  indexes_freq_alpha_first=true;

	int[] indexes_freq_beta  = new int[2];
	bool  indexes_freq_beta_first=true;

	int[][] indexes_freq_gamma = new int[3][]{new int[2],new int[2],new int[2]};
	bool[]  indexes_freq_gamma_first = {true,true,true};

    float inittime;
    float lasttime;
    float now;
    // max des différentes énergies (en log)
    float e_max=1e7F;
    float e_zero = 1e4F;
    float logEMax;
    float minAsZero;
    float[] thetaMax;
    float[] alphaMax;
    float[] betaMax;
    float[] gammaMax;


	public Fft() 
    {
			SetToVal(testArray,0);

            SetToVal(current,0);       //set the current array to 0
            SetToVal(tmp,0);           //set the tmp array to 0
            SetToZero(datas);         //set the datas array to 0
            SetToZero(datasfftRe);    //set the datasfftRe array to 0
            SetToZero(fftIm);         //set the fftIm array to 0
            SetToZero(fftMod2);       //set the fftMod2 array to 0
            SetToVal(ech_freq,0);      //set the ech_freq array to 0

            SetToZero(thetaWaves);  //set the ech_freq array to 0
            SetToZero(alphaWaves);  //set the ech_freq array to 0
            SetToZero(betaWaves);   //set the ech_freq array to 0
            SetToZero(gammaWaves);
			
			
        for (int i = 0; i < ech_freq.Length; i++)    // echantillonnage log en fréquences
            ech_freq[i]=Mathf.Exp(Mathf.Log(4)+(Mathf.Log(42)-Mathf.Log(4))*i/64);

        for (int i = 0; i < ech_freq.Length; i++)
        {
            if (4<=ech_freq[i] && ech_freq[i]<8)  
            {
				if (indexes_freq_theta_first)
				{
					indexes_freq_theta[0]=i;
					indexes_freq_theta_first=false;
				}
				indexes_freq_theta[1]=i;
				/*Debug.Log("tests array size - i "+indexes_freq_theta.Length + "" +i);
                int []temp=indexes_freq_theta;
				System.Array.Resize(ref indexes_freq_theta,indexes_freq_theta.Length+1);
                //indexes_freq_theta=new int[indexes_freq_theta.Length+1];
                indexes_freq_theta=(int[])temp.Clone();
				Debug.Log("test"+indexes_freq_theta.Length +"-"+i);
                indexes_freq_theta[indexes_freq_theta.Length-1]= i;*/
            }
            if (8<=ech_freq[i] && ech_freq[i]<=12)  
            {
				if (indexes_freq_alpha_first)
				{
					indexes_freq_alpha[0]=i;
					indexes_freq_alpha_first=false;
				}
				indexes_freq_alpha[1]=i;
				/*
                int []temp=indexes_freq_alpha;
                indexes_freq_alpha=new int[indexes_freq_alpha.Length+1];
                indexes_freq_alpha=(int[])temp.Clone();
                indexes_freq_alpha[indexes_freq_alpha.Length-1]= i;*/
            }
            if (12<ech_freq[i] && ech_freq[i]<20)   
            {
				if (indexes_freq_beta_first)
				{
					indexes_freq_beta[0]=i;
					indexes_freq_beta_first=false;
				}
				indexes_freq_beta[1]=i;
				/*
                int []temp=indexes_freq_beta;
                indexes_freq_beta=new int[indexes_freq_beta.Length+1];
                indexes_freq_beta=(int[])temp.Clone();
                indexes_freq_beta[indexes_freq_beta.Length-1]= i;       */  
            }

            if (20<=ech_freq[i] && ech_freq[i]<28)  
            {
				if (indexes_freq_gamma_first[0])
				{
					indexes_freq_gamma[0][0]=i;
					indexes_freq_gamma_first[0]=false;
				}
				indexes_freq_gamma[0][1]=i;
                /*int []temp=indexes_freq_gamma[0];
                indexes_freq_gamma[0]=new int[indexes_freq_gamma[0].Length+1];
                indexes_freq_gamma[0]=(int[])temp.Clone();
                indexes_freq_gamma[0][indexes_freq_gamma[0].Length-1]= i; */
            }
            if (28<=ech_freq[i] && ech_freq[i]<36)  
            {
				if (indexes_freq_gamma_first[1])
				{
					indexes_freq_gamma[1][0]=i;
					indexes_freq_gamma_first[1]=false;
				}
				indexes_freq_gamma[1][1]=i;
                /*int []temp=indexes_freq_gamma[1];
                indexes_freq_gamma[1]=new int[indexes_freq_gamma[1].Length+1];
                indexes_freq_gamma[1]=(int[])temp.Clone();
                indexes_freq_gamma[1][indexes_freq_gamma[1].Length-1]= i; */
            }
            if (36<=ech_freq[i] && ech_freq[i]<43)  
            {
				if (indexes_freq_gamma_first[2])
				{
					indexes_freq_gamma[2][0]=i;
					indexes_freq_gamma_first[2]=false;
				}
				indexes_freq_gamma[2][1]=i;
                /*int []temp=indexes_freq_gamma[2];
                indexes_freq_gamma[2]=new int[indexes_freq_gamma[2].Length+1];
                indexes_freq_gamma[2]=(int[])temp.Clone();
                indexes_freq_gamma[2][indexes_freq_gamma[2].Length-1]= i;*/ 
            }
        }

            
            /*indexes_freq_theta=new int[2]{indexes_freq_theta[0],indexes_freq_theta[indexes_freq_theta.Length-1]};
            indexes_freq_alpha=new int[2]{indexes_freq_alpha[0],indexes_freq_alpha[indexes_freq_alpha.Length-1]};
            indexes_freq_beta =new int[2]{indexes_freq_beta[0],indexes_freq_beta[indexes_freq_beta.Length-1]};


            indexes_freq_gamma[0]=new int[2]{indexes_freq_gamma[0][0],indexes_freq_gamma[0][indexes_freq_gamma[0].Length-1]};
            indexes_freq_gamma[1]=new int[2]{indexes_freq_gamma[1][0],indexes_freq_gamma[1][indexes_freq_gamma[1].Length-1]};
            indexes_freq_gamma[2] = new int[2] { indexes_freq_gamma[2][0], indexes_freq_gamma[2][indexes_freq_gamma[2].Length - 1] };*/
            inittime=Time.time;
            lasttime=inittime;
            now=lasttime;

            minAsZero= Mathf.Log(e_zero);
            logEMax=Mathf.Log(e_max);
            thetaMax = new float[4];
            SetToVal(thetaMax, logEMax);
            alphaMax = new float[4];
            SetToVal(alphaMax, logEMax);
            betaMax  = new float[4];
            SetToVal(betaMax, logEMax);
            gammaMax = new float[3];
            SetToVal(gammaMax, logEMax);
		//for (int i=0; i<64; i++)
		//	addSample (current);
    }

    public void addSample(double [] input) 
    {	
		for (int i=0; i<testArray.Length-1; i++) {
			testArray[i]=testArray[i+1];
		}
		testArray [63] = (float)input [0];
		/*
        //fft
        input.CopyTo(current,0);
		for (int electrodIndex=0;electrodIndex<current.Length;electrodIndex++)
            for (int i = 0; i < 64; i++)
            {
			tmp[electrodIndex]=current[electrodIndex]-datas[electrodIndex,nt];// nt est un index circulaire comptant les trames (supposé = temps)
                datasfftRe[electrodIndex,i] += (tmp[electrodIndex]*Mathf.Cos( (float)((ech_freq[i]*nt)*_2pi_inv_eeg_freq ))); //mettre (now-initTime)*EEG_FREQ à la place de nt

                fftIm[electrodIndex,i] += (tmp[electrodIndex]*Mathf.Sin( (float)((ech_freq[i]*nt)*_2pi_inv_eeg_freq )));            // maj de datas et de nt (tableau des trames eeg de la dernière seconde)
			datas[electrodIndex,nt] = current[electrodIndex];
            }
        nt+=1;
        if (nt==EEG_FREQ)  // doit arriver 1 fois par seconde
                nt=0 ;// nt est un index circulaire comptant les trames (supposé = temps)

		for (int electrodIndex=0;electrodIndex<current.Length;electrodIndex++)
			//listener.GetSpectrumData(,0, FFTWindow.Rectangular);
            for (int i = 0; i < 64; i++)
            {
			//Debug.Log("tests current coucou "+current[0]);
            fftMod2[electrodIndex,i]=Mathf.Pow((float)datasfftRe[electrodIndex,i],2)+Mathf.Pow((float)fftIm[electrodIndex,i],2); // compute cerebral energy for every position at every freq.
            }
        for (int i=0;i<4;i++){
            thetaWaves[i]=average(fftMod2,zones[i][0],zones[i][1],indexes_freq_theta[0],indexes_freq_theta[1]);
  
            alphaWaves[i]=average(fftMod2,zones[i][0],zones[i][1],indexes_freq_alpha[0],indexes_freq_alpha[1]);
            betaWaves [i]=average(fftMod2,zones[i][0],zones[i][1],indexes_freq_beta[0] ,indexes_freq_beta[1]);
            if (i<3)  gammaWaves[i]=average(fftMod2,0,14,indexes_freq_gamma[i][0],indexes_freq_gamma[i][1]);
        }*/
    }

    public double[,] getFourrierTransformMod2()
    {
        return fftMod2;
    }
	public float[] GetFT(){
		int N = 64;
		float PI2= 6.2832f;
		// time and frequency domain data arrays
		int n, k;             // indices for time and frequency domains
		float [] x;           // discrete-time signal, x
		float []Xre=new float[N];
		float []Xim=new float[N]; // DFT of x (real and imaginary parts)
		float []P=new float[N];           // power spectrum of x

		x=testArray;

		
		// Calculate DFT of x using brute force
		for (k=0 ; k<N ; ++k)
		{
			// Real part of X[k]
			Xre[k] = 0;
			for (n=0 ; n<N ; ++n) Xre[k] += x[n] * Mathf.Cos(n * k * PI2 / N);
			
			// Imaginary part of X[k]
			Xim[k] = 0;
			for (n=0 ; n<N ; ++n) Xim[k] -= x[n] * Mathf.Sin(n * k * PI2 / N);
			
			// Power at kth frequency bin
			P[k] = Xre[k]*Xre[k] + Xim[k]*Xim[k];
		}
		return P;
	}
	
	public double[] getThetaWaves() 
	{
		return thetaWaves;
    }
    public double[] getAlphaWaves()
    {
        return thetaWaves;
    }
    public double[] getBetaWaves()
    {
        return betaWaves;
    }
    public double[] getGammaWaves()
    {
        return gammaWaves;
    }

    public void update() 
    {

    }

    public void SetToZero(int [] a)
    {
        for (int i = 0; i < a.Length; i++) a[i] = 0;
    }
    public void SetToZero(double [] a)
    {
        for (int i = 0; i < a.Length; i++) a[i] = 0;
    }
    public void SetToVal(double[] a, double value) 
    {
        for (int i = 0; i < a.Length; i++) a[i] = value;
    }
    public void SetToVal(float[] a, float value)
    {
        for (int i = 0; i < a.Length; i++) a[i] = value;
    }
    public void SetToZero(double[,] a)
    {
        for (int i = 0; i < a.GetLength(0); i++)
            for (int j = 0; j < a.GetLength(1); j++)
                a[i, j] = 0;
    }
    public double average (double [,] fftArray,int beginElectrodIndex, int endElectrodIndex,int beginFreqIndex,int endFreqIndex) //liste d'electrode et mettre la liste des frequences et faire évoluer la moyenne vers une somme
    {
        double resultat=0.0;
        int cpt=0;
        for(int i=beginElectrodIndex;i<endElectrodIndex;i++)
            for(int j=beginFreqIndex;j<endFreqIndex;j++)
            {
                resultat+=fftArray[i,j];
                cpt++;
            }
        if (cpt==0) resultat=0.0;
        else resultat=resultat/cpt;
        return resultat;
    }
	public void copyArray(int[]source, int[]dest)
	{} 

}
