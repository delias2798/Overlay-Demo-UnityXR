using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManateeAnimations : MonoBehaviour
{

    public InputActionReference triggerReference = null;
    public Animator animator = null;


    // Start is called before the first frame update
    void Start()
    {
        triggerReference.action.performed += StartNoAnimation;
    }

    // Update is called once per frame
    void Update()
    {
 
    }


    public void TriggerPointAnimation()
    {
        animator.SetTrigger("PointTrigger");
    }

    public void TriggerYesAnimation()
    {
        animator.SetTrigger("YesTrigger");
    }

    public void TriggerNoAnimation()
    {
        animator.SetTrigger("NoTrigger");
    }

    private void StartPointAnimation(InputAction.CallbackContext context)
    {
        TriggerPointAnimation();
    }


    private void StartYesAnimation(InputAction.CallbackContext context)
    {
        TriggerYesAnimation();
    }

    private void StartNoAnimation(InputAction.CallbackContext context)
    {
        TriggerNoAnimation();
    }

}
