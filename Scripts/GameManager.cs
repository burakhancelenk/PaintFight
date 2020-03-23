using System.Collections;
using System.Collections.Generic;
using Es.InkPainter ;
using Es.InkPainter.Sample ;
using ExitGames.Client.Photon ;
using Photon.Pun ;
using Photon.Realtime ;
using UnityEngine;
using UnityEngine.AI ;
using Random = UnityEngine.Random ;

public class GameManager : MonoBehaviour,IOnEventCallback
{      
    public float RemainTime
    {
        get { return remainTime ; }
    }

    public Transform[] Players
    {
        get { return players ; }
    }

    public bool SinglePlayer
    {
        get { return singlePlayer ; }
    }

    public int MyPlayerId
    {
        get { return myPlayerId ; }
    }
    
    [Header("GamePrefs")]
    [SerializeField]
    private float skillSpawnFrequence ; 
    
    [Header("Skills")]
    public GameObject[] SkillPrefabs ;

    [Header("AI")]
    public MeshFilter AIDestMeshFilter ;

    [Header("UI")] 
    public GameObject FinishMenu ;
    public GameObject BackgroundForFinishMenu ;
    public GameObject GamePlayUI ;
    public GunFistButton gfButton ;
    public GamePlayUI gpUI ;
    public FinishMenu FMenu ;

    public JoystickPanel joystickController ;
    
    [Header("Character")]
    public GameObject CharacterPrefab ;
    public Vector3[] PlayerSpawnPositions ;
    public Vector3[] PlayerSpawnRotations ;
    public CameraController CharCameraController ;

    // Burada stok olarak smile ifadesi ve body textureları atalı olacak.
    [Header("CharacterMaterials")]
    public Material[] BodyMaterials ;
    public Material[] EyeMaterials ;
    public Material[] MouthMaterials ;
    

    [Header("CharacterFaceExpressions")] 
    public Texture[] EyeExpressionsRed ;
    public Texture[] EyeExpressionsYellow ;
    public Texture[] EyeExpressionsGreen ;
    public Texture[] EyeExpressionsBlue ;
    public Texture[] MouthExpressionsRed ;
    public Texture[] MouthExpressionsYellow ;
    public Texture[] MouthExpressionsGreen ;
    public Texture[] MouthExpressionsBlue ;
    
    [Header("Audio")] 
    public AudioSource Music ;
    public AudioSource CountDown ;

    [Space] public InkCanvas inkCanvas ;

    // index meanings: 0-red 1-yellow 2-green 3-blue
    private Transform[] players ;
    private NavMeshData navMeshData ;
    private Mesh spawnPosMesh ;
    private float remainTime ;
    private int[] playerScores ;
    private int myPlayerId ;
    private bool singlePlayer ;
    private byte numberOfPlayers ;
    private byte gameDurationInMinutes ;
    private bool calculatingScores ;
    private int numberOfPlayersAlive ;
    private int[,] allScores ;
    
    // yapay zeka botları characterPrefabtan oluşturup içine ai komponentini ekleyerek oluşturacaksın.
    
