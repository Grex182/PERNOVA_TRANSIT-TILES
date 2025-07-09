using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private List<GameObject> passengerPrefabs;
    [SerializeField] private List<GameObject> passengerBulkyPrefabs;

    private readonly int minPassengers = 3;
    [SerializeField] private int chanceBulky = 80; // Doesnt work as well rn, this value doesnt matter bc bulky passengers take so much space

    private List<GameObject> spawnedPassengers = new List<GameObject>();


    private void SpawnSinglePassenger(TileData stationObject, GameObject passenger)
    {
        GameObject passengerSpawn = Instantiate(passenger, stationObject.transform.position, 
                                           Quaternion.identity, this.transform);

        
        int stationNumber = Random.Range(1, 8); // Assuming station numbers are between 1 and 7

        PassengerInstance passengerInstance = passengerSpawn.GetComponent<PassengerInstance>();
        
        passengerInstance.Initialize(stationNumber, PassengerInstance.PassengerLocation.Station);

        spawnedPassengers.Add(passengerSpawn);
    }

    public void SpawnPassengerGroup()
    {
        List<TileData> validStations = GetValidSpawnTiles();
        //int count = Random.Range(minPassengers, validStations.Count);
        int count = 15;
        if (validStations.Count == 0)
        {
            Debug.LogWarning("No available stations for passenger spawning");
            return;
        }

        Debug.Log("Spawn count = " + count);

        for (int i = 0; i < count;)
        {
            int randomIndex = Random.Range(0, validStations.Count);
            TileData spawnTile = validStations[randomIndex];
            int bulkyIndex = randomIndex;

            Vector2 bulkyCheck = new Vector2(spawnTile.GridPos.x, spawnTile.GridPos.y+1);
            TileData bulkyObj = null;
            for (int j = 0; j < validStations.Count; j++)
            {
                if (validStations[j].GridPos == bulkyCheck)
                {
                    bulkyObj = validStations[j];
                    bulkyIndex = j;
                }
            }

            //BulkyChance
            int passengerChoice = Random.Range(0, 100);

            bool canSpawnBulky = spawnTile != null && spawnTile.IsVacant
                               && bulkyObj != null && bulkyObj.IsVacant;

            if (passengerChoice <= chanceBulky && i + 1 < count && canSpawnBulky)
            {
                    GameObject randPass = passengerBulkyPrefabs[Random.Range(0,passengerBulkyPrefabs.Count)];
                    SpawnSinglePassenger(spawnTile, randPass);
                    spawnTile.IsVacant = false;
                    bulkyObj.IsVacant = false;

                    Debug.Log(i + "Spawned Bulky at = " + spawnTile.GridPos.x + "," + spawnTile.GridPos.y + " & " + bulkyObj.GridPos.y);
                    i += 2;
            }
            else if (spawnTile != null && spawnTile.IsVacant)
            {
                    GameObject randPass = passengerPrefabs[Random.Range(0, passengerPrefabs.Count)];
                    SpawnSinglePassenger(spawnTile, randPass);
                    spawnTile.IsVacant = false;
                    Debug.Log(i + "Spawned Standard at = " + spawnTile.GridPos.x + "," + spawnTile.GridPos.y);
                    i++;
            }

            
        }

        Debug.Log($"Spawned {count} passengers");
    }


    private List<TileData> GetValidSpawnTiles()
    {
        var allStations = boardManager.GetTilesOfType(TileTypes.Station);
        if (allStations == null || allStations.Count == 0)
        {
            Debug.LogWarning("No station tiles exist at all!");
            return new List<TileData>();
        }

        //var validStations = allStations.FindAll(tile => !tile.IsExitLane && tile.IsVacant);

        List<TileData> validStations = new List<TileData>();
        for (int i = 0; i < allStations.Count; i++)
        {
            if (!allStations[i].IsExitLane && allStations[i].IsVacant)
            {
                validStations.Add(allStations[i]);
            }
        }
        Debug.Log($"Found {validStations.Count} valid stations out of {allStations.Count} total stations");

        return validStations;
    }

    private TileData FindStationObject(Vector2 gridPosition)
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
                return tileObj.GetComponent<TileData>();
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
