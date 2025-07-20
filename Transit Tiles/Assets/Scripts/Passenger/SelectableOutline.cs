using UnityEngine;

[RequireComponent(typeof(Collider))] // Requires a collider for raycasting
public class SelectableOutline : MonoBehaviour
{
    public static bool hasSelected;
    private int outlineLayer;
    private bool isHovered;

    private void Start()
    {
        outlineLayer = LayerMask.NameToLayer("Outline");
        if (outlineLayer == -1) Debug.LogError("Create an 'Outline' layer first!");
    }

    private void OnMouseEnter()
    {
        if (UiManager.Instance.isPaused || LevelManager.Instance.currState == MovementState.Card) return;
        if (gameObject.CompareTag("Drag") && !hasSelected) // Only apply outline if the object has the "Drag" tag
        {
            if (gameObject.GetComponent<PassengerData>().currTile == TileTypes.Station && LevelManager.Instance.currState != MovementState.Station) return;
                
            SetOutline(true);
        }
    }

    private void OnMouseExit()
    {
        if (gameObject.CompareTag("Drag") && !hasSelected) // Only apply outline if the object has the "Drag" tag
        {
            SetOutline(false); // Hover ended
        }
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

    public bool SetHasSelected(bool value)
    {
        hasSelected = value;
        return hasSelected;
    }
}