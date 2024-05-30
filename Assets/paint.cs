using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Paint : MonoBehaviour
{

    bool isPainting = false;
    public InputActionReference triggerReference = null;
    public GameObject s;
    public Transform t;


    // Start is called before the first frame update
    void Start()
    {
        triggerReference.action.started += StartPainting;
        triggerReference.action.canceled += StopPainting;

    }

    // Update is called once per frame
    void Update()
    {
        if(isPainting)
        {
            Instantiate(s, t.position, Quaternion.identity);
        }
    }


    private void StartPainting(InputAction.CallbackContext context)
    {
        isPainting = true;
    }

    private void StopPainting(InputAction.CallbackContext context)
    {
        isPainting = false;
    }


}
