using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ProductTriggerDetector : MonoBehaviour
{
    [Tooltip("Tag que identifica los productos.")]
    [SerializeField] private string productTag = "Product";

    [Tooltip("Componente Text donde se mostrará el precio acumulado.")]
    [SerializeField] private Text priceText;

    [Tooltip("Referencia al ExperimentManager de la escena.")]
    [SerializeField] private ExperimentManager experimentManager;

    // Conjunto de productos actualmente dentro del trigger
    private readonly HashSet<Producto> productosEnTrigger = new HashSet<Producto>();

    void Awake()
    {
        // Asegurar que el collider está en modo Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Inicializar el texto con el valor inicial del ExperimentManager
        if (priceText != null && experimentManager != null)
            priceText.text = $"₡{experimentManager.DineroAcumulado:N0}";
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(productTag) || experimentManager == null)
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        // Transición vacío → ocupado
        if (productosEnTrigger.Count == 0)
            experimentManager.OnTriggerOccupied();

        // Agregar al conjunto y sumar dinero si es nuevo
        if (productosEnTrigger.Add(producto))
        {
            experimentManager.AddDinero(producto.Precio);
            ActualizarTexto();
            Debug.Log($"[Enter] {producto.NombreProducto}: ₡{producto.Precio:N0}, Total: ₡{experimentManager.DineroAcumulado:N0}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(productTag) || experimentManager == null)
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        // Eliminar del conjunto y restar dinero si existía
        if (productosEnTrigger.Remove(producto))
        {
            experimentManager.RemoveDinero(producto.Precio);
            ActualizarTexto();
            Debug.Log($"[Exit] {producto.NombreProducto} removido, Total: ₡{experimentManager.DineroAcumulado:N0}");

            // Transición ocupado → vacío
            if (productosEnTrigger.Count == 0)
                experimentManager.OnTriggerEmpty();
        }
    }

    /// <summary>
    /// Refresca el componente Text con el valor actual del ExperimentManager.
    /// </summary>
    private void ActualizarTexto()
    {
        if (priceText != null)
            priceText.text = $"₡{experimentManager.DineroAcumulado:N0}";
    }
}
