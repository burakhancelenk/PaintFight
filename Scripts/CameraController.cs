using System.Collections;
using System.Collections.Generic;
using System.Globalization ;
using UnityEditor ;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Rigidbody MyCharacterRb
    {
        set
        {
            if (!myCharacterRb)
            {
                myCharacterRb = value ;
            }
        }
    }
    
    public Transform MyCharacterTr
    {
        set
        {
            if (!myCharacterTr)
            {
                myCharacterTr = value ;
            }
        }
    }

    public CharacterController Cc
    {
        set
        {
            if (cc == null)
            {
                cc = value ;
            }
        }
    }

    public List<Vector3> SkillPositions
    {
        get
        {
            return skillPositions ;
        }
    }

    public List<Transform> ArmedCharacters
    {
        get { return armedCharacters ; }
    }

    public bool GunPicked
    {
        set { gunPicked = value ; }
    }

    public bool FreezerSpawned
    {
        set { freezerSpawned = value ; }
    }

    public Transform FreezerTr
    {
        set { freezerTr = value ; }
    }

    public bool IsFocusedOnCharActive
    {
        set { isFocusedOnCharActive = value ; }
    }

    public bool ShakeCameraTrigger ;
    
    private Rigidbody myCharacterRb ;
    private Transform myCharacterTr ;

    private bool characterMoving ;
    private float camMovSpeed = 1 ;
    private float movRadius = 20 ;
    private Vector3 offset = new Vector3(0f,45f,-17f) ;

    private CharacterController cc ;
    private float fistZoomValue = 15f ;
    private float fistZoomMaxDistance = 64 ;
    private bool fistFightActivated ;

    private List<Vector3> skillPositions = new List<Vector3>();
    private List<Transform> armedCharacters = new List<Transform>() ;
    private bool gunPicked ;
    private bool freezerSpawned ;
    private Transform freezerTr ;
    private bool skillAndGunIntActivated ;
    private bool isFocusedOnCharActive ;
    

    void Start()
    {
        isFocusedOnCharActive = PlayerPrefs.GetString("CamSetting") != "Adaptive" ;
        characterMoving = false ;
    }

    void Update()
    {
        if (myCharacterRb.velocity.sqrMagnitude < 0.1 && characterMoving)
        {
            characterMoving = false ;
        }
        else if (myCharacterRb.velocity.sqrMagnitude >= 0.1 && !characterMoving)
        {
            characterMoving = true ;
        }

        if (cc.ClosestTarget != null && !isFocusedOnCharActive)
        {
            if ((cc.ClosestTarget.position - myCharacterTr.position).sqrMagnitude < fistZoomMaxDistance)
            {
                fistFightActivated = true ;
            }
            else
            {
                fistFightActivated = false ;
            }
        }
    }

    void LateUpdate()
    {
        FollowCharacter();
        if (!isFocusedOnCharActive)
        {
            FistFight();
            SkillAndGunInteractions();
        }
        if (ShakeCameraTrigger)
        {
            ShakeCameraTrigger = false ;
            StartCoroutine(ShakeCamera()) ;
        }   
    }

    void FollowCharacter()
    {
        if (fistFightActivated || skillAndGunIntActivated)
        {
            return;
        }
        if (!characterMoving)
        {
            transform.position = Vector3.Lerp(transform.position , myCharacterTr.position + offset ,
                camMovSpeed * Time.smoothDeltaTime) ;
        }
        else if(characterMoving)
        {
            Vector3 pivotPoint = new Vector3(1,0,0);
            Vector3 charDirection = myCharacterRb.velocity;

            float angle = Vector3.SignedAngle(pivotPoint , -charDirection,Vector3.up);
            float targetPosX = movRadius*1.5f * Mathf.Cos(angle*Mathf.Deg2Rad) ;
            float targetPosZ = movRadius * Mathf.Sin(angle*Mathf.Deg2Rad) ;
            
            transform.position =Vector3.Lerp(transform.position,
                myCharacterTr.position + offset+ new Vector3(-targetPosX,0,targetPosZ),camMovSpeed*Time.smoothDeltaTime);
        }
    }

    void FistFight()
    {
        if (fistFightActivated && !skillAndGunIntActivated)
        {
            Vector3 midPoint = (myCharacterTr.position + cc.ClosestTarget.position) / 2f ;
            Vector3 zoom = (midPoint - transform.position).normalized * fistZoomValue ;
            transform.position =
                Vector3.Lerp(transform.position , midPoint + offset + zoom , camMovSpeed * Time.smoothDeltaTime) ;
        }
    }

    void SkillAndGunInteractions()
    {
        Vector3 midPoint = myCharacterTr.position ;
        Vector3 pivotPointForZoom = myCharacterTr.position ;
        int posCount = 1 ;
        
        if (skillPositions.Count > 0)
        {
            foreach (var pos in skillPositions)
            {
                midPoint += pos ;
                posCount++ ;
            }
        }

        if (armedCharacters.Count > 0)
        {
            foreach (var tr in armedCharacters)
            {
                midPoint += tr.position ;
                posCount++ ;
            }
        }

        if (gunPicked)
        {
            midPoint += cc.ClosestTarget.position ;
            posCount++ ;
        }

        if (freezerSpawned)
        {
            midPoint += freezerTr.position ;
            posCount++ ;
        }

        if (posCount == 1)
        {
            skillAndGunIntActivated = false ;
        }
        else
        {
            skillAndGunIntActivated = true ;
        }

        if (skillAndGunIntActivated)
        {
            midPoint /= posCount ;
            List<Vector3> positions = new List<Vector3>();
            
            foreach (var pos in skillPositions)
            {
                positions.Add(pos);
            }

            foreach (var tr in armedCharacters)
            {
               positions.Add(tr.position);
            }

            if (gunPicked)
            {
                positions.Add(cc.ClosestTarget.position);
            }

            if (freezerSpawned)
            {
               positions.Add(freezerTr.position);
            }

            for (int i = 0; i < positions.Count ; i++)
            {
                if (i == 0)
                {
                    pivotPointForZoom = positions[i] ;
                    continue;
                }

                if ((midPoint-pivotPointForZoom).sqrMagnitude < (midPoint-positions[i]).sqrMagnitude)
                {
                    pivotPointForZoom = positions[i] ;
                }
            }

            Vector3 zoomVector ;
            if (pivotPointForZoom.z > midPoint.z)
            {
                zoomVector = ((pivotPointForZoom - midPoint).magnitude * offset / 31f) - offset ;
            }
            else
            {
                zoomVector = ((pivotPointForZoom - midPoint).magnitude * offset / 21f) - offset ;
            }

            if (Vector3.Angle(zoomVector,offset) >= 160)
            {
                zoomVector = Vector3.zero;
            }

            if ((midPoint + offset + zoomVector).y > 60)
            {
                float temp = (midPoint + offset + zoomVector).y -60 ;
                temp = (zoomVector.y - temp) / zoomVector.y ;
                zoomVector = zoomVector * temp ;
            }
            
            if (myCharacterTr.position.z > midPoint.z)
            {
                if(((myCharacterTr.position - midPoint).magnitude * offset / 31f + midPoint).y > 55)
                {
                    midPoint += (myCharacterTr.position - midPoint) * 0.5f ;
                }
            }
            else
            {
                if(((myCharacterTr.position - midPoint).magnitude * offset / 21f + midPoint).y > 55)
                {
                    midPoint += (myCharacterTr.position - midPoint) * 0.5f ;
                }
            }
            transform.position = Vector3.Lerp(transform.position , midPoint + offset + zoomVector 
                , camMovSpeed * Time.smoothDeltaTime) ;
        }
    }

    IEnumerator ShakeCamera()
    {
        float shakeDuration = 1.1f ;
        float shakeAmount = 2 ;
        float shakeSpeed = 25 ;
        float posChangeTime = 0.05f ;
        Vector3 originalPos = transform.localPosition ;
        float randomDegree = Random.Range(0,360) ;
        Vector3 targetPos = (Vector3.right*Mathf.Cos(randomDegree*Mathf.Deg2Rad)
                             +Vector3.forward*Mathf.Sin(randomDegree*Mathf.Deg2Rad)) * shakeAmount ;

        while (shakeDuration > 0)
        {
            if (posChangeTime < 0)
            {
                targetPos = -targetPos*3/4 ;
                posChangeTime = 0.06f ;
            }
            else
            {
                posChangeTime -= Time.deltaTime ;
            }
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos 
                                                                            + targetPos,shakeSpeed*Time.smoothDeltaTime);
            shakeDuration -= Time.deltaTime;
            yield return null ;
        }
    }
}





