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
    [Header("References")]
    [SerializeField] private PassengerMovement passengerMovement;

    [Header("Tile Setting")]
    public TileTypes tileType;
    public bool isBottomSection;
    public bool canSpawnHere; // NOTE: Not fixed value, can be set in a for loop (?)

    [Header("Tile States")]
    public bool isVacant = true;
    public bool isDirty = false;
    public bool isHoveredOver = false;

    Color _maroon = new Color(0.337f, 0.122f, 0.145f);
    Color _yellow = new Color(1.000f, 0.806f, 0.397f);

    void Start()
    {
        Initialize();
    }
    void Update()
    {
        if (passengerMovement.selectedObject == null)
        {
            isHoveredOver = false;
        }

        if (isHoveredOver)
        {
            foreach (Transform child in this.transform)
            {
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.materials[childRenderer.materials.Length - 1].color = _yellow;
            }
        }
        else if (isVacant)
        {
            
            //isHoveredOver = !(passengerMovement.selectedObject == null);
            foreach (Transform child in this.transform)
            {
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.materials[childRenderer.materials.Length - 1].color = Color.white;
            }
        }
        else
        {
            foreach (Transform child in this.transform)
            {
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.materials[childRenderer.materials.Length - 1].color = _maroon;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hover"))
        {
            isHoveredOver = true;
        }
        if (other.CompareTag("Drag"))
        {
            isVacant = false;
        }
        
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hover"))
        {
            isHoveredOver = false;
        }
        if (other.CompareTag("Drag"))
        {
            bool isTravelTiles = LevelManager.Instance.currState == MovementState.Travel && tileType == TileTypes.Station;
            if (tileType != TileTypes.Wall && !isTravelTiles)
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


