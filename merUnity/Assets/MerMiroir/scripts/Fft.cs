using UnityEngine;

public class Fft
{
    public float[][] signalArray = new float[14][];
    public float[][] elecFreqArray = new float[14][];

    static string[] ELECTRODS = new string[14]{"F3","AF3","FC5","F7",
                                           "F4","AF4","FC6","F8",
                                           "T7","P7","O1",
                                           "T8","P8","O2"}; // list of EEG electrods names
    public Fft()
    {


        for (int i = 0; i < signalArray.Length; i++)
        {
            signalArray[i] = new float[128];
            elecFreqArray[i] = new float[64];
            SetToVal(signalArray[i], 0);
        }
    }

    public void addSample(double[] input)
    {
        for (int j = 0; j < 14; j++)
        {
            for (int i = 0; i < signalArray[j].Length - 1; i++)
            {
                signalArray[j][i] = signalArray[j][i + 1];
            }
            signalArray[j][signalArray[j].Length - 1] = (float)input[j];
        }
    }

    public float[][] GetFT()
    {
        for (int j = 0; j < 14; j++)
        {
            // Twiddle factors (64th roots of unity)
            int N = 128;
            float PI2 = 6.2832f;
            int n, k;                     // time and frequency domain indices
            float[] x;           // discrete-time signal, x
            float[] Xre = new float[N / 2 + 1];
            float[] Xim = new float[N / 2 + 1]; // DFT of x (real and imaginary parts)
            float[] P = new float[N / 2 + 1];           // power spectrum of x

            x = signalArray[j];

            int to_sin = 3 * N / 4; // index offset for sin
            int a, b;
            for (k = 0; k <= N / 2; ++k)
            {
                Xre[k] = 0;
                Xim[k] = 0;
                a = 0;
                b = to_sin;
                for (n = 0; n < N; ++n)
                {
                    //Debug.Log("k: "+k +"n :"+n+"a%N :"+a%N);
                    //Debug.Log("k: "+k +"n :"+n+"b%N :"+b%N);
                    Xre[k] += x[n] * (float)Mathf.Cos((float)((a % N / 100) * N / (2 * Mathf.PI)));
                    Xim[k] -= x[n] * (float)Mathf.Cos((float)((b % N / 100) * N / (2 * Mathf.PI)));

                    a += k;
                    b += k;
                }
                P[k] = Xre[k] * Xre[k] + Xim[k] * Xim[k];
            }
            elecFreqArray[j] = P;
        }
        return elecFreqArray;
    }

    public float[][] getSignal()
    {

        return signalArray;
    }

    public void update()
    {

    }

    public void SetToZero(int[] a)
    {
        for (int i = 0; i < a.Length; i++) a[i] = 0;
    }
    public void SetToZero(double[] a)
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
    public double average(double[,] fftArray, int beginElectrodIndex, int endElectrodIndex, int beginFreqIndex, int endFreqIndex) //liste d'electrode et mettre la liste des frequences et faire évoluer la moyenne vers une somme
    {
        double resultat = 0.0;
        int cpt = 0;
        for (int i = beginElectrodIndex; i < endElectrodIndex; i++)
            for (int j = beginFreqIndex; j < endFreqIndex; j++)
            {
                resultat += fftArray[i, j];
                cpt++;
            }
        if (cpt == 0) resultat = 0.0;
        else resultat = resultat / cpt;
        return resultat;
    }

}
