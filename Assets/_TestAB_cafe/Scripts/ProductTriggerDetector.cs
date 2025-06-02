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

    // Color original del Text para restaurarlo
    private Color defaultColor;

    // Conjunto de productos actualmente dentro del trigger
    private readonly HashSet<Producto> productosEnTrigger = new HashSet<Producto>();

    void Awake()
    {
        // Asegurar que el collider está en modo Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Guardar color original y mostrar valor inicial
        if (priceText != null && experimentManager != null)
        {
            defaultColor = priceText.color;
            priceText.text  = $"₡{experimentManager.DineroAcumulado:N0}";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(productTag) || experimentManager == null)
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        if (productosEnTrigger.Count == 0)
            experimentManager.OnTriggerOccupied();

        if (productosEnTrigger.Add(producto))
        {
            experimentManager.RemoveDinero(producto.Precio);
            ActualizarTexto();
            Debug.Log($"[Enter] {producto.NombreProducto}: ₡{producto.Precio:N0} gastados, Restante: ₡{experimentManager.DineroAcumulado:N0}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(productTag) || experimentManager == null)
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        if (productosEnTrigger.Remove(producto))
        {
            experimentManager.AddDinero(producto.Precio);
            ActualizarTexto();
            Debug.Log($"[Exit] {producto.NombreProducto} reembolsado, Total: ₡{experimentManager.DineroAcumulado:N0}");

            if (productosEnTrigger.Count == 0)
                experimentManager.OnTriggerEmpty();
        }
    }

    /// <summary>
    /// Refresca el componente Text con el valor actual del ExperimentManager
    /// y ajusta el color si el monto es negativo.
    /// </summary>
    private void ActualizarTexto()
    {
        if (priceText == null) return;

        float monto = experimentManager.DineroAcumulado;
        priceText.text  = $"₡{monto:N0}";
        priceText.color = (monto < 0f) ? Color.red : defaultColor;
    }
}
