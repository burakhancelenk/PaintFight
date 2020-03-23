using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates ;
using UnityEngine;
using UnityEngine.UI;

public class CharUI : MonoBehaviour
{
    public Gradient HealthBarGradient ;
    public GameObject CrosshairObject ;
    public Image CrosshairImage ;
    public Image HealthBar ;

    private float currentGradientValue ;
    
    void Start()
    {
        currentGradientValue = 0 ;
        HealthBar.color = HealthBarGradient.Evaluate(currentGradientValue) ;
    }

    public void OpenCloseCrosshair(bool open)
    {
        CrosshairObject.SetActive(open);
    }

    public void ChangeColorOfCrosshair(bool red)
    {
        if (red)
        {
            CrosshairImage.color = Color.red ;
        }
        else
        {
            CrosshairImage.color = Color.yellow ;
        }
    }

    public void TakeDamage(int amount)
    {
        currentGradientValue += (float)amount / 100 ;
        if (currentGradientValue > 1)
        {
            currentGradientValue = 1 ;
        }
        else if (currentGradientValue < 0)
        {
            currentGradientValue = 0 ;
        }
        HealthBar.color = HealthBarGradient.Evaluate(currentGradientValue) ;
    }
}
