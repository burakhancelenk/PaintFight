using Es.InkPainter.Sample ;
using ExitGames.Client.Photon ;
using Photon.Pun ;
using Photon.Realtime ;
using UnityEngine;
using UnityEngine.AI ;
using Quaternion = UnityEngine.Quaternion ;
using Vector3 = UnityEngine.Vector3 ;

public class CharacterController : MonoBehaviour,IPunObservable,IOnEventCallback
{
    public bool RocketPicked
    {
        get { return rocketPicked ; }
        set { rocketPicked = value ; }
    }

    public bool AK47Picked
    {
        get { return ak47Picked ;}
        set { ak47Picked = value ; }
    }

    public bool SpeedSkillPicked
    {
        set { speedSkillPicked = value ; }
    }

    public bool GrowSkillPicked
    {
        get { return growSkillPicked ; }
        set { growSkillPicked = value ; }
    }

    public bool FreezerSkillPicked
    {
        set { freezerSkillPicked = value ; }
    }

    public int AK47Ammo
    {
        get { return ak47Ammo ; }
        set { ak47Ammo = value ; }
    }

    public bool ReturnToSmile
    {
        get { return returnToSmile ; }
        set { returnToSmile = value ; }
    }

    public CharacterController ClosestTargetCC
    {
        get { return closestTargetCC ; }
    }

    public Transform ClosestTarget
    {
        get { return closestTarget ; }
    }

    public Transform[] OtherPlayers
    {
        get { return otherPlayers ; }
        set
        {
            if (otherPlayers == null)
            {
                otherPlayers = value ;
            }
        }
    }

    public Color PaintColor
    {
        get { return paintColor ; }
        set { paintColor = value ; }
    }

    public bool BotPlayer
    {
        set
        {
            botPlayer = value ;
        }
        get
        {
            return botPlayer ;
        }
    }

