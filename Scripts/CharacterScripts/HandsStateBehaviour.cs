using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsStateBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator , AnimatorStateInfo animatorStateInfo , int layerIndex)
    {
        // Deactivate the hands after using guns.
        animator.gameObject.SetActive(false);
    }
}
