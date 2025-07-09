using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileTypes
{
    Train,
    Seat,
    Station
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
}