    public int Health
    {
        get { return health ; }
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
    
    public GameObject Hands ;
    public GameObject AK47 ;
    public GameObject RocketLauncher ;
    public GameObject BoxGloves ;
    public GameObject CharacterDeathEffect ;
    public FaceExpressionHandler faceExpHandler ;
    public Transform RotRoot ;
    public CharUI CharacterUI ;
    public AudioSource SkillSoundFXAS ;
    public AudioClip[] SkillSoundFxs ;
    public AudioSource AK47Fire ;
    public AudioSource AK47Shot ;
    public AudioClip[] AK47ShotClips ;
    public AudioSource RocketShooting ;
    public AudioSource RocketExplode ;
    public AudioSource FistInAirAS ;
    public AudioSource FistHitAS ;
    public AudioClip[] FistHitClips ;
    public AudioSource CharDeath ;
    
    private Animator animatorAK47 ;
    private Animator animatorRocketLauncher ;
    private Animator animatorHands ;
    private Animator animatorBoxGloves ;
    private Transform closestTarget ;
    private Transform[] otherPlayers ;
    private CharacterController closestTargetCC ;
    private NavMeshAgent navMeshAgent ;
    private AIHandler botAiHandler ;
    private Rigidbody rb ;
    private PhotonView myPhotonView ;
    private GameManager gm ;
   
    private Color paintColor ;     // assigned by GameManager
    private bool rocketPicked ;
    private bool ak47Picked ;
    private bool speedSkillPicked ;
    private bool growSkillPicked ;
    private bool freezerSkillPicked ;
    private bool returnToSmile ;
    private int ak47Ammo ;
    private bool hasRocketAmmo ;
    private float characterSpeed = 500 ;
    private float rotationSpeed = 10 ;
    private float fistMaxIntDistance = 50;
    private bool fistLockedToOpponent ;
    private float timerForBoxGloves ;
    private float timerForUltraFist ;
    private float timerForSpeedSkill ;
    private float timerForGrowSkill ;
    private float timerForFreezerSkill ;
    private float timerForReturnToSmile ;
    private float ultraFistThreshold = 0.3f ;
    private bool checkForUltra ;
    private int health = 100 ;
    private float posY ;
    private bool botPlayer ;
    private float navmeshSpeed ;
    private float navmeshASpeed ;
    private byte ak47ShotIndex ;
    private byte fistHitIndex ;
    private bool isMyChar ;
    private bool mpGunFistButtonDown ;
    private bool mpGunFistButtonUp ;
    private byte heavyFuncWait = 1 ;
    private byte heavyFuncWaitCount ;
    public bool IsMyChar ;
    
    void Start()
    {
        animatorAK47 = AK47.GetComponent<Animator>() ;
        animatorRocketLauncher = RocketLauncher.GetComponent<Animator>() ;
        animatorHands = Hands.GetComponent<Animator>() ;
        animatorBoxGloves = BoxGloves.GetComponent<Animator>() ;
        rb = GetComponent<Rigidbody>() ;
        posY = transform.position.y ;
        if (botPlayer)
        {
            navMeshAgent = GetComponent<NavMeshAgent>() ;
            botAiHandler = GetComponent<AIHandler>() ;
            navmeshSpeed = navMeshAgent.speed ;
            navmeshASpeed = navMeshAgent.angularSpeed ;
        }

        if (IsMyChar)
        {
            isMyChar = true ;
            gm.gfButton.CharController = GetComponent<CharacterController>() ;
            myPhotonView = GetComponent<PhotonView>() ;
        }
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void FixedUpdate()
    {
        if (freezerSkillPicked)
        {
            return;
        }

        if (isMyChar)
        {
            float horizontalDirection = Input.GetAxis("Horizontal") ;
            float verticalDirection = Input.GetAxis("Vertical") ;
            rb.velocity = Vector3.ClampMagnitude(horizontalDirection * Vector3.right + verticalDirection
                                                 *Vector3.forward,1f)*characterSpeed*Time.fixedDeltaTime ;
        }
        // Freeze character position on Y axis, rigidbody constrains not work well.
        transform.position = Vector3.up*posY + 
                             Vector3.forward*transform.position.z +
                             Vector3.right*transform.position.x ;
    }

    void Update()
    {
        ++heavyFuncWaitCount ;
        if (heavyFuncWaitCount >= heavyFuncWait)
        {
            FindClosestTarget();
            heavyFuncWaitCount = 0 ;
        }
        
        if (!freezerSkillPicked)
        {
            if (rocketPicked || ak47Picked)
            {
                // character must look at the closest target
                if (closestTarget)
                {
                    if (botPlayer && navMeshAgent.updateRotation)
                    {
                        navMeshAgent.updateRotation = false ;
                    }
                    Quaternion targetRotation = Quaternion.LookRotation(closestTarget.position-transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,rotationSpeed*Time.deltaTime);
                    targetRotation = Quaternion.LookRotation(closestTarget.position-RotRoot.position+Vector3.up*1.4f);
                    RotRoot.rotation = Quaternion.Lerp(RotRoot.rotation,targetRotation,rotationSpeed*Time.deltaTime);
                }
            
                // gunshot
                HandleGunAcivities();
            }
            else
            {
                Quaternion targetRotation ;
                float horizontalDirection = Input.GetAxis("Horizontal") ;
                float verticalDirection = Input.GetAxis("Vertical") ;
                if (botPlayer && !fistLockedToOpponent)
                {
                    navMeshAgent.updateRotation = true ;
                }
                else if(isMyChar)
                { 
                    // character must look at the move direction
                    if ((Mathf.Abs(horizontalDirection) > 0 || Mathf.Abs(verticalDirection) > 0)
                        && !fistLockedToOpponent)
                    {
                        targetRotation = Quaternion.LookRotation(horizontalDirection*Vector3.right + 
                                                                 verticalDirection*Vector3.forward);
                        transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,rotationSpeed*Time.deltaTime);
                    } 
                }
                
                //Fist fight interaction, character must look at the closest target
                if (BoxGloves.activeSelf)
                {
                    if (closestTarget)
                    {
                        if ((closestTarget.position - transform.position).sqrMagnitude < fistMaxIntDistance)
                        {
                            if (botPlayer && navMeshAgent.updateRotation)
                            {
                                navMeshAgent.updateRotation = false ;
                            }
                            targetRotation = Quaternion.LookRotation(closestTarget.position-transform.position);
                            transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,rotationSpeed*Time.deltaTime);

                            if (!fistLockedToOpponent)
                            {
                                fistLockedToOpponent = true ;
                            }
                        }
                        else if(fistLockedToOpponent)
                        {
                            fistLockedToOpponent = false ;
                        }
                    }
                }
            
                // Fist
                HandleFistActivities();
                // Fist deactivation timer
                StartTimerForBoxGloves();
                // UltraFist decision timer
                StartTimerForUltraFist();
            }
        }
        // SpeedSkill deactivation timer
        StartTimerForSpeedSkill();
        // GrowSkill deactivation timer
        StartTimerForGrowSkill();
        // FreezerSkill deactivation timer
        StartTimerForFrezerSkill();
        // ReturnToSmile timer
        StartTimerForReturnToSmile();
    }

