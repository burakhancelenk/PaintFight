using System.Collections;
using System.Collections.Generic;
using Photon.Pun ;
using Photon.Pun.UtilityScripts ;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Audio ;
using UnityEngine.SceneManagement ;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Items")]
    public Button SingleplayerButton ;
    public Button MultiplayerButton ;
    public Button SettingsButton ;
    public Button HowToPlayButton ;
    
    [Header("Singleplayer Menu Items")]
    public Button SingleplayerPlayButton ;
    public Button SingleplayerBackButton ;
    public Button SingleNopIncreaseButton ;
    public Button SingleNopDecreaseButton ;
    public Text SingleNopText ;
    public Button SingleDurIncreaseButton ;
    public Button SingleDurDecreaseButton ;
    public Text SingleDurText ;
    public Button CharColorIncreaseButton ;
    public Button CharColorDecreaseButton ;
    public Text CharColorText ;

    [Header("Multiplayer Menu Objects")] 
    public GameObject ChooseMPGameTypeMenu ;
    public GameObject QuickMatchMenu ;
    public GameObject PlayWithFriendsMenu ;
    public GameObject CreateRoomMenu ;
    public GameObject JoinRoomMenu ;
    public GameObject InfoPanel ;

    [Header("Multiplayer Game Type Menu Items")]
    public Button QuickMatchButton ;
    public Button PlayWithFriendsButton ;
    public Button GameTypeBackButton ;
    
    [Header("Multiplayer QuickMatch Menu Items")]
    public Button QuickMatchPlayButton ;
    public Button QuickMatchBackButton ;
    public Button QuickMatchNopIncreaseButton ;
    public Button QuickMatchNopDecreaseButton ;
    public Text QuickMatchNopText ;
    public Button QuickMatchDurIncreaseButton ;
    public Button QuickMatchDurDecreaseButton ;
    public Text QuickMatchDurText ;

    [Header("Multiplayer PlayWithFriends Menu Items")]
    public Button CreateRoomButton ;
    public Button JoinRoomButton ;
    public Button PlayWithFriendsBackButton ;

    [Header("Multiplayer Create Room Menu Items")]
    public InputField CreateMenuRoomNameInputField ;
    public GameObject CreateMenuWarningText ;
    public Text CreateMenuNopText ;
    public Button CreateMenuNopIncreaseButton ;
    public Button CreateMenuNopDecreaseButton ;
    public Text CreateMenuDurText ;
    public Button CreateMenuDurIncreaseButton ;
    public Button CreateMenuDurDecreaseButton ;
    public Button CreateMenuPlayButton ;
    public Button CreateMenuBackButton ;

    [Header("Multiplayer Join Room Menu Items")]
    public InputField JoinMenuRoomNameInputField ;
    public GameObject JoinMenuWarningText ;
    public Button JoinMenuBackButton ;
    public Button JoinMenuPlayButton;

    [Header("Multiplayer Info Panel Items")]
    public GameObject ConnectingToServerText ;
    public GameObject SearchingForPlayersText ;
    public GameObject GameLoadingQMText ;
    public GameObject JoinedRoomText ;
    public GameObject RoomCreatedText ;
    public GameObject GameLoadingFMText ;
    public GameObject DisconnectedFromMasterText ;
    public Button ReconnectButton ;
    public Button InfoPanelBackButton ;
    
    
    [Header("Settings Menu Items")]
    public Button SettingsBackButton ;
    public Button SettingsApplyButton ;
    public Button CamIncreaseButton ;             
    public Button CamDecreaseButton ;
    public Button RestartInfoButton ;
    public Text CamSettingText ;
    public Button GraphicsIncreaseButton ;        
    public Button GraphicsDecreaseButton ;        
    public Text GraphicsSettingText ;
    public Slider MusicSlider ;                   
    public Slider SoundFxSlider ;                 
    
    [Header("HowToPlay Menu Items")]
    public Button HowToPlayNextButton ;
    public Button HowToPlayPreviousButton ;
    public Button HowToPlayBackButton ;
    public Image TutorialImage ;
    public Text InfoText ;
    public Sprite[] TutorialSprites ;
    [TextArea]
    public string[] TutorialInfos ;

    [Header("Menu Objects")]
    public GameObject MainMenuObject ;
    public GameObject SingleplayerObject ;
    public GameObject MultiplayerObject ;
    public GameObject SettingsObject ;
    public GameObject RestartInfoObject ;
    public GameObject HowToPlayObject ;

    [Header("Audio")] 
    public AudioSource MenuSoundFXAS ;
    public AudioSource MPGameFoundSoundFxAs ;
    public AudioMixer MyAudioMixer ;

    private int graphicsSettingIndex ;
    private int howToPlayTutorialIndex ;
    private int charColorIndex ;

    private GameObject LastOpenedMenuObject ;

    [Space] public MultiplayerManager MPManager ;

    private void Start()
    {
        MultiplayerManager.CurrentStatus = MultiplayerManager.GameStatus.InMenu ;
        if (PlayerPrefs.HasKey("GraphicsSettingIndex"))
        {
            graphicsSettingIndex = PlayerPrefs.GetInt("GraphicsSettingIndex") ;
            QualitySettings.SetQualityLevel(graphicsSettingIndex,true);
        }
        else
        {
            graphicsSettingIndex = 1 ;
        }

        switch (graphicsSettingIndex)
        {
            case 0:
                GraphicsSettingText.text = "Low" ;
                break;
            case 1:
                GraphicsSettingText.text = "Medium" ;
                break;
            case 2:
                GraphicsSettingText.text = "High" ;
                break;
        }

        if (PlayerPrefs.HasKey("CamSetting"))
        {
            CamSettingText.text = PlayerPrefs.GetString("CamSetting") ;
        }
        else
        {
            CamSettingText.text = "Adaptive" ;
        }

        MusicSlider.value = PlayerPrefs.GetFloat("MusicSlider") ;
        SoundFxSlider.value = PlayerPrefs.GetFloat("SoundFxSlider") ;
        MyAudioMixer.SetFloat("MusicVolume" , (MusicSlider.value - 100) * 80 / 100) ;
        MyAudioMixer.SetFloat("SoundFXVolume" , (SoundFxSlider.value - 100) * 80 / 100) ;
        
        SingleplayerButton.onClick.AddListener(SingleplayerButtonClicked);
        MultiplayerButton.onClick.AddListener(MultiplayerButtonClicked);
        SettingsButton.onClick.AddListener(SettingsButtonClicked);
        HowToPlayButton.onClick.AddListener(HowToPlayButtonClicked);
        QuickMatchButton.onClick.AddListener(QuickMatchButtonClicked);
        PlayWithFriendsButton.onClick.AddListener(PlayWithFriendsButtonClicked);
        QuickMatchPlayButton.onClick.AddListener(QuickMatchPlayButtonClicked);
        CreateRoomButton.onClick.AddListener(CreateRoomButtonClicked);
        JoinRoomButton.onClick.AddListener(JoinRoomButtonClicked);
        CreateMenuPlayButton.onClick.AddListener(CreateMenuPlayButtonClicked);
        JoinMenuPlayButton.onClick.AddListener(JoinMenuPlayButtonClicked);
        ReconnectButton.onClick.AddListener(ReconnectButtonClicked);
        SingleplayerBackButton.onClick.AddListener(delegate
        {
            BackButton(SingleplayerObject , MainMenuObject);
        });
        GameTypeBackButton.onClick.AddListener(delegate
        {
            BackButton(MultiplayerObject,MainMenuObject);
            MultiplayerManager.DisconnectionAttempt = true ;
            MultiplayerManager.ConnectedToMaster = false ;
            MPManager.DisconnectFromServer();
        });
        QuickMatchBackButton.onClick.AddListener(delegate
        {
            BackButton(QuickMatchMenu,ChooseMPGameTypeMenu);
        });
        PlayWithFriendsBackButton.onClick.AddListener(delegate
        {
            BackButton(PlayWithFriendsMenu,ChooseMPGameTypeMenu);
        });
        CreateMenuBackButton.onClick.AddListener(delegate
        {
            BackButton(CreateRoomMenu,PlayWithFriendsMenu);
        });
        JoinMenuBackButton.onClick.AddListener(delegate
        {
            BackButton(JoinRoomMenu,PlayWithFriendsMenu);
        });
        SettingsBackButton.onClick.AddListener(delegate
        {
            BackButton(SettingsObject,MainMenuObject);
        });
        HowToPlayBackButton.onClick.AddListener(delegate
        {
            BackButton(HowToPlayObject,MainMenuObject);
        });
        SingleNopIncreaseButton.onClick.AddListener(delegate
        {
            SingleNopIncDecButtonClicked(true);
        });
        SingleNopDecreaseButton.onClick.AddListener(delegate
        {
            SingleNopIncDecButtonClicked(false);
        });
        QuickMatchNopIncreaseButton.onClick.AddListener(delegate
        {
            QuickMatchNopIncDecButtonClicked(true);
        });
        QuickMatchNopDecreaseButton.onClick.AddListener(delegate
        {
            QuickMatchNopIncDecButtonClicked(false);
        });
        CreateMenuNopIncreaseButton.onClick.AddListener(delegate
        {
            CreateMenuNopIncDecButtonClicked(true);
        });
        CreateMenuNopDecreaseButton.onClick.AddListener(delegate
        {
            CreateMenuNopIncDecButtonClicked(false);
        });
        QuickMatchDurIncreaseButton.onClick.AddListener(delegate
        {
            QuickMatchDurIncDecButtonClicked(true);
        });
        QuickMatchDurDecreaseButton.onClick.AddListener(delegate
        {
            QuickMatchDurIncDecButtonClicked(false);
        });
        CreateMenuDurIncreaseButton.onClick.AddListener(delegate
        {
            CreateMenuDurIncDecButtonClicked(true);
        });
        CreateMenuDurDecreaseButton.onClick.AddListener(delegate
        {
            CreateMenuDurIncDecButtonClicked(false);
        });
        SingleDurIncreaseButton.onClick.AddListener(delegate
        {
            SingleDurIncDecButtonClicked(true);
        });
        SingleDurDecreaseButton.onClick.AddListener(delegate
        {
            SingleDurIncDecButtonClicked(false);
        });
        GraphicsIncreaseButton.onClick.AddListener(delegate
        {
            GraphicsSettingChangeButtonClicked(true);
        });
        GraphicsDecreaseButton.onClick.AddListener(delegate
        {
            GraphicsSettingChangeButtonClicked(false);
        });
        HowToPlayNextButton.onClick.AddListener(delegate
        {
            HowToPlayChangeSpriteButtonClicked(true);
        });
        HowToPlayPreviousButton.onClick.AddListener(delegate
        {
            HowToPlayChangeSpriteButtonClicked(false);
        });
        CharColorIncreaseButton.onClick.AddListener(delegate
        {
            ChangeColorOfCharacter(true);
        });
        CharColorDecreaseButton.onClick.AddListener(delegate
        {
            ChangeColorOfCharacter(false);
        });
        CamIncreaseButton.onClick.AddListener(CamSettingChangeButtonClicked);
        CamDecreaseButton.onClick.AddListener(CamSettingChangeButtonClicked);
        MusicSlider.onValueChanged.AddListener(MusicSliderValueChanged);
        SoundFxSlider.onValueChanged.AddListener(SoundFxSliderValueChanged);
        SettingsApplyButton.onClick.AddListener(SettingsApplyButtonClicked);
        SingleplayerPlayButton.onClick.AddListener(SingleplayerPlayButtonClicked);
        RestartInfoButton.onClick.AddListener(RestartInfoButtonClicked);
    }

    void SingleplayerButtonClicked()
    {
        MenuSoundFXAS.Play();
        SingleplayerObject.SetActive(true);
        MainMenuObject.SetActive(false);
    }
    
    void MultiplayerButtonClicked()
    {
        MenuSoundFXAS.Play();
        PlayerPrefs.SetString("PlayMode","MultiPlayer");
        MPManager.ConnectToServer();
        MultiplayerObject.SetActive(true);
        ChooseMPGameTypeMenu.SetActive(false);
        InfoPanel.SetActive(true);
        MainMenuObject.SetActive(false);
        ConnectingToServerText.SetActive(true);
        InfoPanelBackButton.gameObject.SetActive(true);
        InfoPanelBackButton.onClick.RemoveAllListeners();
        InfoPanelBackButton.onClick.AddListener(delegate
        {
            InfoPanel.SetActive(false);
            ConnectingToServerText.SetActive(false);
            MainMenuObject.SetActive(true);
            MultiplayerObject.SetActive(false);
            InfoPanelBackButton.gameObject.SetActive(false);
            MultiplayerManager.DisconnectionAttempt = true ;
            MultiplayerManager.ConnectedToMaster = false ;
            MPManager.DisconnectFromServer();
        });
    }

    public void ConnectionToMasterEstablishedInMenu()
    {
        ChooseMPGameTypeMenu.SetActive(true);
        ConnectingToServerText.SetActive(false);
        InfoPanel.SetActive(false);
        ReconnectButton.gameObject.SetActive(false);
    }

    public void DisconnectedFromMasterInMenu()
    {
        InfoPanel.SetActive(true);
        InfoPanelBackButton.gameObject.SetActive(true);
        ConnectingToServerText.SetActive(false);
        SearchingForPlayersText.SetActive(false);
        GameLoadingQMText.SetActive(false);
        GameLoadingFMText.SetActive(false);
        JoinedRoomText.SetActive(false);
        RoomCreatedText.SetActive(false);
        DisconnectedFromMasterText.SetActive(true);
        ReconnectButton.gameObject.SetActive(true);
        InfoPanelBackButton.onClick.AddListener(delegate
        {
            DisconnectedFromMasterText.SetActive(false);
            LastOpenedMenuObject.SetActive(false);
            MultiplayerObject.SetActive(false);
            InfoPanelBackButton.gameObject.SetActive(false);
            InfoPanel.SetActive(false);
            MainMenuObject.SetActive(true);
        });
    }
    
    void SettingsButtonClicked()
    {
        MenuSoundFXAS.Play();
        SettingsObject.SetActive(true);
        MainMenuObject.SetActive(false);
    }
    
    void HowToPlayButtonClicked()
    {
        MenuSoundFXAS.Play();
        HowToPlayObject.SetActive(true);
        MainMenuObject.SetActive(false);
        howToPlayTutorialIndex = 0 ;
        TutorialImage.sprite = TutorialSprites[howToPlayTutorialIndex] ;
        InfoText.text = TutorialInfos[howToPlayTutorialIndex] ;
    }

    void QuickMatchButtonClicked()
    {
        MenuSoundFXAS.Play();
        ChooseMPGameTypeMenu.SetActive(false);
        QuickMatchMenu.SetActive(true);
    }

    void PlayWithFriendsButtonClicked()
    {
        MenuSoundFXAS.Play();
        ChooseMPGameTypeMenu.SetActive(false);
        PlayWithFriendsMenu.SetActive(true);
    }

    void QuickMatchPlayButtonClicked()
    {
        MPManager.RequestQuickMatch(byte.Parse(QuickMatchNopText.text),byte.Parse(QuickMatchDurText.text));
        QuickMatchMenu.SetActive(false);
        InfoPanel.SetActive(true);
        SearchingForPlayersText.SetActive(true);
        InfoPanelBackButton.gameObject.SetActive(true);
        InfoPanelBackButton.onClick.RemoveAllListeners();
        InfoPanelBackButton.onClick.AddListener(delegate
        {
            LastOpenedMenuObject = QuickMatchMenu ;
            InfoPanel.SetActive(false);
            SearchingForPlayersText.SetActive(false);
            InfoPanelBackButton.gameObject.SetActive(false);
            MultiplayerManager.DisconnectionAttempt = true ;
            MultiplayerManager.DisconnectForReconnection = true ;
            MPManager.DisconnectFromServer();
        });
    }

    public void QMGameFound()
    {
        MPGameFoundSoundFxAs.Play();
        PlayerPrefs.SetInt("NOP",int.Parse(QuickMatchNopText.text));
        PlayerPrefs.SetInt("Duration",int.Parse(QuickMatchDurText.text));
        InfoPanelBackButton.gameObject.SetActive(false);
        SearchingForPlayersText.SetActive(false);
        GameLoadingQMText.SetActive(true);
        Invoke(nameof(LoadMPGameScene),1.5f);
    }

    void LoadMPGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true ;
            PhotonNetwork.LoadLevel(1);
        }
    }

    void CreateRoomButtonClicked()
    {
        PlayWithFriendsMenu.SetActive(false);
        CreateRoomMenu.SetActive(true);
    }

    void JoinRoomButtonClicked()
    {
        PlayWithFriendsMenu.SetActive(false);
        JoinRoomMenu.SetActive(true);
    }

    void CreateMenuPlayButtonClicked()
    {
        MPManager.CreateRoomForFriends(CreateMenuRoomNameInputField.text,byte.Parse(CreateMenuNopText.text)
            ,byte.Parse(CreateMenuDurText.text));
        CreateRoomMenu.SetActive(false);
        MultiplayerManager.RoomCreatedByMe = true ;
        PlayerPrefs.SetInt("NOP",int.Parse(CreateMenuNopText.text));
        PlayerPrefs.SetInt("Duration",int.Parse(CreateMenuDurText.text));
        InfoPanelBackButton.onClick.RemoveAllListeners();
        InfoPanelBackButton.onClick.AddListener(delegate
        {
            LastOpenedMenuObject = CreateRoomMenu ;
            MultiplayerManager.DisconnectForReconnection = true ;
            MultiplayerManager.DisconnectionAttempt = true ;
            MPManager.DisconnectFromServer();
            InfoPanel.SetActive(false);
            RoomCreatedText.SetActive(false);
            CreateMenuWarningText.SetActive(false);
            InfoPanelBackButton.gameObject.SetActive(false);
        });
    }

    public void RoomCreated()
    {
        InfoPanel.SetActive(true);
        InfoPanelBackButton.gameObject.SetActive(true);
        RoomCreatedText.SetActive(true);
        
    }

    public void RoomCreationFailed()
    {
        CreateRoomMenu.SetActive(true);
        CreateMenuWarningText.SetActive(true);
    }

    public void FMGameFound()
    {
        MPGameFoundSoundFxAs.Play();
        RoomCreatedText.SetActive(false);
        JoinedRoomText.SetActive(false);
        GameLoadingFMText.SetActive(true);
        int[] roomInfs = MPManager.TakeRoomInformations() ;
        PlayerPrefs.SetInt("Duration",roomInfs[0]) ;
        PlayerPrefs.SetInt("NOP",roomInfs[1]);
        Invoke(nameof(LoadMPGameScene),1.5f);
    }

    void JoinMenuPlayButtonClicked()
    {
        JoinRoomMenu.SetActive(false);
        JoinMenuWarningText.SetActive(false);
        MPManager.JoinFriendsRoom(JoinMenuRoomNameInputField.text);
        InfoPanelBackButton.onClick.RemoveAllListeners();
        InfoPanelBackButton.onClick.AddListener(delegate
        {
            LastOpenedMenuObject = JoinRoomMenu ;
            MultiplayerManager.DisconnectionAttempt = true ;
            MultiplayerManager.DisconnectForReconnection = true ;
            MPManager.DisconnectFromServer();
            InfoPanel.SetActive(false);
            JoinedRoomText.SetActive(false);
            InfoPanelBackButton.gameObject.SetActive(false);
        });
    }

    public void JoinedRoom()
    {
        InfoPanel.SetActive(true);
        InfoPanelBackButton.gameObject.SetActive(true);
        JoinedRoomText.SetActive(true);
    }

    public void JoinRoomFailed()
    {
        JoinRoomMenu.SetActive(true);
        JoinMenuWarningText.SetActive(true);
    }

    void ReconnectButtonClicked()
    {
        MultiplayerManager.ConnectedToMaster = true ;
        MPManager.ConnectToServer();
        DisconnectedFromMasterText.SetActive(false);
        ConnectingToServerText.SetActive(true);
        ReconnectButton.gameObject.SetActive(false);
    }

    public void OpenLastOpenedMenuObject()
    {
        InfoPanel.SetActive(false);
        ConnectingToServerText.SetActive(false);
        LastOpenedMenuObject.SetActive(true);
    }

    void BackButton(GameObject from , GameObject to)
    {
        MenuSoundFXAS.Play();
        to.SetActive(true);
        from.SetActive(false);
    }

    void SingleNopIncDecButtonClicked(bool increse)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(SingleNopText.text) ;
        if (increse)
        {
            temp++ ;
            if (temp > 4)
            {
                temp = 4 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }
        charColorIndex = 0 ;
        CharColorText.text = "Red" ;
        SingleNopText.text = temp+"" ;
    }
    
    void QuickMatchNopIncDecButtonClicked(bool increse)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(QuickMatchNopText.text) ;
        if (increse)
        {
            temp++ ;
            if (temp > 4)
            {
                temp = 4 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }
        QuickMatchNopText.text = temp+"" ;
    }
    
    void CreateMenuNopIncDecButtonClicked(bool increse)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(CreateMenuNopText.text) ;
        if (increse)
        {
            temp++ ;
            if (temp > 4)
            {
                temp = 4 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }
        CreateMenuNopText.text = temp+"" ;
    }

    void SingleDurIncDecButtonClicked(bool increase)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(SingleDurText.text) ;
        if (increase)
        {
            temp++ ;
            if (temp > 10)
            {
                temp = 10 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }
        SingleDurText.text = temp+"" ;
    }
    
    void QuickMatchDurIncDecButtonClicked(bool increase)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(QuickMatchDurText.text) ;
        if (increase)
        {
            temp++ ;
            if (temp > 10)
            {
                temp = 10 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }

        QuickMatchDurText.text = temp+"" ;
    }
    
    void CreateMenuDurIncDecButtonClicked(bool increase)
    {
        MenuSoundFXAS.Play();
        int temp = int.Parse(CreateMenuDurText.text) ;
        if (increase)
        {
            temp++ ;
            if (temp > 10)
            {
                temp = 10 ;
            }
        }
        else
        {
            temp-- ;
            if (temp < 2)
            {
                temp = 2 ;
            }
        }

        CreateMenuDurText.text = temp+"" ;
    }

    void CamSettingChangeButtonClicked()
    {
        MenuSoundFXAS.Play();
        if (CamSettingText.text == "Adaptive")
        {
            CamSettingText.text = "Focused on character" ;
        }
        else
        {
            CamSettingText.text = "Adaptive" ;
        }
    }

    void GraphicsSettingChangeButtonClicked(bool next)
    {
        MenuSoundFXAS.Play();
        if (next)
        {
            graphicsSettingIndex++ ;
            if (graphicsSettingIndex > 2)
            {
                graphicsSettingIndex = 0 ;
            }
        }
        else
        {
            graphicsSettingIndex-- ;
            if (graphicsSettingIndex < 0)
            {
                graphicsSettingIndex = 2 ;
            }
        }
        
        switch (graphicsSettingIndex)
        {
            case 0:
                GraphicsSettingText.text = "Low" ;
                break;
            case 1:
                GraphicsSettingText.text = "Medium" ;
                break;
            case 2:
                GraphicsSettingText.text = "High" ;
                break;
        }
    }

    void MusicSliderValueChanged(float value)
    {
        MyAudioMixer.SetFloat("MusicVolume" , (value - 100) * 80 / 100) ;
    }
    
    void SoundFxSliderValueChanged(float value)
    {
        MyAudioMixer.SetFloat("SoundFXVolume" , (value - 100) * 80 / 100) ;
    }

    void HowToPlayChangeSpriteButtonClicked(bool next)
    {
        MenuSoundFXAS.Play();
        if (next)
        {
            howToPlayTutorialIndex++ ;
            if (howToPlayTutorialIndex >= TutorialSprites.Length)
            {
                howToPlayTutorialIndex = TutorialSprites.Length - 1 ;
            }
        }
        else
        {
            howToPlayTutorialIndex-- ;
            if (howToPlayTutorialIndex < 0)
            {
                howToPlayTutorialIndex = 0 ;
            }
        }
        TutorialImage.sprite = TutorialSprites[howToPlayTutorialIndex] ;
        InfoText.text = TutorialInfos[howToPlayTutorialIndex] ;
    }

    void SettingsApplyButtonClicked()
    {
        MenuSoundFXAS.Play();
        if (PlayerPrefs.GetInt("GraphicsSettingIndex") != graphicsSettingIndex)
        {
            RestartInfoObject.SetActive(true);
        }
        else
        {
            MainMenuObject.SetActive(true) ;
            SettingsObject.SetActive(false) ;
        }
        PlayerPrefs.SetInt("GraphicsSettingIndex",graphicsSettingIndex);
        PlayerPrefs.SetString("CamSetting",CamSettingText.text);
        PlayerPrefs.SetFloat("MusicSlider",MusicSlider.value);
        PlayerPrefs.SetFloat("SoundFxSlider",SoundFxSlider.value);
    }

    void RestartInfoButtonClicked()
    {
        MainMenuObject.SetActive(true);
        RestartInfoObject.SetActive(false);
        SettingsObject.SetActive(false);
    }

    void ChangeColorOfCharacter(bool increase)
    {
        MenuSoundFXAS.Play();
        if (increase)
        {
            charColorIndex++ ;
            if (charColorIndex > int.Parse(SingleNopText.text)-1)
            {
                charColorIndex = 0 ;
            }
        }
        else
        {
            charColorIndex-- ;
            if (charColorIndex < 0)
            {
                charColorIndex = int.Parse(SingleNopText.text)-1 ;
            }
        }

        switch (charColorIndex)
        {
            case 0:
                CharColorText.text = "Red" ;
                break;
            case 1:
                CharColorText.text = "Yellow" ;
                break;
            case 2:
                CharColorText.text = "Green" ;
                break;
            case 3:
                CharColorText.text = "Blue" ;
                break;
        }
    }

    void SingleplayerPlayButtonClicked()
    {
        MenuSoundFXAS.Play();
        PlayerPrefs.SetInt("NOP",int.Parse(SingleNopText.text));
        PlayerPrefs.SetInt("Duration",int.Parse(SingleDurText.text));
        PlayerPrefs.SetInt("CharColor",charColorIndex);
        PlayerPrefs.SetString("PlayMode","SinglePlayer");
        SceneManager.LoadScene(1) ;
    }
}
