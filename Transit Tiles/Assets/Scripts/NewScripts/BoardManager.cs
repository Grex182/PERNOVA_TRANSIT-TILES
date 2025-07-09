using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> tileObjects = new List<GameObject>();
    [SerializeField] public Dictionary<TileTypes, List<TileData.Tile>> tileDataDictionary;

    private void Awake()
    {
        PopulateTileList();

        InitializeTileSystem();
        Debug.Log(tileDataDictionary.Count + " tile types initialized.");

        foreach (GameObject tileObj in tileObjects)
        {
            if (tileObj == null) continue;

            TileData tileDataComponent = tileObj.GetComponent<TileData>();
            if (tileDataComponent == null)
            {
                Debug.LogWarning($"Tile object {tileObj.name} is missing TileData component");
                continue;
            }

            TileData.Tile tile = tileDataComponent.GetTile();
            AddTile(tile);
        }
    }

    private void InitializeTileSystem()
    {
        tileDataDictionary = new Dictionary<TileTypes, List<TileData.Tile>>()
        {
            { TileTypes.Train, new List<TileData.Tile>() },
            { TileTypes.Seat, new List<TileData.Tile>() },
            { TileTypes.Station, new List<TileData.Tile>() }
        };

        foreach (GameObject tileObj in tileObjects)
        {
            if (tileObj == null) continue;

            TileData tileDataComponent = tileObj.GetComponent<TileData>();
            if (tileDataComponent == null)
            {
                Debug.LogWarning($"Tile object {tileObj.name} is missing TileData component");
                continue;
            }

            // Convert world position to grid coordinates (adjust scaling as needed)
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(tileObj.transform.position.x),
                Mathf.RoundToInt(tileObj.transform.position.z)
            );

            // Create new tile data
            TileData.Tile tile = new TileData.Tile(
                tileType: tileDataComponent.TileType,
                gridPosition: gridPos,
                isBottomSection: tileDataComponent.IsBottomSection,
                isExitLane: tileDataComponent.IsExitLane,
                isVacant: tileDataComponent.IsVacant,
                isDirty: tileDataComponent.IsDirty
            );

            // Add to dictionary
            AddTile(tile);
        }
    }

    public void AddTile(TileData.Tile tile)
    {
        if (tileDataDictionary.ContainsKey(tile.TileType))
        {
            tileDataDictionary[tile.TileType].Add(tile);
        }
        else
        {
            Debug.LogWarning($"Tile type {tile.TileType} not found in dictionary");
        }
    }

    public List<TileData> GetTilesOfType(TileTypes type)
    {
        List<TileData> tileList = new List<TileData>();
        for (int i = 0; i < tileObjects.Count; i++)
        {
            if (tileObjects[i].GetComponent<TileData>().TileType == type)
            {
                tileList.Add(tileObjects[i].GetComponent<TileData>());
            }
        }
        Debug.Log($"Got {tileList.Count} Tiles");
        return tileList;
    }

    public TileData.Tile GetTileAtPosition(Vector2Int gridPosition)
    {
        foreach (var tileList in tileDataDictionary.Values)
        {
            foreach (var tile in tileList)
            {
                if (tile.GridPosition == gridPosition)
                    return tile;
            }
        }
        return null;
    }

    private void PopulateTileList()
    {
        tileObjects.Clear(); // Clear list first

        // Loop through all children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            tileObjects.Add(child.gameObject);
        }

        Debug.Log($"Added {tileObjects.Count} children to list");
    }

    public void DisableStationsTiles()
    {
        if (LevelManager.Instance.currState != MovementState.Travel)
        {
            Debug.LogWarning("Disabling stations failed: Not in Travel state.");
            return;
        }

        foreach (GameObject tileObj in tileObjects)
        {
            if (tileObj == null) continue;

            TileData tileDataComponent = tileObj.GetComponent<TileData>();
            if (tileDataComponent == null) continue;

            TileData.Tile stationTile = tileDataComponent.GetTile();
            if (stationTile.TileType != TileTypes.Station) continue;

            // Disable station tile
            tileObj.SetActive(false);
        }

        Debug.Log("Station tiles disabled and passengers cleared.");
    }
}
