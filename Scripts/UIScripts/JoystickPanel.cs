using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

public class JoystickPanel : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler
{
    public Vector3 MoveDirection
    {
        get { return moveDirection ; }
    }
    
    public RectTransform JoyStick ;
    public Image JoystickBackImage ;
    public Image JoystickImage ;
    
    private Vector3 moveDirection ;
    private Vector3 firstPos ;
    private float radious ;
    private Color semiTrasparentColor ;

    private void Start()
    {
        radious = 130f * Screen.width / 1920 ;
        firstPos = JoyStick.transform.position ;
        semiTrasparentColor = JoystickBackImage.color ;
    }

    public void OnPointerDown(PointerEventData data)
    {
        JoystickImage.color = Color.white;
        JoystickBackImage.color = Color.white ;
        moveDirection = new Vector3(data.position.x , data.position.y , 0) - firstPos;
        if (Mathf.Pow(radious,2) < moveDirection.sqrMagnitude)
        {
            JoyStick.transform.position = firstPos + Vector3.ClampMagnitude(moveDirection,radious) ;
            moveDirection = Vector3.ClampMagnitude(moveDirection , radious) ;
        }
        else
        {
            JoyStick.transform.position =firstPos + moveDirection ;
        }
    }
    
    public void OnPointerUp(PointerEventData data)
    {
        JoystickBackImage.color = semiTrasparentColor ;
        JoystickImage.color = semiTrasparentColor ;
        moveDirection = Vector3.zero;
        JoyStick.transform.position = firstPos ;
    }

    public void OnDrag(PointerEventData data)
    {
        moveDirection = new Vector3(data.position.x , data.position.y , 0) - firstPos;
        if (Mathf.Pow(radious,2) < moveDirection.sqrMagnitude)
        {
            JoyStick.transform.position =firstPos + Vector3.ClampMagnitude(moveDirection,radious) ;
            moveDirection = Vector3.ClampMagnitude(moveDirection , radious) ;
        }
        else
        {
            JoyStick.transform.position =firstPos + moveDirection ;
        }
    }
}
