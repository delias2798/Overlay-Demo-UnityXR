using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BasketTriggerDetector : MonoBehaviour
{
    [Tooltip("Tag que identifica la canasta de compras.")]
    [SerializeField] private string basketTag = "Basket";

    [Tooltip("Referencia al ExperimentManager de la escena.")]
    [SerializeField] private ExperimentManager experimentManager;

    [Tooltip("GameObject que se activará al cumplir la condición.")]
    [SerializeField] private GameObject objectToActivate;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(basketTag) ||
            experimentManager == null ||
            objectToActivate == null)
            return;

        float dinero = experimentManager.DineroAcumulado;
        if (dinero > 0f)
        {
            objectToActivate.SetActive(true);
            Debug.Log($"[BasketTrigger] Dinero ₡{dinero:N0} > 0 → Activando objeto.");
        }
        else
        {
            Debug.Log($"[BasketTrigger] Dinero ₡{dinero:N0} no cumple condición → No se activa.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(basketTag) && objectToActivate != null)
            objectToActivate.SetActive(false);
    }
}
