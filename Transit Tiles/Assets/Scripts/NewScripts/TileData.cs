using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileTypes
{
    Train,
    Seat,
    Station,
    Wall
}

public class TileData : MonoBehaviour
{
    [Header("Tile Setting")]
    public TileTypes tileType;
    public bool isBottomSection;
    public bool canSpawnHere; // NOTE: Not fixed value, can be set in a for loop (?)

    [Header("Tile States")]
    public bool isVacant = true;
    public bool isDirty = false;

    Color _maroon = new Color(0.337f, 0.122f, 0.145f);

    void Update()
    {
        if(!isVacant)
        {
            foreach (Transform child in this.transform)
            {
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.materials[childRenderer.materials.Length - 1].color = _maroon;
            }
        }
        else
        {
            foreach (Transform child in this.transform)
            {
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.materials[childRenderer.materials.Length - 1].color = Color.white;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drag"))
        {
            Debug.Log("Passenger entered tile: " + other.name); // Check Console
            isVacant = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drag"))
        {
            if (tileType != TileTypes.Wall)
            {
                isVacant = true;
            }
        }
    }

}


