using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement ;
using UnityEngine.SocialPlatforms.Impl ;
using UnityEngine.UI ;

public class FinishMenu : MonoBehaviour
{
    public float[] ScoresValues
    {
        get { return scores ; }
        set { scores = value ; }
    }
    
    public Button MainMenuButton ;
    public RectTransform[] ScoreBars ;
    public Text[] Scores ;
    public Text ResultText ;
    public Image Star1 ;
    public Image Star2 ;
    public Sprite star ;
    
    [Header("Audio")] 
    public AudioSource SoundFX ;
    public AudioSource WinAS ;
    public AudioSource LoseAS ;
    
    private float[] scores ;
    private GameManager gm ;
    private MultiplayerManager MpManager ;
    
    void Start()
    {
        MainMenuButton.onClick.AddListener(MainMenuButtonClicked);
        gm = GameObject.Find("GameManager").GetComponent<GameManager>() ;
        scores = new float[PlayerPrefs.GetInt("NOP")];
        MpManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>() ;
    }
    
    void MainMenuButtonClicked()
    {
        SoundFX.Play();
        Time.timeScale = 1 ;
        if (gm.SinglePlayer)
        {
            SceneManager.LoadScene(0) ;
        }
        else
        {
            MultiplayerManager.DisconnectionAttempt = true ;
            MpManager.DisconnectFromServer();
            SceneManager.LoadScene(0) ;
        }
        
    }

    public void VisualizeScores()
    {
        int numberofPlayer = PlayerPrefs.GetInt("NOP") ;
        int myRank = 1 ;
        float totalAmount = 0 ;
        float biggestScore = 0 ;
        float[] scorePercentages = new float[numberofPlayer];
        
        for (int i = 0; i < numberofPlayer; i++)
        {
            totalAmount += scores[i] ;
            if (i != gm.MyPlayerId)
            {
                if (scores[gm.MyPlayerId] < scores[i])
                {
                    myRank++ ;
                }
            }
        }
        
        for (int i = 0; i < numberofPlayer; i++)
        {
            scorePercentages[i] = scores[i] / totalAmount * 100f ;
        }

        for (int i = 0; i < numberofPlayer; i++)
        {
            if (i == 0)
            {
                biggestScore = scorePercentages[0] ;
                continue;
            }

            if (biggestScore < scorePercentages[i])
            {
                biggestScore = scorePercentages[i] ;
            }
        }

        for (int i = 0; i < numberofPlayer; i++)
        {
            ScoreBars[i].sizeDelta = Vector2.right*ScoreBars[i].sizeDelta.x + Vector2.up*Screen.height/3*scorePercentages[i]/biggestScore;
            Scores[i].text = "%" + Mathf.Round(scorePercentages[i]) ;
        }

        switch (myRank)
        {
                case 1:
                    ResultText.text = myRank + "st" ;
                    Star1.sprite = star ;
                    Star2.sprite = star ;
                    WinAS.Play();
                    break;
                case 2:
                    ResultText.text = myRank + "nd" ;
                    LoseAS.Play();
                    break;
                case 3:
                    ResultText.text = myRank + "rd" ;
                    LoseAS.Play();
                    break;
                case 4:
                    ResultText.text = myRank + "th" ;
                    LoseAS.Play();
                    break;
        }
    }
}
