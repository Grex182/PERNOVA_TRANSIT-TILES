using UnityEngine;

[RequireComponent(typeof(Collider))] // Requires a collider for raycasting
public class Outline : MonoBehaviour
{
    private int outlineLayer;
    private bool isHovered;
    private bool isSelected;

    private void Start()
    {
        outlineLayer = LayerMask.NameToLayer("Outline");
        if (outlineLayer == -1) Debug.LogError("Create an 'Outline' layer first!");
    }


    // Toggle outline for the entire hierarchy
    public void SetOutline(bool enable)
    {
        SetLayerRecursively(gameObject, enable ? outlineLayer : 0); // 0 = Default layer
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    
}