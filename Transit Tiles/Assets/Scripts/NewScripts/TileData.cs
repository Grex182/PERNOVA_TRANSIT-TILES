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

    void Start()
    {
        Initialize();
    }
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

    private void Initialize()
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
        switch (tileType)
        {             
            case TileTypes.Train:
                transform.GetChild(2).gameObject.SetActive(true);
                break;
            case TileTypes.Seat:
                transform.GetChild(1).gameObject.SetActive(true);
                break;
            case TileTypes.Station:
                transform.GetChild(0).gameObject.SetActive(true);
                break;
            case TileTypes.Wall:
                
                break;
            default:
                Debug.LogError("Tile type not recognized: " + tileType);
                break;
        }
    }

}


