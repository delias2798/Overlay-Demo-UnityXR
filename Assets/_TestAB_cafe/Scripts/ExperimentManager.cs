using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [Tooltip("Dinero inicial del experimento.")]
    [SerializeField] private float dineroInicial = 15000f;

    // Dinero acumulado durante la sesión
    private float dineroAcumulado;

    void Awake()
    {
        dineroAcumulado = dineroInicial;
        Debug.Log($"[ExperimentManager] Estado iniciado. Dinero: ₡{dineroAcumulado:N0}");
    }

    /// <summary>
    /// Se llama la primera vez que al menos un producto entra en el trigger.
    /// </summary>
    public void OnTriggerOccupied()
    {
        Debug.Log("[ExperimentManager] Área ocupada por al menos un producto.");
        // Lógica adicional (p. ej., activar UI, iniciar cronómetro, etc.)
    }

    /// <summary>
    /// Se llama cuando el último producto sale del trigger.
    /// </summary>
    public void OnTriggerEmpty()
    {
        Debug.Log("[ExperimentManager] Área libre de productos.");
        // Lógica adicional (p. ej., pausar UI, registrar datos, etc.)
    }

    /// <summary>
    /// Agrega el precio de un producto al total acumulado.
    /// </summary>
    public void AddDinero(float monto)
    {
        dineroAcumulado += monto;
        Debug.Log($"[ExperimentManager] ₡{monto:N0} agregados. Total: ₡{dineroAcumulado:N0}");
    }

    /// <summary>
    /// Resta el precio de un producto del total acumulado.
    /// </summary>
    public void RemoveDinero(float monto)
    {
        dineroAcumulado -= monto;
        Debug.Log($"[ExperimentManager] ₡{monto:N0} removidos. Total: ₡{dineroAcumulado:N0}");
    }

    /// <summary>
    /// Propiedad de solo lectura para obtener el dinero acumulado.
    /// </summary>
    public float DineroAcumulado => dineroAcumulado;
}
