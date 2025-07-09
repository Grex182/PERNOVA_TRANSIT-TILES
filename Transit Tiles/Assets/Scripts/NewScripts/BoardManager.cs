using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[,] grid = new GameObject[18, 10];

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        AssignTileToArray();
        ShiftStationTiles();
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

    private GameObject GetNextStation(Transform parent)
    {
        int activeChild = 0;

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (child.activeInHierarchy)
            {
                activeChild++;
                
                if (activeChild == 2) // Get the second active child
                {
                    return child;
                }
            }
        }

        return null;
    }

    private void SetParent()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                GameObject tile = grid[x, z];
                if (tile != null && tile.GetComponent<TileData>().tileType == TileTypes.Station)
                {
                    GameObject newParent = GetNextStation(WorldGenerator.Instance.stationsParent.transform);
                    tile.transform.SetParent(newParent.transform, true);
                }
            }
        }
    }
}
