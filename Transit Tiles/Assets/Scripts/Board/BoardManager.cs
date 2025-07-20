using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[,] grid = new GameObject[20, 13];

    [SerializeField] private GameObject stationParent;


    public void Initialize()
    {
        AssignTileToArray();
        SetParent();
        //ShiftStationTiles();
    }

    private void AssignTileToArray()
    {
        foreach (Transform child in transform)
        {
            int x = Mathf.RoundToInt(child.transform.position.x);
            int z = Mathf.RoundToInt(child.transform.position.z);

            if (x >= 0 && x < grid.GetLength(0) &&
                z >= 0 && z < grid.GetLength(1))
            {
                grid[x, z] = child.gameObject;
            }
            else
            {
                Debug.LogWarning($"Tile position ({x}, {z}) is out of bounds. Skipping assignment for {child.gameObject.name}.");
                continue;
            }
        }
    }

    private void ShiftStationTiles()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                GameObject tile = grid[x, z];
                if (tile != null && tile.GetComponent<TileData>().tileType == TileTypes.Station)
                {
                    Vector3 position = tile.transform.position;
                    position.z -= 1f; // Shift right by 1 unit
                    tile.transform.position = position;
                }
            }
        }

        SetParent();
    }

    public void VacateStationTiles(bool isVacant)
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < 6; z++)
            {
                GameObject tile = grid[x, z];
                if (tile != null && tile.GetComponent<TileData>().tileType == TileTypes.Station)
                {
                    tile.GetComponent<TileData>().isVacant = isVacant;
                }
            }
        }

    }

    

    public void BlockStationTiles(bool doBlock)
    {
        for (int x = 0; x < 4; x++)
        {
                GameObject tile = grid[8+x, 6];
                if (tile != null && tile.GetComponent<TileData>().tileType == TileTypes.Station)
                {
                    tile.GetComponent<TileData>().isVacant = !doBlock;
                }
        }

    }

    private void SetParent()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < 6; z++)
            {
                GameObject tile = grid[x, z];
                if (tile != null && tile.GetComponent<TileData>().tileType == TileTypes.Station)
                {
                    tile.transform.SetParent(stationParent.transform, true);
                }
            }
        }
    }

    //Testing
    private int[,] _spawnTilesNormal = new int[6, 5]
    {
        { 1, 1, 1, 1, 0 }, // Row 0
        { 1, 1, 1, 1, 1 }, // Row 1
        { 1, 1, 1, 1, 1 }, // Row 2
        { 1, 1, 1, 1, 1 }, // Row 3
        { 1, 1, 1, 1, 1 }, // Row 4
        { 1, 1, 1, 1, 0 }  // Row 5
    };
    private int[,] _spawnTilesGood = new int[6, 5]
    {
        { 1, 1, 1, 1, 0 }, // Row 0
        { 1, 1, 1, 1, 0 }, // Row 1
        { 1, 1, 1, 1, 0 }, // Row 2
        { 1, 1, 1, 1, 0 }, // Row 3
        { 1, 1, 1, 1, 0 }, // Row 4
        { 1, 1, 1, 1, 0 }  // Row 5
    };
    private int[,] _spawnTilesBest = new int[6, 5]
    {
        { 1, 1, 1, 1, 0 }, // Row 0
        { 1, 1, 1, 1, 0 }, // Row 1
        { 0, 0, 0, 0, 0 }, // Row 2
        { 0, 0, 0, 0, 0 }, // Row 3
        { 1, 1, 1, 1, 0 }, // Row 4
        { 1, 1, 1, 1, 0 }  // Row 5
    };

}
