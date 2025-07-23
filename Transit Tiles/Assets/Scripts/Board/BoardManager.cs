using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[,] grid = new GameObject[20, 13];
    [SerializeField] private GameObject stationParent;
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private GameObject trashParent;

    [SerializeField] private int _trashSpawnChance;

    public void Initialize()
    {
        AssignTileToArray();
        SetParent();
        //ShiftStationTiles();

        _trashSpawnChance = 100;
    }

    public void ClearTrash()
    {
        foreach (Transform trash in trashParent.transform)
        {
            Vector3 pos = trash.position;
            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);

            grid[x, z].GetComponent<TileData>().isVacant = true;

            trash.GetComponent<TrashData>().TrashRemove();
        }
    }

    public void SpawnTrash()
    {
        List<Transform> validTiles = new List<Transform>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform child = this.transform.GetChild(i);
            TileData tileData = child.GetComponent<TileData>();

            if (tileData.tileType == TileTypes.Train && tileData.isVacant)
            {
                validTiles.Add(child);
            }
        }

        int tries = Random.Range(1, 4);
        for (int i = 0; i < tries; i++)
        {

            if (validTiles.Count == 0) break;

            int roll = Random.Range(0, 100);
            if (roll <= _trashSpawnChance)
            {
                int tileIndex = Random.Range(0, validTiles.Count);
                Transform tile = validTiles[tileIndex];

                Instantiate(trashPrefab, tile.transform.position, Quaternion.identity, trashParent.transform);

                validTiles.RemoveAt(tileIndex);
            }
        }
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
