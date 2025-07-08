using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameObject passengerPrefab;

    private readonly int minPassengers = 3;

    private List<GameObject> spawnedPassengers = new List<GameObject>();

    private List<string> passengerTypes = new List<string>
    {
        "Standard", "Bulky"
    };

    private void SpawnSinglePassenger(GameObject stationObject)
    {
        GameObject passenger = Instantiate(passengerPrefab, stationObject.transform.position, 
                                           Quaternion.identity, this.transform);

        string passengerType = passengerTypes[Random.Range(0, passengerTypes.Count)];
        int stationNumber = Random.Range(1, 8); // Assuming station numbers are between 1 and 7

        PassengerInstance passengerInstance = passenger.GetComponent<PassengerInstance>();
        
        passengerInstance.Initialize(passengerType, stationNumber, 
                                     PassengerInstance.PassengerLocation.Station);

        spawnedPassengers.Add(passenger);
    }

    public void SpawnPassengerGroup()
    {
        List<TileData.Tile> validStations = GetValidSpawnTiles();
        int count = Random.Range(minPassengers, validStations.Count);

        if (validStations.Count == 0)
        {
            Debug.LogWarning("No available stations for passenger spawning");
            return;
        }

        Debug.Log("Spawn count = " + count);

        for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, validStations.Count);
                TileData.Tile spawnTile = validStations[randomIndex];
                GameObject stationObject = FindStationObject(spawnTile.GridPosition);

                if (stationObject != null)
                {
                    SpawnSinglePassenger(stationObject);
                    spawnTile.IsVacant = false;
                }
            }

        Debug.Log($"Spawned {count} passengers");
    }


    private List<TileData.Tile> GetValidSpawnTiles()
    {
        var allStations = boardManager.GetTilesOfType(TileTypes.Station);
        if (allStations == null || allStations.Count == 0)
        {
            Debug.LogWarning("No station tiles exist at all!");
            return new List<TileData.Tile>();
        }

        var validStations = allStations.FindAll(tile => !tile.IsExitLane && tile.IsVacant && tile.TileType == TileTypes.Station);

        Debug.Log($"Found {validStations.Count} valid stations out of {allStations.Count} total stations");

        return validStations;
    }

    private GameObject FindStationObject(Vector2 gridPosition)
    {
        foreach (GameObject tileObj in boardManager.tileObjects)
        {
            if (tileObj == null) continue;

            TileData tileData = tileObj.GetComponent<TileData>();
            if (tileData != null &&
                tileData.TileType == TileTypes.Station &&
                new Vector2Int(
                    Mathf.RoundToInt(tileObj.transform.position.x),
                    Mathf.RoundToInt(tileObj.transform.position.z)
                ) == gridPosition)
            {
                return tileObj;
            }
        }
        return null;
    }

    public void DespawnStationPassengers()
    {
        foreach (var passenger in spawnedPassengers)
        {
            if (passenger.GetComponent<PassengerInstance>().currLocation == PassengerInstance.PassengerLocation.Station)
            {
                Destroy(passenger);
                spawnedPassengers.Remove(passenger);
            }
        }
    }
}
