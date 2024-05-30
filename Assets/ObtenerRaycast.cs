using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObtenerRaycast : MonoBehaviour
{

    public XRRayInteractor rayInteractor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitpoint = new RaycastHit();
        if(rayInteractor.TryGetCurrent3DRaycastHit(out hitpoint)){
            Debug.Log("Hello: " + hitpoint.point);
            Vector3 local = hitpoint.transform.InverseTransformPoint(hitpoint.point);
            Debug.Log("Local: " + local);
          
        }
    }

    public void MiMetodo(){
        
    }

}
