using System ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio ;
using UnityEngine.EventSystems ;
using UnityEngine.SceneManagement ;
using  UnityEngine.UI;

public class GamePlayUI : MonoBehaviour
{
    public Text RemainTimeText ;
    public Button PauseButton ;
    public Sprite FistSprite ;
    public Sprite CrosshairSprite ;
    public Image GunFistImage ;
    public Button MainMenuButton ;

    public GameObject BackgroundColorObject ;
    public GameObject BackgroundItemsForPauseObject ;
    public GameObject PauseMenuObject ;
    public GameObject GamePlayUiObject ;
    public GameObject DisconnectionPanel ;

    public AudioSource SoundFX ;
    public AudioSource Music ;

    private Vector2 joystickFirstPos ;
    private GameManager gm ;
    private MultiplayerManager MpManager ;

    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>() ;
        MpManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>() ;
        PauseButton.onClick.AddListener(PauseButtonClicked);
        MainMenuButton.onClick.AddListener(delegate
        {
            SoundFX.Play();
            MultiplayerManager.DisconnectionAttempt = true ;
            MpManager.DisconnectFromServer();
            SceneManager.LoadScene(0) ;
        });
    }

    void Update()
    {
        if (gm.RemainTime % 60 < 10)
        {
            RemainTimeText.text = (int)gm.RemainTime / 60 + ":0" + (int)gm.RemainTime % 60 ;
        }
        else
        {
            RemainTimeText.text = (int)gm.RemainTime / 60 + ":" + (int)gm.RemainTime % 60 ; 
        }
    }

    void PauseButtonClicked()
    {
        SoundFX.Play();
        Music.Pause();
        if (PlayerPrefs.GetString("PlayMode") == "SinglePlayer")
        {
            Time.timeScale = 0 ;
        }
        PauseMenuObject.SetActive(true);
        BackgroundColorObject.SetActive(true);
        BackgroundItemsForPauseObject.SetActive(true);
        GamePlayUiObject.SetActive(false);
    }

    public void ChangeGunFistSprite(bool isFist)
    {
        if (isFist)
        {
            GunFistImage.sprite = FistSprite ;
        }
        else
        {
            GunFistImage.sprite = CrosshairSprite ;
        }
    }

    public void OpenCloseDisconnectionPanel(bool open)
    {
        if (open)
        {
            DisconnectionPanel.SetActive(true);
        }
        else
        {
            DisconnectionPanel.SetActive(false);
        }
    }
}
