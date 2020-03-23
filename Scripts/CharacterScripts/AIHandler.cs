using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI ;

public class AIHandler : MonoBehaviour
{
    
    public bool DecidedToShoot
    {
        get { return decidedToShoot ; }
        set
        {
            if (!value)
            {
                decidedToShoot = false ;
            }
        }
    }
    

    public bool CancelShooting
    {
        get { return cancelShooting ; }
        set
        {
            if (!value)
            {
                cancelShooting = false ;
            }
        }
    }

    public bool DecidedToPunch
    {
        get { return decidedToPunch ; }
        set
        {
            if (!value)
            {
                decidedToPunch = false ;
            }
        }
    }
    
    public bool CancelPunch
    {
        get { return cancelPunch ; }
        set
        {
            if (!value)
            {
                cancelPunch = false ;
            }
        }
    }

    public bool AK47Taken
    {
        set { ak47Taken = value ; }
    }

    public bool RocketTaken
    {
        set { rocketTaken = value ; }
    }

    public Mesh DestMesh
    {
        set
        {
                destMesh = value ;
        }
        get { return destMesh ; }
    }

    public Vector3 SkillPosition
    {
        set { skillPosition = value ; }
    }

    public bool PickSkill
    {
        set { pickSkill = value ; }
    }

    public bool DontUpdateDest
    {
        set { dontUpdateDest = value ; }
        get { return dontUpdateDest ; }
    }

    public bool FreezerSkillSpawned
    {
        set { freezerSkillSpawned = value ; }
    }

    public Transform FreezerTr
    {
        set { freezerTr = value ; }
    }

    public bool DestForFreezerTaken
    {
        set { destForFreezerTaken = value ; }
    }

