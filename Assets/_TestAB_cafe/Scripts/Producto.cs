using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Producto : MonoBehaviour
{
    [Header("Configuración del producto")]
    [Tooltip("Nombre descriptivo. Si no se asigna, se usa el GameObject.name.")]
    [SerializeField] private string nombreProducto;

    [Tooltip("Precio del producto en colones.")]
    [SerializeField] private float precio;

    [Header("Interfaz de usuario")]
    [Tooltip("Componente Text donde se mostrará el precio.")]
    [SerializeField] private Text textPrecio;

    /// <summary>
    /// Nombre efectivo del producto.
    /// </summary>
    public string NombreProducto { get; private set; }

    /// <summary>
    /// Precio efectivo del producto.
    /// </summary>
    public float Precio => precio;

    void Awake()
    {
        NombreProducto = string.IsNullOrWhiteSpace(nombreProducto)
            ? gameObject.name
            : nombreProducto;

        if (textPrecio != null)
            ActualizarUIPrecio();
        else
            Debug.LogWarning($"[Producto] Falta asignar Text en '{NombreProducto}'.", this);
    }

    /// <summary>
    /// Actualiza el texto con el símbolo de colones y el valor formateado.
    /// </summary>
    public void ActualizarUIPrecio()
    {
        // Formato: ₡1 000
        textPrecio.text = $"₡{Precio:0}";
    }
}
