using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class UIFollowLeftHand : MonoBehaviour
{
    [Tooltip("Controlador XR para la mano izquierda.")]
    public ActionBasedController leftController;
    [Tooltip("Intervalo de actualización en segundos.")]
    public float updateInterval = 0.05f;
    [Tooltip("Offset de posición relativo al controlador.")]
    public Vector3 positionOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("Offset de rotación relativo al controlador.")]
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    void Start()
    {
        if (leftController != null)
            StartCoroutine(FollowRoutine());
    }

    private IEnumerator FollowRoutine()
    {
        var ctrlTransform = leftController.transform;
        while (leftController.gameObject.activeInHierarchy)
        {
            transform.position = ctrlTransform.TransformPoint(positionOffset);
            transform.rotation = ctrlTransform.rotation * Quaternion.Euler(rotationOffset);

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