    private Transform meshPointsParent ;
    private Vector3 previousPosition ;
    private NavMeshAgent navMeshAgent ;
    private float lastAngleY ;
    private float angleThresholdForAP ;
    private bool decidedToShoot ;
    private bool cancelShooting ;
    private bool decidedToPunch ;
    private bool cancelPunch ;
    private bool ak47Taken ;
    private bool rocketTaken ;
    private CharacterController cc ;
    private Mesh destMesh ;
    private Vector3 targetPos ;
    private List<Vector3> paintedAreaVertices ;
    private Vector3 skillPosition ;
    private bool pickSkill ;
    private bool dontUpdateDest ;
    private bool freezerSkillSpawned ;
    private bool destForFreezerTaken ;
    private Vector3 destForFreezer ;
    private Transform freezerTr ;
    private Transform target ;
    private bool targetDetected ;
    private bool targetIsDest ;
    private bool gunDestOverride ;
    private float gunDecisionTimer ;
    private float rocketDecisionWaitTime ;
    private bool startGunDecisionTimer ;
    private float timerForUFDecision ;
    private bool startTimerForUFDecision ;
    private bool punchUltraFist ;
    
    

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>() ;
        paintedAreaVertices = new List<Vector3>();
        lastAngleY = transform.rotation.y ;
        angleThresholdForAP = 5 ;
        navMeshAgent = GetComponent<NavMeshAgent>() ;
        TrackAreaPainting() ;
        SetRandomDestination();
        StartCoroutine(PunchDecision()) ;
    }

    // Update is called once per frame
    void Update()
    {
        ControlApTrackingConditions() ;
        if ((targetPos-transform.position).sqrMagnitude < 25 && !dontUpdateDest && !freezerSkillSpawned && !gunDestOverride)
        {
            SetRandomDestination();
        }

        if (pickSkill && !freezerSkillSpawned && !gunDestOverride)
        {
            navMeshAgent.SetDestination(skillPosition) ;
            pickSkill = false ;
        }

        if (freezerSkillSpawned && !destForFreezerTaken)
        {
            if (pickSkill)
            {
                pickSkill = false ;
                dontUpdateDest = false ;
            }
            StayAwayFromFreezer();
            destForFreezerTaken = true ;
        }
        else if (destForFreezerTaken)
        {
            if (pickSkill)
            {
                pickSkill = false ;
                dontUpdateDest = false ;
            }
            if ((destForFreezer - transform.position).sqrMagnitude < 1)
            {
                StayAwayFromFreezer();
            }
        }
        
        GunDecision();
        if (startTimerForUFDecision)
        {
            timerForUFDecision += Time.deltaTime ;
            if (timerForUFDecision > 1)
            {
                punchUltraFist = true ;
                startTimerForUFDecision = false ;
                timerForUFDecision = 0 ;
            }
        }
        
    }

    void TrackAreaPainting()
    {
        lastAngleY = transform.rotation.eulerAngles.y ;
        previousPosition = transform.position ;
        paintedAreaVertices.Add(transform.position) ;
    }

    void ControlApTrackingConditions()
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.y - lastAngleY) >= angleThresholdForAP)
        {
            if ((previousPosition - transform.position).sqrMagnitude >= 4)
            {
                TrackAreaPainting() ;
            }
        }
    }

    public void SetRandomDestination()
    {
        Vector3 randomPointInTriangle = FindRandomPointInMesh() ;
        TrackAreaPainting();
        Vector3 destinationVector = randomPointInTriangle - transform.position ;
        Vector3 paintedPointsVector ;
        float degree ;
        if (paintedAreaVertices.Count > 2)
        {
            for (int i=0 ; i<paintedAreaVertices.Count-1 ; i++)
            {
                paintedPointsVector = paintedAreaVertices[i + 1] - paintedAreaVertices[i] ;
                degree = Vector3.Angle(paintedPointsVector , destinationVector) ;
                if ((degree < 4 && degree >=0) || (degree >176 && degree <=180))
                {
                    if ((paintedAreaVertices[i]-transform.position).sqrMagnitude <= 3)
                    {
                        randomPointInTriangle = FindRandomPointInMesh() ;
                        break;
                    }
                    if ((paintedAreaVertices[i+1]-transform.position).sqrMagnitude <= 3)
                    {
                        randomPointInTriangle = FindRandomPointInMesh() ;
                        break;
                    }
                }   
            }
        }
        targetPos = randomPointInTriangle ;
        navMeshAgent.SetDestination(randomPointInTriangle) ;
    }

    Vector3 FindRandomPointInMesh()
    {
        Vector3[] randomTriangle = new Vector3[3];
        int randomIndex = Random.Range(2 , destMesh.vertices.Length - 1) ;
        for (byte i = 0; i < randomTriangle.Length; i++)
        {
            randomTriangle[i] = destMesh.vertices[randomIndex-i] ;
        }
        
        // Pick a random point inside this triangle with barycentric coordinate system
        // 0 index is start point
        Vector3[] vectorsForCoordinate = new Vector3[2];
        vectorsForCoordinate[0] = randomTriangle[2] - randomTriangle[0] ;
        vectorsForCoordinate[1] = randomTriangle[1] - randomTriangle[0] ;
        
        // random vector weights
        float w1 = Random.Range(0f , 1f) ;
        float w2 = Random.Range(0f , 1f) ;

        // if the sum of 2 weight bigger than 1, make it smaller
        if (w1+w2 > 1)
        {
            w1 = 1 - w1 ;
            w2 = 1 - w2 ;
        }
        // calculate the random point in triangle
        Vector3 randomPointInTriangle =
            vectorsForCoordinate[0] * w1 + vectorsForCoordinate[1] * w2 + randomTriangle[0] ;
        return randomPointInTriangle ;
    }

    IEnumerator PunchDecision()
    {
        WaitForSeconds loopFrequency = new WaitForSeconds(0.3f) ;
        float ultraWaitTime ;
        while (true)
        {
            if (!ak47Taken && !rocketTaken)
            {
                for (byte i = 0; i < cc.OtherPlayers.Length; i++)
                {
                    if (!cc.OtherPlayers[i])
                    {
                        continue;
                    }
                    if ((transform.position-cc.OtherPlayers[i].position).sqrMagnitude <= 16)
                    {
                        decidedToPunch = true ;
                        if (!punchUltraFist)
                        {
                            Invoke(nameof(ReleasePunch),0.1f);
                            startTimerForUFDecision = true ;
                        }
                        else
                        {
                            if (Random.Range(0,10)%2 == 0)
                            {
                                ultraWaitTime = Random.Range(0f,2f) ; 
                                yield return new WaitForSeconds(ultraWaitTime);
                                cancelPunch = true ;
                                punchUltraFist = false ;
                            }
                        }
                        break;
                    }
                }
            }
            yield return loopFrequency ;
        }
    }

    void ReleasePunch()
    {
        cancelPunch = true ;
    }

    void GunDecision()
    {
        if (ak47Taken && !freezerSkillSpawned)
        {
            if (!targetDetected)
            {
                for (int i = 0; i < cc.OtherPlayers.Length; i++)
                {
                    if (!cc.OtherPlayers[i])
                    {
                        continue;
                    }
                    if (cc.OtherPlayers[i].GetComponent<CharacterController>().Health <= 30)
                    {
                        target = cc.OtherPlayers[i] ;
                        targetDetected = true ;
                        gunDestOverride = true ;
                    }
                }

                if (!targetDetected)
                {
                    targetDetected = true ;
                    SetRandomDestination();
                    decidedToShoot = true ;
                    return;
                }
                navMeshAgent.SetDestination(target.position) ;
                startGunDecisionTimer = true ;
            }

            if (startGunDecisionTimer)
            {
                gunDecisionTimer += Time.deltaTime ;
                if (gunDecisionTimer > 5  && cc.ClosestTarget != target)
                {
                    SetRandomDestination();
                    gunDestOverride = false ;
                    decidedToShoot = true ;
                    gunDecisionTimer = 0 ;
                    startGunDecisionTimer = false ;
                }
                else if (target != null)
                {
                    if ((transform.position-target.position).sqrMagnitude > 9 && !targetIsDest)
                    {
                        navMeshAgent.SetDestination(target.position) ;
                        targetIsDest = true ;
                    }
                   
                    if (cc.ClosestTarget == target)
                    {
                        if ((transform.position - target.position).sqrMagnitude <= 6 && targetIsDest)
                        {
                            SetRandomDestination();
                            targetIsDest = false ;
                        }   
                        decidedToShoot = true ;
                        gunDecisionTimer = 0 ;
                    }
                    else
                    {
                        cancelShooting = true ;
                    }
                }
                else if (gunDestOverride)
                {
                    gunDestOverride = false ;
                    SetRandomDestination();
                }
            }

            if (cc.AK47Ammo <= 0)
            {
                AK47Taken = false ;
            }
        }
        else if (rocketTaken && !freezerSkillSpawned)
        {
            if (rocketDecisionWaitTime <= 0)
            {
                rocketDecisionWaitTime = Random.Range(1f , 5f) ;
            }

            if (gunDecisionTimer > rocketDecisionWaitTime)
            {
                decidedToShoot = true ;
                rocketTaken = false ;
            }
            else
            {
                gunDecisionTimer += Time.deltaTime ;
            }
        }
        else if (freezerSkillSpawned && ! decidedToShoot)
        {
            decidedToShoot = true ;
            gunDestOverride = false ;
            rocketTaken = false ;
        }
    }

    public void ClearGunDecisionFlags()
    {
        gunDecisionTimer = 0 ;
        rocketDecisionWaitTime = -1 ;
        gunDestOverride = false ;
        startGunDecisionTimer = false ;
        targetDetected = false ;
        targetIsDest = false ;
        target = null ;
    }
    
    

    void StayAwayFromFreezer()
    {
        if (freezerSkillSpawned)
        {
            bool posXMustSmaller = freezerTr.position.x > transform.position.x ;
            bool posZMustSmaller = freezerTr.position.z > transform.position.z ;

            List<Vector3> possibleVertices = new List<Vector3>();
            for (int i = 0; i < destMesh.vertexCount; i++)
            {
                if (posXMustSmaller && destMesh.vertices[i].x <= transform.position.x)
                {
                    if (posZMustSmaller && destMesh.vertices[i].z <= transform.position.z)
                    {
                        possibleVertices.Add(destMesh.vertices[i]);
                    }
                    else if (!posZMustSmaller && destMesh.vertices[i].z >= transform.position.z)
                    {
                        possibleVertices.Add(destMesh.vertices[i]);
                    }
                }
                else if (!posXMustSmaller && destMesh.vertices[i].x >= transform.position.x)
                {
                    if (posZMustSmaller && destMesh.vertices[i].z <= transform.position.z)
                    {
                        possibleVertices.Add(destMesh.vertices[i]);
                    }
                    else if (!posZMustSmaller && destMesh.vertices[i].z >= transform.position.z)
                    {
                        possibleVertices.Add(destMesh.vertices[i]);
                    }
                }
            }
            
            Vector3[] randomTriangle = new Vector3[3];
            int randomIndex ;
            if (possibleVertices.Count > 4)
            {
                randomIndex = Random.Range(2 , possibleVertices.Count - 1) ;
                for (byte i = 0; i < randomTriangle.Length; i++)
                {
                    randomTriangle[i] = possibleVertices[randomIndex-i] ;
                }
            }
            else
            {
                randomIndex = Random.Range(2 , destMesh.vertexCount - 1) ;
                for (byte i = 0; i < randomTriangle.Length; i++)
                {
                    randomTriangle[i] = destMesh.vertices[randomIndex-i] ;
                }
            }
        
            // Pick a random point inside this triangle with barycentric coordinate system
            // 0 index is start point
            Vector3[] vectorsForCoordinate = new Vector3[2];
            vectorsForCoordinate[0] = randomTriangle[2] - randomTriangle[0] ;
            vectorsForCoordinate[1] = randomTriangle[1] - randomTriangle[0] ;
        
            // random vector weights
            float w1 = Random.Range(0f , 1f) ;
            float w2 = Random.Range(0f , 1f) ;

            // if the sum of 2 weight bigger than 1, make it smaller
            if (w1+w2 > 1)
            {
                w1 = 1 - w1 ;
                w2 = 1 - w2 ;
            }
            // calculate the random point in triangle
            Vector3 randomPointInTriangle =
                vectorsForCoordinate[0] * w1 + vectorsForCoordinate[1] * w2 + randomTriangle[0] ;
            targetPos = randomPointInTriangle ;
            navMeshAgent.SetDestination(randomPointInTriangle) ;
            destForFreezer = randomPointInTriangle ;
        }
    }
}
