using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AK47AnimEvents : MonoBehaviour
{
    public CharacterController cc ;
    public Transform Barrel ;
    public ParticleSystem FireEffect ;
    public GameObject hitEffect ;
    public LineRenderer BulletTrace ;

    private RaycastHit hit ;
    private ParticleSystem hitEffectPS ;
    

    void HandleShooting()
    {
        if (cc.ClosestTarget)
        {
            cc.AK47Ammo-- ;
            if (cc.AK47Ammo >= 0)
            {
                cc.PlayFireSoundForAK47();
                if (!FireEffect.gameObject.activeSelf)
                {
                    FireEffect.gameObject.SetActive(true);
                }
                else
                {
                    FireEffect.Play();
                }
                if (Physics.Raycast(Barrel.position , transform.forward , out hit ,
                    Mathf.Infinity))
                {
                    if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("PlayerEquipment"))
                    {
                        cc.ClosestTargetCC.PlayShotSoundForAK47();
                        if (cc.ClosestTargetCC.faceExpHandler.CurrentExpression != FaceExpressionHandler.FaceExpressions.SAD)
                        {
                            cc.ClosestTargetCC.faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SAD);
                        }
                        if (cc.ClosestTargetCC.ReturnToSmile)
                        {
                            cc.ClosestTargetCC.ResetReturnToSmileTimer();
                        }
                        else
                        {
                            cc.ClosestTargetCC.ReturnToSmile = true ;
                        }
                        cc.ClosestTargetCC.DecreaseHealth(1);
                        BulletTrace.enabled = true ;
                        BulletTrace.SetPosition(1,transform.InverseTransformPoint(hit.point));
                        if (!hitEffectPS)
                        {
                            hitEffectPS = Instantiate(hitEffect,hit.point,hitEffect.transform.rotation)
                                .GetComponent<ParticleSystem>();
                        }
                        else
                        {
                            hitEffectPS.transform.position = hit.point ;
                            hitEffectPS.Play();
                        }
                    }
                    else if (hit.transform.CompareTag("Obstacle"))
                    {
                        hit.transform.GetComponent<ObstacleHandler>().PlayAk47DamageSound() ;
                        BulletTrace.enabled = true ;
                        BulletTrace.SetPosition(1,transform.InverseTransformPoint(hit.point));
                        if (!hitEffectPS)
                        {
                            hitEffectPS = Instantiate(hitEffect,hit.point,hitEffect.transform.rotation)
                                .GetComponent<ParticleSystem>();
                        }
                        else
                        {
                            hitEffectPS.transform.position = hit.point ;
                            hitEffectPS.Play();
                        }
                    }
                }
            } 
        }
        else
        {
            cc.AK47Ammo = 0 ;
        }
    }

    void TurnOffBulletTrace()
    {
        BulletTrace.enabled = false ;
    }

    void TurnOffFireEffect()
    {
        if (FireEffect.gameObject.activeSelf)
        {
            FireEffect.Stop();
            FireEffect.gameObject.SetActive(false);
        }
    }

    void FinishShooting()
    {
        if (cc.AK47Ammo <= 0)
        {
            Destroy(hitEffectPS.gameObject);
        }
    }
}
