using UnityEngine;
using System.Collections;

public class MathsTool 
{
    public static float CenterValue(float moyenne, float ecartType, float valMin, float valMax, float valCapteur, float tolerance)
    {
        //centre la valeur en fonction de la moyenne et l'écart type
        if (valCapteur <= 0)
            return valMin;
        if (valCapteur <= (moyenne - tolerance * ecartType))
        {
            return valMin;
        }
        else if (valCapteur > (moyenne + tolerance * ecartType))
        {
            return valMax;
        }
        else
        {
            if (ecartType != 0) return (valMin + valMax) / 2f + (valMax - valMin) * (valCapteur - moyenne) / (ecartType * 2f * tolerance);
        }
        return (valMin + valMax) / 2f;
    }
}
