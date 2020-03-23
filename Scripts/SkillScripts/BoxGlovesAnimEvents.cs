using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGlovesAnimEvents : MonoBehaviour
{
    public int CurrentFrame
    {
        get { return currentFrame ; }
    }

    public CharacterController cc ;
    public GameObject fistEffect ;
    
    private Animator animator ;
    private Transform GamePlayCamera ;
    private ParticleSystem fistEffectPS ;
    private int currentFrame ;
    private float fistMaxIntDistance = 30;

    
    private void Start()
    {
        GamePlayCamera = GameObject.FindWithTag("MainCamera").transform ;
        animator = GetComponent<Animator>() ;
        animator.SetBool("Finish",true);
        animator.SetBool("LeftFist",true);
        animator.SetBool("Normal",true);
        animator.SetBool("BreakUltra",false);
    }

    private void OnEnable()
    {
        if (animator)
        {
            animator.SetBool("Finish",true);
            animator.SetBool("LeftFist",true);
            animator.SetBool("Normal",true);
            animator.SetBool("BreakUltra",false);
        }
    }

    void FinishAnimation()
    {
        animator.SetBool("Finish",true) ;
    }

    void SetAnimSpeed(float speed)
    {
        animator.SetFloat("Speed",speed) ;
    }

    void GetCurrentFrame(int frame)
    {
        currentFrame = frame ;
    }

    void BreakUltra()
    {
        animator.SetBool("BreakUltra",true);
        animator.SetBool("Finish",true);
        animator.SetFloat("Speed",1);
        StartCoroutine(SetFalseBreakUltra()) ;
    }
    
    void ChangeHand()
    {
        if (animator.GetBool("LeftFist"))
        {
            animator.SetBool("LeftFist",false);
        }
        else
        {
            animator.SetBool("LeftFist",true);
        }
    }

    IEnumerator SetFalseBreakUltra()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("BreakUltra",false);
    }

    void AddForceToOpponent()
    {
        if (cc.ClosestTarget)
        {
            if (Vector3.SqrMagnitude(cc.ClosestTarget.position - transform.position) < fistMaxIntDistance)
            {
                // Apply the force of fist using currentFrame
                if (cc.GrowSkillPicked)
                {
                    cc.ClosestTarget.GetComponent<Rigidbody>().AddForce((cc.ClosestTarget.position - transform.position)
                                                            .normalized*(Mathf.Pow(currentFrame,2)*2),ForceMode.Impulse);
                }
                else
                {
                    cc.ClosestTarget.GetComponent<Rigidbody>().AddForce((cc.ClosestTarget.position - transform.position)
                                                             .normalized*(Mathf.Pow(currentFrame,2)),ForceMode.Impulse);
                }
            } 
        }
    }

    void DamageForFist(int damageAmount)
    {
        if (cc.ClosestTarget)
        {
            if (Vector3.SqrMagnitude(cc.ClosestTarget.position - transform.position) < fistMaxIntDistance-6)
            {
                cc.ClosestTargetCC.DecreaseHealth(damageAmount);
                cc.PlayFistHit();
                if (fistEffectPS)
                {
                    fistEffectPS.gameObject.SetActive(true);
                    Vector3 charPos = cc.ClosestTarget.transform.Find("CharacterEquipments").position ;
                    fistEffectPS.transform.position = charPos + (GamePlayCamera.position-charPos).normalized*2;
                    fistEffectPS.Play();
                }
                else
                {
                    Vector3 tempPos = cc.ClosestTarget.transform.Find("CharacterEquipments").position ;
                    tempPos = tempPos + (GamePlayCamera.position-tempPos).normalized*2;
                    fistEffectPS = Instantiate(fistEffect , tempPos , fistEffect.transform.rotation).GetComponent<ParticleSystem>() ;
                    fistEffectPS.Play();
                }
                Invoke(nameof(SetFalseFistEffect),1f);
            }
            else
            {
                cc.PlayFistInAir();
            }
        }  
    }

    void DamageForUltraFist(int damageAmount)
    {
        if (cc.ClosestTarget)
        {
            if (Vector3.SqrMagnitude(cc.ClosestTarget.position - transform.position) < fistMaxIntDistance)
            {
                cc.ClosestTargetCC.DecreaseHealth(damageAmount);
                cc.PlayFistHit();
                if (fistEffectPS)
                {
                    fistEffectPS.gameObject.SetActive(true);
                    fistEffectPS.transform.position = cc.ClosestTarget.transform.Find("CharacterEquipments").position ;
                    fistEffectPS.Play();
                }
                else
                {
                    fistEffectPS = Instantiate(fistEffect , cc.ClosestTarget.transform.Find("CharacterEquipments").position
                        , fistEffect.transform.rotation).GetComponent<ParticleSystem>() ;
                }
                Invoke(nameof(SetFalseFistEffect),1f);
            }
            else
            {
                cc.PlayFistInAir();
            }
        }
    }
    
    void SetFalseFistEffect()
    {
        if (fistEffectPS.isStopped)
        {
            fistEffectPS.gameObject.SetActive(false);
        }
    }
}
