using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBlades : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    
    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, speed), Space.Self);
    }
}
