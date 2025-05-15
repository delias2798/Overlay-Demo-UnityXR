using System.Collections;
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

    // Acumulador del precio total
    private float totalPrecio = 0f;

    void Awake()
    {
        // Asegurar que el collider está en modo Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Inicializar el texto
        if (priceText != null)
            priceText.text = $"₡{totalPrecio:N0}";
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(productTag))
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        // Sumar precio y actualizar acumulador
        totalPrecio += producto.Precio;
        ActualizarTexto();
        
        Debug.Log($"[TriggerEnter] Producto: {producto.NombreProducto}, Precio: ₡{producto.Precio:N0}, Total: ₡{totalPrecio:N0}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(productTag))
            return;

        var producto = other.GetComponent<Producto>();
        if (producto == null)
            return;

        // Restar precio y actualizar acumulador
        totalPrecio -= producto.Precio;
        if (totalPrecio < 0f) totalPrecio = 0f;  // Evitar valores negativos
        ActualizarTexto();

        Debug.Log($"[TriggerExit] Salió el producto: {producto.NombreProducto}, Total: ₡{totalPrecio:N0}");
    }

    /// <summary>
    /// Actualiza el componente Text con el valor formateado en colones.
    /// </summary>
    private void ActualizarTexto()
    {
        if (priceText != null)
            priceText.text = $"₡{totalPrecio:N0}";
    }
}
