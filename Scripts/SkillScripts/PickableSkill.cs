using System.Collections;
using System.Collections.Generic;
using Es.InkPainter.Sample ;
using UnityEngine;
using UnityEngine.AI ;

public class PickableSkill : MonoBehaviour
{
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
    
    // 0 AK47
    // 1 RocketLauncher
    // 2 Speed x2
    // 3 Grow
    // 4 Health
    // 5 Bomb
    [SerializeField]
    private byte skillIndex ;
    private GameManager gm ;
    
    public GameObject PickableSkillEffect ;
    public GameObject BombSkillEffect ;

    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>() ;
            switch (skillIndex)
            {
                case 0:
                    if (cc.BotPlayer)
                    {
                        AIHandler handler = cc.GetComponent<AIHandler>() ;
                        handler.ClearGunDecisionFlags();
                        handler.AK47Taken = true ;
                        gm.CharCameraController.ArmedCharacters.Add(other.transform);
                    }
                    else
                    {
                        gm.CharCameraController.GunPicked = true ;
                        gm.gpUI.ChangeGunFistSprite(false);
                    }
                    cc.AK47Picked = true ;
                    cc.PickAK47();
                    break;
                case 1:
                    if (cc.BotPlayer)
                    {
                        AIHandler handler = other.GetComponent<AIHandler>() ;
                        handler.ClearGunDecisionFlags();
                        handler.RocketTaken = true ;
                        gm.CharCameraController.ArmedCharacters.Add(other.transform);
                    }
                    else
                    {
                        gm.CharCameraController.GunPicked = true ;
                        gm.gpUI.ChangeGunFistSprite(false);
                    }
                    cc.RocketPicked = true ;
                    cc.PickRocketLauncher();
                    cc.faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                    cc.ReturnToSmile = false ;
                    break;        
                case 2:
                    cc.SpeedSkillPicked = true ;
                    cc.SetDoubleSpeed();
                    break;
                case 3:
                    cc.GrowSkillPicked = true ;
                    cc.Grow();
                    break;
                case 4:
                    cc.IncreaseHealth(25);
                    break;
                case 5:
                    cc.ActivateBombSkill();
                    if (!cc.BotPlayer)
                    {
                        gm.CharCameraController.ShakeCameraTrigger = true ;
                    }
                    break;
            }

            if (skillIndex != 5)
            {
                Instantiate(PickableSkillEffect , transform.position+Vector3.up*1f , PickableSkillEffect.transform.rotation) ;
            }
            else
            {
                Instantiate(BombSkillEffect , transform.position , BombSkillEffect.transform.rotation) ;
            }
            cc.PlaySkillSoundFX(skillIndex);
            gm.InformAIsSkillPicked();
            gm.CharCameraController.SkillPositions.Remove(transform.position) ; 
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            other.transform.position += Vector3.right*6;
        }
    }
}
