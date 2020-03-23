using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio ;
using UnityEngine.SceneManagement ;
using UnityEngine.UI ;

public class PauseMenu : MonoBehaviour
{
    public CameraController CamController ;
    
    [Header("Menu Objects")] 
    public GameObject PauseMenuObject ;
    public GameObject GamePlayUiObject;
    public GameObject SettingsMenuObject ;
    public GameObject BackgroundColorObject ;
    public GameObject BackgroundItemsForPauseObject ;

    [Header("Pause Menu Items")] 
    public Button ResumeButton ;
    public Button SettingsButton ;
    public Button MainMenuButton ;
    
    [Header("Settings Menu Items")]
    public Button CamIncreaseButton ;
    public Button CamDecreaseButton ;
    public Button SettingsBackButton ;
    public Text CamSettingText ;
    public Slider MusicSlider ;
    public Slider SoundFxSlider ;

    [Header("Audio")] 
    public AudioSource SoundFX ;
    public AudioSource Music ;
    public AudioMixer MyAudioMixer ;

    private MultiplayerManager MpManager ;

    void Start()
    {
        MusicSlider.value = PlayerPrefs.GetFloat("MusicSlider") ;
        SoundFxSlider.value = PlayerPrefs.GetFloat("SoundFxSlider") ;
        CamSettingText.text = PlayerPrefs.GetString("CamSetting") ;
        ResumeButton.onClick.AddListener(ResumeButtonClicked);
        SettingsButton.onClick.AddListener(SettingsMenuButtonClicked);
        MainMenuButton.onClick.AddListener(MainMenuButtonClicked);
        CamIncreaseButton.onClick.AddListener(CamIncDecButtonClicked);
        CamDecreaseButton.onClick.AddListener(CamIncDecButtonClicked);
        MusicSlider.onValueChanged.AddListener(MusicSliderValueChanged);
        SoundFxSlider.onValueChanged.AddListener(SoundFxSliderValueChanged);
        SettingsBackButton.onClick.AddListener(SettingsBackButtonClicked);
        MpManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>() ;
    }

    void ResumeButtonClicked()
    {
        SoundFX.Play();
        Music.Play();
        if (PlayerPrefs.GetString("PlayMode") == "SinglePlayer")
        {
            Time.timeScale = 1 ;
        }
        GamePlayUiObject.SetActive(true);
        BackgroundColorObject.SetActive(false);
        BackgroundItemsForPauseObject.SetActive(false);
        PauseMenuObject.SetActive(false);
    }

    void SettingsMenuButtonClicked()
    {
        SoundFX.Play();
        SettingsMenuObject.SetActive(true);
        SettingsBackButton.gameObject.SetActive(true);
        PauseMenuObject.SetActive(false);
    }

    void MainMenuButtonClicked()
    {
        SoundFX.Play();
        if (PlayerPrefs.GetString("PlayMode") == "SinglePlayer")
        {
            SceneManager.LoadScene(0) ;
        }
        else
        {
            SceneManager.LoadScene(0) ;
            MultiplayerManager.DisconnectionAttempt = true ;
            MpManager.DisconnectFromServer();
        }
    }

    void CamIncDecButtonClicked()
    {
        SoundFX.Play();
        if (CamSettingText.text == "Adaptive")
        {
            CamSettingText.text = "Focused on character" ;
            CamController.IsFocusedOnCharActive = true ;
        }
        else
        {
            CamSettingText.text = "Adaptive" ;
            CamController.IsFocusedOnCharActive = false ;
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

    void SettingsBackButtonClicked()
    {
        SoundFX.Play();
        PlayerPrefs.SetFloat("MusicSlider",MusicSlider.value);
        PlayerPrefs.SetFloat("SoundFxSlider",SoundFxSlider.value);
        PlayerPrefs.SetString("CamSetting",CamSettingText.text);
        SettingsMenuObject.SetActive(false);
        PauseMenuObject.SetActive(true);
        SettingsBackButton.gameObject.SetActive(false);
    }
}
