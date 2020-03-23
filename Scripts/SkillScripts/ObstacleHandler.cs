using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    public AudioClip Ak47Damage1 ;
    public AudioClip Ak47Damage2 ;
    public AudioClip Ak47Damage3 ;

    public AudioSource FreezerCrackAs ;
    public AudioSource RocketBlowAs ;
    public AudioSource Ak47ShotAs ;

    private byte ak47ShotIndex ;
    
    public void PlayAk47DamageSound()
    {
        switch (ak47ShotIndex)
        {
            case 0:
                Ak47ShotAs.PlayOneShot(Ak47Damage1);
                break;
            case 1:
                Ak47ShotAs.PlayOneShot(Ak47Damage2);
                break;
            case 2:
                Ak47ShotAs.PlayOneShot(Ak47Damage3);
                break;
        }

        ak47ShotIndex++ ;
        if (ak47ShotIndex > 2)
        {
            ak47ShotIndex = 0 ;
        }
    }

    public void PlayFreezerCrackSound()
    {
        FreezerCrackAs.Play();
    }

    public void PlayRocketBlowSound()
    {
        RocketBlowAs.Play();
    }
}
