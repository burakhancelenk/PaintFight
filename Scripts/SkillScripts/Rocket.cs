using System.Collections;
using System.Collections.Generic;
using Es.InkPainter.Sample ;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public Color ShooterColor ;
    public GameObject RocketBlowEffect ;
    public GameObject CharacterBurnEffect ;
    
    [HideInInspector]
    public Transform Target ;
    
    private float speed = 70 ;
    private GameManager gm ;

    private void Start()
    {
        Invoke(nameof(ActivateCollision),0.3f);
        if (Target)
        {
            Target.GetComponent<FaceExpressionHandler>().ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SUPRİSED);
            Target.GetComponent<CharacterController>().ReturnToSmile = false ;
        }

        gm = GameObject.Find("GameManager").GetComponent<GameManager>() ;
    }

    private void Update()
    {
        if (Target)
        {
            transform.rotation = Quaternion.LookRotation(Target.position-transform.position);
            transform.Translate(Vector3.forward*Time.deltaTime*speed,Space.Self); 
        }
        else
        {
            Instantiate(RocketBlowEffect , transform.position , RocketBlowEffect.transform.rotation) ;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<CharacterController>().PlayRocketExplode();
            other.GetComponent<FaceExpressionHandler>().ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SAD);
            other.GetComponent<CharacterController>().ReturnToSmile = true ;
            other.GetComponent<CharacterController>().ResetReturnToSmileTimer();
            if (Target != other.transform)
            {
                Target.GetComponent<FaceExpressionHandler>().ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                Target.GetComponent<CharacterController>().ReturnToSmile = false ;
            }
            Target.GetComponent<CharacterController>().CharacterUI.OpenCloseCrosshair(false);
            other.GetComponent<CollisionPainter>().brush.Color = ShooterColor ;
            other.GetComponent<CharacterController>().ActivateRocketSkill();
            Instantiate(RocketBlowEffect , other.transform.position+Vector3.up*4.35f , RocketBlowEffect.transform.rotation) ;
            Instantiate(CharacterBurnEffect , other.transform.position , CharacterBurnEffect.transform.rotation)
                .transform.SetParent(other.transform);
            if (gm.Players[gm.MyPlayerId] == other.transform)
            {
                gm.CharCameraController.ShakeCameraTrigger = true ;
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            other.GetComponent<ObstacleHandler>().PlayRocketBlowSound();
            Target.GetComponent<FaceExpressionHandler>().ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
            Target.GetComponent<CharacterController>().ReturnToSmile = false ;
            Target.GetComponent<CharacterController>().CharacterUI.OpenCloseCrosshair(false);
            Instantiate(RocketBlowEffect , other.transform.position , RocketBlowEffect.transform.rotation) ;
            Destroy(gameObject);
        }
    }

    private void ActivateCollision()
    {
        GetComponent<CapsuleCollider>().enabled = true ;
    }
}
