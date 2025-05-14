using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProductHighlightTrigger : MonoBehaviour
{
    [Tooltip("Nombre del layer que activará el cambio de color.")]
     [SerializeField] string targetTagName = "Product";

    int targetLayer;

    Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTagName))
            return;

        var rend = other.GetComponent<Renderer>();
        if (rend == null)
            return;

        if (!originalColors.ContainsKey(rend))
        {
            var mats = rend.materials;
            var colors = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
                colors[i] = mats[i].color;

            originalColors[rend] = colors;
        }

        foreach (var mat in rend.materials)
            mat.color = Color.blue;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != targetLayer)
            return;

        var rend = other.GetComponent<Renderer>();
        if (rend == null || !originalColors.TryGetValue(rend, out var colors))
            return;

        var mats = rend.materials;
        int count = Mathf.Min(mats.Length, colors.Length);
        for (int i = 0; i < count; i++)
            mats[i].color = colors[i];

        originalColors.Remove(rend);
    }
}