    void Start()
    {
        calculatingScores = false ;
        singlePlayer = PlayerPrefs.GetString("PlayMode") == "SinglePlayer" ;
        if (singlePlayer)
        {
            myPlayerId = PlayerPrefs.GetInt("CharColor") ;
        }
        numberOfPlayers = (byte)PlayerPrefs.GetInt("NOP") ;
        gameDurationInMinutes = (byte) PlayerPrefs.GetInt("Duration") ;
        allScores = new int[numberOfPlayers , numberOfPlayers] ;
        players = new Transform[numberOfPlayers];
        playerScores = new int[numberOfPlayers];
        remainTime = gameDurationInMinutes * 60 ;
        spawnPosMesh = new Mesh();
        Matrix4x4 localToWorld = AIDestMeshFilter.transform.localToWorldMatrix;
        Vector3[] temp = new Vector3[AIDestMeshFilter.sharedMesh.vertices.Length];
        for (int j = 0; j < temp.Length; j++)
        {
            temp[j] = localToWorld.MultiplyPoint3x4(AIDestMeshFilter.sharedMesh.vertices[j]) ;
        }
        spawnPosMesh.vertices = temp ;
        SpawnPlayers();
        StartCoroutine(HandleSkillSpawnProcess()) ;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Update()
    {
        HandleGameTime();
        CheckPlayersAreAlive();

        if (!CountDown.isPlaying && remainTime <= 6f)
        {
            CountDown.Play();            
        }
        else if (CountDown.isPlaying && remainTime <= 0.1f)
        {
            CountDown.Stop();
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 2)
        {
            object[] skillData = (object[])photonEvent.CustomData ;
            HandleSkillSpawnProcessForMP((byte)skillData[0],(Vector3)skillData[1]);
        }
        else if (photonEvent.Code == 3)
        {
            int[] scores = (int[]) photonEvent.CustomData ;
            int emptySpaceIndex = 0 ;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                for (int j = 0; j < numberOfPlayers; j++)
                {
                    if (allScores[i,j] == -1)
                    {
                        emptySpaceIndex = i ;
                    }
                }
            }

            for (int i = 0; i < numberOfPlayers; i++)
            {
                allScores[emptySpaceIndex , i] = scores[i] ;
            }
        }
    }