    public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position.x);
            stream.SendNext(transform.position.z);
            stream.SendNext(transform.rotation.eulerAngles.y);
        }
        else
        {
            float x = (float)stream.ReceiveNext() ;
            float y = (float)stream.ReceiveNext() ;
            float rotY = (float) stream.ReceiveNext() ;
            
            if ((!fistLockedToOpponent || (Mathf.Abs(x - transform.position.x) > 0.001f && Mathf.Abs(y - transform.position.z) > 0.001f)) 
                && !AK47Picked && !RocketPicked)
            {
                transform.rotation = Quaternion.Euler(0,rotY,0);
            } 
            transform.position = Vector3.right*x+Vector3.forward*y;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
        {
            if ((int) photonEvent.CustomData == myPhotonView.ViewID)
            {
                PlayCharDeath();
                Destroy(gameObject);
                Instantiate(CharacterDeathEffect , transform.Find("CharacterEquipments").position
                    ,CharacterDeathEffect.transform.rotation) ;
            }
        }
    }

    [PunRPC]
    private void GunFistButtonDownPressed(PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == myPhotonView.ViewID)
        {
            mpGunFistButtonDown = true ;
        }
    }

    [PunRPC]
    private void GunFistButtonUpPressed(PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == myPhotonView.ViewID)
        {
            mpGunFistButtonUp = true ;
        }
    }

    public void CallGunFistButtonDownPressedRpc()
    {
        myPhotonView.RPC("GunFistButtonDownPressed",RpcTarget.Others);
    }
    
    public void CallGunFistButtonUpPressedRpc()
    {
        myPhotonView.RPC("GunFistButtonUpPressed",RpcTarget.Others);
    }
    

    public void PickAK47()
    {
        // clear old flags, deactive equipments and prepare new gun
        RocketLauncher.SetActive(false);
        Hands.SetActive(false);
        BoxGloves.SetActive(false);
        hasRocketAmmo = false ;
        rocketPicked = false ;
        timerForBoxGloves = 0 ;
        timerForUltraFist = 0 ;
        checkForUltra = false ;
        AK47.SetActive(true);
        Hands.SetActive(true);
        ak47Ammo = 30 ;
        closestTargetCC.CharacterUI.OpenCloseCrosshair(true);
        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(false);
    }
    
    public void PickRocketLauncher()
    {
        // clear old flags, deactive equipments and prepare new gun
        AK47.SetActive(false);
        Hands.SetActive(false);
        BoxGloves.SetActive(false);
        timerForBoxGloves = 0 ;
        timerForUltraFist = 0 ;
        ak47Picked = false ;
        checkForUltra = false ;
        ak47Ammo = 0 ;
        RocketLauncher.SetActive(true);
        Hands.SetActive(true);
        animatorHands.SetBool("RocketLauncher",true);
        hasRocketAmmo = true ;
        closestTargetCC.CharacterUI.OpenCloseCrosshair(true);
        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(false);
    }

    void FindClosestTarget()
    {
        for (byte i = 0; i < otherPlayers.Length; i++)
        {
            if (otherPlayers[i])
            {
                if (closestTarget == null)
                {
                    closestTarget = otherPlayers[i] ;
                    closestTargetCC = otherPlayers[i].GetComponent<CharacterController>() ;   
                    continue;
                }

                if (Vector3.SqrMagnitude(transform.position - closestTarget.position) >
                    Vector3.SqrMagnitude(transform.position - otherPlayers[i].position))
                {
                    if (ak47Picked || rocketPicked)
                    {
                        closestTargetCC.CharacterUI.OpenCloseCrosshair(false);
                        otherPlayers[i].GetComponent<CharacterController>().CharacterUI.OpenCloseCrosshair(true);
                        otherPlayers[i].GetComponent<CharacterController>().CharacterUI.CrosshairImage.color = 
                            closestTargetCC.CharacterUI.CrosshairImage.color ;
                    }
                    closestTarget = otherPlayers[i] ;
                    closestTargetCC = otherPlayers[i].GetComponent<CharacterController>() ;
                }
            }
        }
    }

    void HandleGunAcivities()
    {
        if (closestTarget)
        {
            if (Input.GetMouseButtonDown(0) && isMyChar)
            {
                //gm.gfButton.GunFistButtonDown = false ;
                if (!gm.SinglePlayer)
                {
                    CallGunFistButtonDownPressedRpc(); 
                }
                if (hasRocketAmmo)
                {
                    animatorRocketLauncher.SetTrigger("Fire") ;
                    animatorHands.SetBool("WeaponFire" , true) ;
                    hasRocketAmmo = false ;
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                    gm.CharCameraController.GunPicked = false ;
                    //gm.gpUI.ChangeGunFistSprite(true); 
                    closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                    // trigger rocket fire anim then change hasRocketAmmo in anim event
                }
                else if (ak47Ammo > 0)
                {
                    animatorAK47.SetBool("Fire" , true) ;
                    animatorHands.SetBool("WeaponFire" , true) ;
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                    returnToSmile = false ;
                    closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                    // trigger ak47 fire anim then decrease the value of ak47Ammo in anim event
                }
            }
            else if (Input.GetMouseButtonUp(0) && isMyChar)
            {
                //gm.gfButton.GunFistButtonUp = false ;
                if (!gm.SinglePlayer)
                {
                    CallGunFistButtonUpPressedRpc();
                }
                if (AK47Picked)
                {
                    animatorAK47.SetBool("Fire" , false) ;
                    animatorHands.SetBool("WeaponFire" , false) ;
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                    closestTargetCC.CharacterUI.ChangeColorOfCrosshair(false);
                }
            }
            else if (!isMyChar && !gm.SinglePlayer)
            {
                if (mpGunFistButtonDown)
                {
                    mpGunFistButtonDown = false ;
                    if (hasRocketAmmo)
                    {
                        animatorRocketLauncher.SetTrigger("Fire") ;
                        animatorHands.SetBool("WeaponFire" , true) ;
                        hasRocketAmmo = false ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE); 
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                        // trigger rocket fire anim then change hasRocketAmmo in anim event
                    }
                    else if (ak47Ammo > 0)
                    {
                        animatorAK47.SetBool("Fire" , true) ;
                        animatorHands.SetBool("WeaponFire" , true) ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                        returnToSmile = false ;
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                        // trigger ak47 fire anim then decrease the value of ak47Ammo in anim event
                    }
                }
                else if (mpGunFistButtonUp)
                {
                    mpGunFistButtonUp = false ;
                    if (AK47Picked)
                    {
                        animatorAK47.SetBool("Fire" , false) ;
                        animatorHands.SetBool("WeaponFire" , false) ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(false);
                    }
                }
            }

            if (botPlayer)
            {
                if (botAiHandler.DecidedToShoot)
                {
                    botAiHandler.DecidedToShoot = false ;
                    if (hasRocketAmmo)
                    {
                        animatorRocketLauncher.SetTrigger("Fire") ;
                        animatorHands.SetBool("WeaponFire" , true) ;
                        hasRocketAmmo = false ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                        gm.CharCameraController.ArmedCharacters.Remove(transform) ;
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                        // trigger rocket fire anim then change hasRocketAmmo in anim event
                    }
                    else if (ak47Ammo > 0)
                    {
                        animatorAK47.SetBool("Fire" , true) ;
                        animatorHands.SetBool("WeaponFire" , true) ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                        returnToSmile = false ;
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(true);
                        // trigger ak47 fire anim then decrease the value of ak47Ammo in anim event
                    }
                }
                else if (botAiHandler.CancelShooting)
                {
                    botAiHandler.CancelShooting = false ;
                    if (AK47Picked)
                    {
                        animatorAK47.SetBool("Fire" , false) ;
                        animatorHands.SetBool("WeaponFire" , false) ;
                        faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                        closestTargetCC.CharacterUI.ChangeColorOfCrosshair(false);
                    }
                }
            }
        }

        // Check ak47 ammo
        if (ak47Ammo <= 0 && ak47Picked)
        {
            ak47Picked = false ;
            animatorAK47.SetTrigger("NoAmmo");
            animatorHands.SetTrigger("WeaponOut");
            faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
            closestTargetCC.CharacterUI.OpenCloseCrosshair(false) ;
            if (botPlayer)
            {
                gm.CharCameraController.ArmedCharacters.Remove(transform) ;
            }
            else
            {
                if (isMyChar)
                {
                    gm.CharCameraController.GunPicked = false ;
                    gm.gpUI.ChangeGunFistSprite(true); 
                }
            }
        }      
    }

    void HandleFistActivities()
    {
        if (Input.GetMouseButtonDown(0) && isMyChar)
        {
            //gm.gfButton.GunFistButtonDown = false ;
            // BoxGloves object activated and flags cleared
            if (!gm.SinglePlayer)
            {
                CallGunFistButtonDownPressedRpc();
            }
            if (!BoxGloves.activeSelf)
            {
                BoxGloves.SetActive(true); 
                faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                returnToSmile = false ;
            }
            
            checkForUltra = true ;
            timerForUltraFist = 0 ;
            timerForBoxGloves = 0 ;
        }
        else if (Input.GetMouseButtonUp(0) && isMyChar)
        {
            //gm.gfButton.GunFistButtonUp = false ;
            if (!gm.SinglePlayer)
            {
                CallGunFistButtonUpPressedRpc();
            }
            // Execute punch according to the ultra punch decider.    
            checkForUltra = false ;
            if (animatorBoxGloves.GetBool("Normal"))
            {
                animatorBoxGloves.SetBool("Finish",false);
            }
            else
            {
                if (!animatorBoxGloves.GetBool("BreakUltra"))
                {
                    float targetAnimPosition = (5f-(BoxGloves.GetComponent<BoxGlovesAnimEvents>().CurrentFrame)/24f)
                                               /7f ;
                    if (animatorBoxGloves.GetBool("LeftFist"))
                    {
                        animatorBoxGloves.Play("LeftFistUltra",0,targetAnimPosition);
                    }
                    else
                    {
                        animatorBoxGloves.Play("RightFistUltra",0,targetAnimPosition);
                    }

                    animatorBoxGloves.SetFloat("Speed",30);
                }
            }
        }
        else if (!isMyChar && !gm.SinglePlayer)
        {
            if (mpGunFistButtonDown)
            {
                mpGunFistButtonDown = false ;
                // BoxGloves object activated and flags cleared
                if (!BoxGloves.activeSelf)
                {
                    BoxGloves.SetActive(true); 
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                    returnToSmile = false ;
                }
            
                checkForUltra = true ;
                timerForUltraFist = 0 ;
                timerForBoxGloves = 0 ;
            }
            else if (mpGunFistButtonUp)
            {
                mpGunFistButtonUp = false ;
                // Execute punch according to the ultra punch decider.    
                checkForUltra = false ;
                if (animatorBoxGloves.GetBool("Normal"))
                {
                    animatorBoxGloves.SetBool("Finish",false);
                }
                else
                {
                    if (!animatorBoxGloves.GetBool("BreakUltra"))
                    {
                        float targetAnimPosition = (5f-(BoxGloves.GetComponent<BoxGlovesAnimEvents>().CurrentFrame)/24f)
                                                   /7f ;
                        if (animatorBoxGloves.GetBool("LeftFist"))
                        {
                            animatorBoxGloves.Play("LeftFistUltra",0,targetAnimPosition);
                        }
                        else
                        {
                            animatorBoxGloves.Play("RightFistUltra",0,targetAnimPosition);
                        }

                        animatorBoxGloves.SetFloat("Speed",30);
                    }
                } 
            }
        }

        if (botPlayer)
        {
            if (botAiHandler.DecidedToPunch)
            {
                botAiHandler.DecidedToPunch = false ;
                // BoxGloves object activated and flags cleared
                if (!BoxGloves.activeSelf)
                {
                    BoxGloves.SetActive(true); 
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.ANGRY);
                    returnToSmile = false ;
                }
            
                checkForUltra = true ;
                timerForUltraFist = 0 ;
                timerForBoxGloves = 0 ;
            }
            else if (botAiHandler.CancelPunch)
            {
                botAiHandler.CancelPunch = false ;
                // Execute punch according to the ultra punch decider.    
                checkForUltra = false ;
                if (animatorBoxGloves.GetBool("Normal"))
                {
                    animatorBoxGloves.SetBool("Finish",false);
                }
                else
                {
                    if (!animatorBoxGloves.GetBool("BreakUltra"))
                    {
                        float targetAnimPosition = (5f-(BoxGloves.GetComponent<BoxGlovesAnimEvents>().CurrentFrame)/24f)
                                                   /7f ;
                        if (animatorBoxGloves.GetBool("LeftFist"))
                        {
                            animatorBoxGloves.Play("LeftFistUltra",0,targetAnimPosition);
                        }
                        else
                        {
                            animatorBoxGloves.Play("RightFistUltra",0,targetAnimPosition);
                        }

                        animatorBoxGloves.SetFloat("Speed",30);
                    }
                }
            }
        }
    }

    public void DecreaseHealth(int damageAmount)
    {
        if (damageAmount > 0)
        {
            health -= damageAmount ;
            CharacterUI.TakeDamage(damageAmount);
            if (health <= 0)
            {
                PlayCharDeath();
                Destroy(gameObject);
                Instantiate(CharacterDeathEffect , transform.Find("CharacterEquipments").position
                    ,CharacterDeathEffect.transform.rotation) ;
                if (!gm.SinglePlayer)
                {
                    int viewId = myPhotonView.ViewID ;
                    RaiseEventOptions reo = new RaiseEventOptions{Receivers = ReceiverGroup.Others};
                    SendOptions so = new SendOptions{Reliability = true};
                    PhotonNetwork.RaiseEvent(1 , viewId , reo , so) ;
                }
            }
        } 
    }

    public void IncreaseHealth(int healthAmount)
    {
        if (healthAmount > 0)
        {
            health += healthAmount ;
            if (health > 100)
            {
                health = 100 ;
            }
            CharacterUI.TakeDamage(-healthAmount);
        }
    }

    private void StartTimerForBoxGloves()
    {
        if (BoxGloves.activeSelf)
        {
            if (animatorBoxGloves.GetBool("Finish"))
            {
                timerForBoxGloves += Time.deltaTime ;
                if (timerForBoxGloves >= 2)
                {
                    BoxGloves.SetActive(false);
                    fistLockedToOpponent = false ;
                    timerForBoxGloves = 0 ;
                    faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                }
            }
            else
            {
                if (timerForBoxGloves > 0)
                {
                    timerForBoxGloves = 0 ;
                }
            }
        }
    }

    private void StartTimerForUltraFist()
    {
        if (checkForUltra)
        {
            timerForUltraFist += Time.deltaTime ;
            if (timerForUltraFist > 0)
            {
                if (timerForUltraFist > ultraFistThreshold)
                {
                    animatorBoxGloves.SetBool("Normal",false);
                    animatorBoxGloves.SetBool("Finish",false);
                    checkForUltra = false ;
                }
                else
                {
                    animatorBoxGloves.SetBool("Normal",true);
                }   
            }
        }
        else if (timerForUltraFist >= 0)
        {
            timerForUltraFist = -1 ;
        }    
    }

    private void StartTimerForSpeedSkill()
    {
        if (speedSkillPicked)
        {
            timerForSpeedSkill += Time.deltaTime ;
            if (timerForSpeedSkill > 5)
            {
                timerForSpeedSkill = 0 ;
                characterSpeed = 500 ;
                GetComponentInChildren<Animator>().SetFloat("Speed",1f);
                if (botPlayer)
                {
                    navMeshAgent.speed = 8 ;
                    navMeshAgent.angularSpeed = 400 ;
                    navMeshAgent.velocity /= 2 ;
                }
                speedSkillPicked = false ;
            }
        }
    }

    private void StartTimerForGrowSkill()
    {
        if (growSkillPicked)
        {
            timerForGrowSkill += Time.deltaTime ;
            if (timerForGrowSkill > 5)
            {
                timerForGrowSkill = 0 ;
                fistMaxIntDistance /= 1.6f ;
                GetComponent<CollisionPainter>().brush.brushScale /= 1.5f ;
                transform.localScale = Vector3.one;
                growSkillPicked = false ;
            }
        }
    }

    private void StartTimerForFrezerSkill()
    {
        if (freezerSkillPicked)
        {
            timerForFreezerSkill += Time.deltaTime ;
            if (timerForFreezerSkill > 3)
            {
                timerForFreezerSkill = 0 ;
                GetComponentInChildren<Animator>().enabled = true ;
                animatorAK47.speed = 1 ;
                animatorRocketLauncher.speed = 1 ;
                animatorHands.speed = 1 ;
                freezerSkillPicked = false ;
                faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                timerForReturnToSmile = 0 ;
                if (botPlayer)
                {
                    navMeshAgent.speed = navmeshSpeed ;
                    navMeshAgent.angularSpeed = navmeshASpeed ;
                }
            }
        }
    }

    public void SetDoubleSpeed()
    {
        if (speedSkillPicked)
        {
            timerForSpeedSkill = 0 ;
            GetComponentInChildren<Animator>().SetFloat("Speed",2f);
            characterSpeed = 1000 ;
            if (botPlayer)
            {
                navMeshAgent.speed = 16 ;
                navMeshAgent.angularSpeed = 400 ;
                navMeshAgent.velocity *= 2 ;
            }
        }
    }

    public void Grow()
    {
        if (growSkillPicked)
        {
            timerForGrowSkill = 0 ;
            transform.localScale = Vector3.one*1.5f;
            fistMaxIntDistance *= 1.6f ;
            GetComponent<CollisionPainter>().brush.brushScale *= 1.5f ;
        }
    }

    public void Freeze()
    {
        if (freezerSkillPicked)
        {
            if (BoxGloves.activeSelf)
            {
                BoxGloves.SetActive(false);
                checkForUltra = false ;
                fistLockedToOpponent = false ;
            }
            else if (AK47.activeSelf)
            {
                animatorAK47.speed = 0 ;
                animatorHands.speed = 0 ;
                animatorAK47.SetBool("Fire",false);
                animatorHands.SetBool("WeaponFire",false);
            }
            else if (RocketLauncher.activeSelf)
            {
                animatorRocketLauncher.speed = 0 ;
                animatorHands.speed = 0 ;
            }
            timerForBoxGloves = 0 ;
            timerForUltraFist = 0 ;
            GetComponentInChildren<Animator>().enabled = false ;
        }
    }

    public void ActivateBombSkill()
    {
        Invoke(nameof(BombSkill),0.1f);
        Invoke(nameof(DeactivateBombSkill),0.15f);
    }

    private void BombSkill()
    {
        GetComponent<CollisionPainter>().brush.brushScale *= 15 ;
    }

    private void DeactivateBombSkill()
    {
        GetComponent<CollisionPainter>().brush.brushScale /= 15 ;
    }

    public void ActivateRocketSkill()
    {
        Invoke(nameof(DeactivateRocketSkill),5f);
    }

    public void DeactivateRocketSkill()
    {
        GetComponent<CollisionPainter>().brush.Color = paintColor ;
    }

    private void StartTimerForReturnToSmile()
    {
        if (returnToSmile)
        {
            timerForReturnToSmile += Time.deltaTime ;
            if (timerForReturnToSmile >= 1)
            {
                faceExpHandler.ChangeFaceExpression(FaceExpressionHandler.FaceExpressions.SMILE);
                timerForReturnToSmile = 0 ;
                returnToSmile = false ;
            } 
        }
        else if (timerForReturnToSmile > 0)
        {
            timerForReturnToSmile = 0 ;
        }
    }

    public void ResetReturnToSmileTimer()
    {
        timerForReturnToSmile = 0 ;
    }

    public void PlaySkillSoundFX(int index)
    {
        switch (index)
        {
                case 0:
                    SkillSoundFXAS.clip = SkillSoundFxs[0] ;
                    SkillSoundFXAS.Play();
                    break;
                case 1:
                    SkillSoundFXAS.clip = SkillSoundFxs[0] ;
                    SkillSoundFXAS.Play();
                    break;
                case 2:
                    SkillSoundFXAS.clip = SkillSoundFxs[1] ;
                    SkillSoundFXAS.Play();
                    break;
                case 3:
                    SkillSoundFXAS.clip = SkillSoundFxs[1] ;
                    SkillSoundFXAS.Play();
                    break;
                case 4:
                    SkillSoundFXAS.clip = SkillSoundFxs[1] ;
                    SkillSoundFXAS.Play();
                    break;
                case 5:
                    SkillSoundFXAS.clip = SkillSoundFxs[2] ;
                    SkillSoundFXAS.Play();
                    break;
                case 6:
                    // Freezer hit
                    SkillSoundFXAS.clip = SkillSoundFxs[3] ;
                    SkillSoundFXAS.Play();
                    Invoke(nameof(PlayDefreezeSoundFx),3f);
                    break;
        }
    }

    void PlayDefreezeSoundFx()
    {
        SkillSoundFXAS.clip = SkillSoundFxs[4] ;
        SkillSoundFXAS.Play();
    }

    public void PlayFireSoundForAK47()
    {
        AK47Fire.PlayOneShot(AK47Fire.clip);
    }

    public void PlayShotSoundForAK47()
    {
        AK47Shot.PlayOneShot(AK47ShotClips[ak47ShotIndex]);
        ak47ShotIndex++ ;
        if (ak47ShotIndex > 2)
        {
            ak47ShotIndex = 0 ;
        }
    }

    public void PlayRocketShooting()
    {
        RocketShooting.Play();
    }

    public void PlayRocketExplode()
    {
        RocketExplode.Play();
    }

    public void PlayFistInAir()
    {
        FistInAirAS.time = 0.05f ;
        FistInAirAS.Play();
    }

    public void PlayFistHit()
    {
        FistHitAS.clip = FistHitClips[fistHitIndex] ;
        FistHitAS.Play();
        fistHitIndex++ ;
        if (fistHitIndex > 2)
        {
            fistHitIndex = 0 ;
        }
    }

    public void PlayCharDeath()
    {
        CharDeath.PlayOneShot(CharDeath.clip);
    }
}
