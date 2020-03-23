using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherAnimEvents : MonoBehaviour
{
    public Animator HandsAnimator ;
    public CharacterController cc ;
    public GameObject Rocket ;
    public ParticleSystem RocketShootEffect ;
    public Transform RocketBarrel ;

    private bool outState ;
    
    
    private void OnEnable()
    {
        outState = false ;
    }

    void SetTriggerWeaponOutForHands()
    {
        HandsAnimator.SetTrigger("WeaponOut");
        outState = true ;
    }

    void Fire()
    {
        if (cc.ClosestTarget)
        {
            cc.PlayRocketShooting();
            Rocket r = Instantiate(Rocket , RocketBarrel.transform.position , RocketBarrel.transform.rotation)
                .GetComponent<Rocket>() ;
            r.ShooterColor = cc.PaintColor ;
            r.Target = cc.ClosestTarget ;
            RocketShootEffect.gameObject.SetActive(true);
            RocketShootEffect.Play();
        }
        else
        {
            cc.RocketPicked = false ;
            HandsAnimator.SetTrigger("WeaponOut");
            outState = true ;
        }
    }

    void FinishShooting()
    {
        if (outState)
        {
            cc.RocketPicked = false ;  
            RocketShootEffect.gameObject.SetActive(false);
        }   
    }
}
