using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems ;
using UnityEngine.UI;

public class GunFistButton : MonoBehaviour , IPointerDownHandler,IPointerUpHandler
{

    public CharacterController CharController
    {
        set
        {
            if (charController == null)
            {
                charController = value ;
            }
        }
    }
    
    public bool GunFistButtonDown
    {
        get { return gunFistButtonDown ; }
        set { gunFistButtonDown = value ; }
    }
    
    public bool GunFistButtonUp
    {
        get { return gunFistButtonUp ; }
        set { gunFistButtonUp = value ; }
    }
    
    private Image gunFistImage ;
    private bool gunFistButtonDown ;
    private bool gunFistButtonUp ;
    private bool singleplayer ;
    private Color semiTransparentColor ;
    private CharacterController charController ;
 
    void Start()
    {
        singleplayer = PlayerPrefs.GetString("PlayMode") == "SinglePlayer" ;
        gunFistImage = GetComponent<Image>() ;
        semiTransparentColor = gunFistImage.color ;
    }
    
    public void OnPointerDown(PointerEventData data)
    {
        gunFistImage.color = Color.white ;
        gunFistButtonDown = true ;
        if (!singleplayer)
        {
            charController.CallGunFistButtonDownPressedRpc();
        }
    }
    
    public void OnPointerUp(PointerEventData data)
    {
        gunFistImage.color = semiTransparentColor ;
        gunFistButtonUp = true ;
        if (!singleplayer)
        {
            charController.CallGunFistButtonUpPressedRpc();
        }
    }
}
