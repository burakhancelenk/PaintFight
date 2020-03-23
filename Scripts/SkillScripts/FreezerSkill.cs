using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI ;

public class FreezerSkill : MonoBehaviour
{
    public bool SinglePlayer
    {
        set { singlePlayer = value ; }
    }

    public GameManager GM
    {
        set
        {
            if (gm == null)
            {
                gm = value ;
            }
        }
    }
    
    public GameObject CharacterFreezeEffect ;
    public GameObject FreezerDestroyEffect ;
    public Transform FreezerCloud ;
    public Transform FreezerAura ;
    
    private Transform[] players ;
    private Transform closestTarget ;
    private Vector3 freezerCloudDistance ;
    private Vector3 freezerAuraDistance ;
    private float speed = 10 ;
    private float focusSpeed = 7 ;
    private float posY ;
    private float angleX ;
    private Quaternion targetRot ;
    private bool singlePlayer ;
    private byte heavyFuncWait = 1 ;
    private byte heavyFuncWaitCount ;
    private GameManager gm ;

    void Start()
    {
        posY = transform.position.y ;
        angleX = 0 ;
        GameManager gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>() ; 
        players = new Transform[gm.Players.Length];
        players = gm.Players ;
        freezerCloudDistance = transform.position - FreezerCloud.position ;
        freezerAuraDistance = transform.position - FreezerAura.position ;
    }

    void Update()
    {
        ++heavyFuncWaitCount ;
        if (heavyFuncWaitCount >= heavyFuncWait)
        {
            FindClosestTarget();
            heavyFuncWaitCount = 0 ;
        }
        if (closestTarget)
        {
            angleX += Time.deltaTime *1.25f* 720/(Mathf.Sqrt(Mathf.PI)) ;
            angleX %= 360 ;
            targetRot = Quaternion.LookRotation(closestTarget.position - transform.position) ;
            transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(Vector3.up*targetRot.eulerAngles.y
                                                                      +Vector3.right * angleX),focusSpeed*Time.deltaTime);
            transform.Translate((closestTarget.position-transform.position).normalized * Time.deltaTime * speed,Space.World);
            transform.position = Vector3.up*posY + Vector3.forward*transform.position.z + Vector3.right*transform.position.x;
            FreezerAura.position = transform.position - freezerAuraDistance ;
            FreezerCloud.position = transform.position - freezerCloudDistance ;
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>() ;
            Transform effectPos = other.transform.Find("CharacterEquipments") ;
            cc.FreezerSkillPicked = true ;
            cc.PlaySkillSoundFX(6);
            cc.Freeze();
            cc.faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SUPRİSED);
            cc.ReturnToSmile = false ;
            if (cc.BotPlayer)
            {
                NavMeshAgent na = cc.GetComponent<NavMeshAgent>() ;
                na.angularSpeed = 0 ;
                na.velocity = Vector3.zero;
                na.speed = 0 ;
            }

            if (singlePlayer)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    AIHandler handler ;
                    if (gm.MyPlayerId != i)
                    {
                        handler = players[i].GetComponent<AIHandler>() ;
                        handler.FreezerSkillSpawned = false ;
                        handler.DestForFreezerTaken = false ;
                    }
                }
            }
            Instantiate(FreezerDestroyEffect , transform.position , FreezerDestroyEffect.transform.rotation) ;
            Instantiate(CharacterFreezeEffect , effectPos.position , CharacterFreezeEffect.transform.rotation) ;
            gm.CharCameraController.FreezerSpawned = false ;
            Destroy(transform.parent.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            other.GetComponent<ObstacleHandler>().PlayFreezerCrackSound();
            if (singlePlayer)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    AIHandler handler ;
                    if (gm.MyPlayerId != i)
                    {
                        handler = players[i].GetComponent<AIHandler>() ;
                        handler.FreezerSkillSpawned = false ;
                        handler.DestForFreezerTaken = false ;
                    }
                }
            }
            Instantiate(FreezerDestroyEffect , transform.position , FreezerDestroyEffect.transform.rotation) ;
            gm.CharCameraController.FreezerSpawned = false ;
            Destroy(transform.parent.gameObject);
        }
    }

    void FindClosestTarget()
    {
        for (byte i = 0; i < players.Length; i++)
        {
            if (players[i])
            {
                if (closestTarget == null)
                {
                    closestTarget = players[i] ;
                    continue;
                }

                if (Vector3.SqrMagnitude(transform.position - closestTarget.position) >
                    Vector3.SqrMagnitude(transform.position - players[i].position))
                {
                    closestTarget = players[i] ;
                }
            }
        }
    }
}