    IEnumerator HandleSkillSpawnProcess()
    {
        if (singlePlayer || PhotonNetwork.IsMasterClient)
        {
            while (true)
            {
                yield return new WaitForSeconds(skillSpawnFrequence) ;
                byte randomSkill = (byte)(Random.Range(0 , 70)%7) ;
                Vector3 skillPos = FindRandomPointInMesh();
                if (PhotonNetwork.IsMasterClient)
                {
                    object[] content = {randomSkill,skillPos};
                    RaiseEventOptions reo = new RaiseEventOptions(){Receivers = ReceiverGroup.Others};
                    SendOptions so = new SendOptions(){Reliability = true};
                    PhotonNetwork.RaiseEvent(2 , content , reo , so) ;
                }
                if (randomSkill != 3)
                {
                    PickableSkill ps = Instantiate(SkillPrefabs[randomSkill] , skillPos ,
                        SkillPrefabs[randomSkill].transform.rotation)
                        .GetComponent<PickableSkill>() ;
                    ps.GM = this ;
                    // inform all agents
                    if (singlePlayer)
                    {
                        float[] playerDistacesToSkill = new float[numberOfPlayers];
                        for (int i = 0; i < numberOfPlayers; i++)
                        {
                            if (!players[i])
                            {
                                playerDistacesToSkill[i] = float.PositiveInfinity ;
                                continue;
                            }
                            float distance = (players[i].transform.position - skillPos).sqrMagnitude ;
                            if (i != 0)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    if (playerDistacesToSkill[j]+9 < distance && !float.IsInfinity(playerDistacesToSkill[j]))
                                    {
                                        playerDistacesToSkill[i] = float.PositiveInfinity ;
                                        break;
                                    }
                                    else
                                    {
                                        playerDistacesToSkill[i] = distance ;
                                    }
                                } 
                            }
                            else
                            {
                                playerDistacesToSkill[i] = distance ;
                            } 
                        }
                
                        for (int i = 0; i < numberOfPlayers; i++)
                        {
                            if (myPlayerId != i && !float.IsInfinity(playerDistacesToSkill[i]))
                            {
                                AIHandler handler = players[i].GetComponent<AIHandler>() ;
                                handler.DontUpdateDest = true ;
                                handler.SkillPosition = skillPos ;
                                handler.PickSkill = true ;
                            }
                        }
                    } 
                    CharCameraController.SkillPositions.Add(ps.transform.position);
                }
                else
                {
                    Transform freezer = Instantiate(SkillPrefabs[randomSkill] , skillPos , SkillPrefabs[randomSkill].transform.rotation)
                        .GetComponent<Transform>();
                    FreezerSkill fs = freezer.Find("Freezer").GetComponent<FreezerSkill>() ;
                    fs.GM = GetComponent<GameManager>() ;
                    fs.SinglePlayer = singlePlayer ;
                    if (singlePlayer)
                    {
                        for (int i = 0; i < numberOfPlayers; i++)
                        {
                            if (!players[i])
                            {
                                continue;
                            }
                            if (myPlayerId != i)
                            {
                                AIHandler handler = players[i].GetComponent<AIHandler>() ;
                                handler.FreezerSkillSpawned = true ;
                                handler.FreezerTr = freezer.Find("Freezer") ;
                            }   
                        }
                    }

                    CharCameraController.FreezerSpawned = true ;
                    CharCameraController.FreezerTr = freezer.Find("Freezer") ;
                }
            } 
        }   
    }

    public void HandleSkillSpawnProcessForMP(byte randomSkill , Vector3 skillPos)
    {
        if (randomSkill != 3)
        {
            PickableSkill ps = Instantiate(SkillPrefabs[randomSkill] , skillPos ,
            SkillPrefabs[randomSkill].transform.rotation)
                .GetComponent<PickableSkill>() ;
            ps.GM = this ;
            CharCameraController.SkillPositions.Add(ps.transform.position);
        }
        else
        {
            Transform freezer = Instantiate(SkillPrefabs[randomSkill] , skillPos , SkillPrefabs[randomSkill].transform.rotation)
                .GetComponent<Transform>();
            FreezerSkill fs = freezer.Find("Freezer").GetComponent<FreezerSkill>() ;
            fs.GM = GetComponent<GameManager>() ;
            fs.SinglePlayer = singlePlayer ;
            CharCameraController.FreezerSpawned = true ;
            CharCameraController.FreezerTr = freezer.Find("Freezer") ;
        }
    }
    
    Vector3 FindRandomPointInMesh()
    {
        Vector3[] randomTriangle = new Vector3[3];
        int randomIndex = Random.Range(2 , spawnPosMesh.vertices.Length - 1) ;
        for (byte i = 0; i < randomTriangle.Length; i++)
        {
            randomTriangle[i] = spawnPosMesh.vertices[randomIndex-i] ;
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

    public void InformAIsSkillPicked()
    {
        if (singlePlayer)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (!players[i])
                {
                    continue;
                }
                if (myPlayerId != i)
                {
                    AIHandler handler = players[i].GetComponent<AIHandler>() ;
                    if (handler.DontUpdateDest)
                    {
                        handler.DontUpdateDest = false ;
                        handler.SetRandomDestination() ;
                    }
                }
            }
        }
    }

    void SpawnPlayers()
    {
        for (byte i = 0; i < numberOfPlayers; i++)
        {
            //Create instances
            players[i] = Instantiate(CharacterPrefab , PlayerSpawnPositions[i] ,
                                     Quaternion.Euler(PlayerSpawnRotations[i])).transform ;
            players[i].GetComponent<CharacterController>().GM = GetComponent<GameManager>() ;
            if (!singlePlayer)
            {
                int temp = MultiplayerManager.SetViewId(i,players[i]);
                if (temp != -1)
                {
                    myPlayerId = temp ;
                    players[temp].GetComponent<CharacterController>().IsMyChar = true ;
                    CharCameraController.MyCharacterRb = players[i].GetComponent<Rigidbody>() ;
                    CharCameraController.MyCharacterTr = players[i].transform ;
                    CharCameraController.Cc = players[temp].GetComponent<CharacterController>() ;
                }
            }

            if (myPlayerId != i && singlePlayer)
            {
                NavMeshAgent na = players[i].gameObject.AddComponent<NavMeshAgent>() ;
                na.radius = 1.4f ;
                na.height = 2.75f ;
                na.agentTypeID = 0 ;
                na.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance ;
                na.speed = 8 ;
                na.angularSpeed = 400 ;
                na.autoBraking = false ;
                na.acceleration = 100 ;
                AIHandler ah = players[i].gameObject.AddComponent<AIHandler>() ;
                ah.DestMesh = spawnPosMesh ;
                players[i].GetComponent<CharacterController>().BotPlayer = true ;
            }
            else if (myPlayerId == i && singlePlayer)
            {
                players[i].GetComponent<CharacterController>().IsMyChar = true ;
                CharCameraController.MyCharacterRb = players[i].GetComponent<Rigidbody>() ;
                CharCameraController.MyCharacterTr = players[i].transform ;
                CharCameraController.Cc = players[i].GetComponent<CharacterController>() ;
            }
            // Assign materials
            MeshRenderer mr = players[i].Find("CharacterEquipments").Find("CharacterShape")
                .GetComponent<MeshRenderer>() ;
            mr.sharedMaterials = new Material[]{BodyMaterials[i],EyeMaterials[i],MouthMaterials[i]};
            // Paint color and box gloves color assignments 
            switch (i)
            {
                    case 0:
                        players[i].GetComponent<CharacterController>().PaintColor = Color.red;
                        players[i].GetComponent<CollisionPainter>().brush.Color = Color.red;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("LeftBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.red;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("RightBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.red;
                        break;
                    case 1:
                        players[i].GetComponent<CharacterController>().PaintColor = Color.yellow;
                        players[i].GetComponent<CollisionPainter>().brush.Color = Color.yellow;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("LeftBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.yellow;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("RightBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.yellow;
                        break;
                    case 2:
                        players[i].GetComponent<CharacterController>().PaintColor = Color.green;
                        players[i].GetComponent<CollisionPainter>().brush.Color = Color.green;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("LeftBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.green;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("RightBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.green;
                        break;
                    case 3:
                        players[i].GetComponent<CharacterController>().PaintColor = Color.blue;
                        players[i].GetComponent<CollisionPainter>().brush.Color = Color.blue;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("LeftBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.blue;
                        players[i].Find("CharacterEquipments").Find("BoxGloves").Find("RightBoxGlove").Find("Cube")
                            .GetComponent<MeshRenderer>().material.color = Color.blue;
                        break;
            }
            // Face expression textures assignment
            for (byte j = 0; j < 4; j++)
            {
                switch (i)
                {
                        case 0:
                            players[i].GetComponent<FaceExpressionHandler>().EyeExpressions[j] = 
                                EyeExpressionsRed[j] ;
                            break;
                        case 1:
                            players[i].GetComponent<FaceExpressionHandler>().EyeExpressions[j] = 
                                EyeExpressionsYellow[j];
                            break;
                        case 2:
                            players[i].GetComponent<FaceExpressionHandler>().EyeExpressions[j] =
                                EyeExpressionsGreen[j] ;
                            break;
                        case 3:
                            players[i].GetComponent<FaceExpressionHandler>().EyeExpressions[j] =
                                EyeExpressionsBlue[j] ;
                            break;
                }  
            }
            for (byte j = 0; j < 3; j++)
            {
                switch (i)
                {
                        case 0:
                            players[i].GetComponent<FaceExpressionHandler>().MouthExpressions[j] = MouthExpressionsRed[j] ;
                            break;
                        case 1:
                            players[i].GetComponent<FaceExpressionHandler>().MouthExpressions[j] = MouthExpressionsYellow[j] ;
                            break;
                        case 2:
                            players[i].GetComponent<FaceExpressionHandler>().MouthExpressions[j] = MouthExpressionsGreen[j] ;
                            break;
                        case 3:
                            players[i].GetComponent<FaceExpressionHandler>().MouthExpressions[j] = MouthExpressionsBlue[j] ;
                            break;
                }
            }
        }

        for (byte i = 0; i < numberOfPlayers; i++)
        {
            int index = 0 ;
            Transform[] otherPlayers = new Transform[numberOfPlayers-1];
            for (byte j = 0; j < numberOfPlayers; j++)
            {
                if (i == j)
                {
                    continue;
                }
                otherPlayers[index] = players[j].transform ;
                index++ ;
            }
            players[i].GetComponent<CharacterController>().OtherPlayers = otherPlayers ;
        }
    }

    void HandleGameTime()
    {
        remainTime -= Time.deltaTime ;
        if (remainTime < 0 && !calculatingScores)
        {
            Time.timeScale = 0 ;
            GamePlayUI.SetActive(false);
            FinishMenu.SetActive(true);
            BackgroundForFinishMenu.SetActive(true);
            calculatingScores = true ;
            FinishGame();
        }
    }

    void CheckPlayersAreAlive()
    {
        numberOfPlayersAlive = 0 ;
        for (byte i = 0 ; i < numberOfPlayers ; i++)
        {
            if (players[i])
            {
                numberOfPlayersAlive++ ;
            }
        }

        if (numberOfPlayersAlive > 1)
        {
            return;
        }

        if (!calculatingScores)
        {
            Time.timeScale = 0 ;
            GamePlayUI.SetActive(false);
            FinishMenu.SetActive(true);
            BackgroundForFinishMenu.SetActive(true);
            calculatingScores = true ;
            FinishGame();
        }
    }
    
    private void FindTheWinner()
    {
        RenderTexture source = inkCanvas.PaintDatas[0].paintMainTexture;
        
        //Create fake texture to get texture from the rendertexture
        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);  
        RenderTexture.active = source;
        texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        texture.Apply();

        //Get Color of each pixel in an array.
        Color[] pixelColors = texture.GetPixels() ;
        Color[] playerColors = new Color[numberOfPlayers] ;

        for (int i = 0; i < numberOfPlayers; i++)
        {
            switch (i)
            {
                    case 0:
                        playerColors[i] = Color.red;
                        break;
                    case 1:
                        playerColors[i] = Color.yellow;
                        break;
                    case 2:
                        playerColors[i] = Color.green;
                        break;
                    case 3:
                        playerColors[i] = Color.blue;
                        break;
            }
            playerScores[i] = 0 ;
        }

        //Check Every pixel to compare which one is painted.
        for (int i = 0; i < pixelColors.Length; i++)
        {
            for (int y = 0; y < playerColors.Length; y++)
            {
                if (CheckColorEquality(pixelColors[i] , playerColors[y]))
                {
                    playerScores[y]++ ;
                }
            }
        }
    }

    //Check Equality colors with range
    private bool CheckColorEquality(Color color1, Color color2)
    {
        if (((int)(color1.r * 1000) <= (int)(color2.r * 1000 + 20) && (int)(color1.r * 1000) >= (int)(color2.r * 1000 - 20)))
        {
            if ((int)(color1.g * 1000) <= (int)(color2.g * 1000 + 20) && (int)(color1.g * 1000) >= (int)(color2.g * 1000 - 20))
            {
                if ((int) (color1.b * 1000) <= (int) (color2.b * 1000 + 20) &&
                    (int) (color1.b * 1000) >= (int) (color2.b * 1000 - 20))
                {
                    return true;
                }   
            }
        }
        return false ;
    }


    void FinishGame()
    {
        FindTheWinner();
        
        if (singlePlayer)
        {
            for (int i = 0; i < numberOfPlayers ; i++)
            {
                FMenu.ScoresValues[i] = playerScores[i] ;
            }
            FMenu.VisualizeScores();
            Music.Pause();
        }
        else
        {
            int emptySpaceIndex = 0 ;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                for (int j = 0; j < numberOfPlayers; j++)
                {
                    if (allScores[i,j] == -1)
                    {
                        emptySpaceIndex = i ;
                    }
                }
            }

            for (int i = 0; i < numberOfPlayers; i++)
            {
                allScores[emptySpaceIndex , i] = playerScores[i] ;
            }
            RaiseEventOptions reo = new RaiseEventOptions(){Receivers = ReceiverGroup.Others};
            SendOptions so = new SendOptions(){Reliability = true};
            PhotonNetwork.RaiseEvent(3 , playerScores , reo , so) ;
            StartCoroutine(CalculateScoreForMP()) ;
        }
    }

    IEnumerator CalculateScoreForMP()
    {
        while (true)
        {
            bool allScoresFilled = true ;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                for (int j = 0; j < numberOfPlayers; j++)
                {
                    if (allScores[i,j] == -1)
                    {
                        allScoresFilled = false ;
                    }
                }
            }
            if (allScoresFilled)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    int temp = 0 ;
                    for (int j = 0; j < numberOfPlayers; j++)
                    {
                        temp += allScores[j,i] ;
                    }
                    temp /= numberOfPlayers ;
                    FMenu.ScoresValues[i] = temp ;
                }
                FMenu.VisualizeScores();
                Music.Pause();
                break;
            }
            yield return null ;
        }
    }
}

