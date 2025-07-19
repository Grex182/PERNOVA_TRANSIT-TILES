using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameObject stationParent;
    [SerializeField] private List<GameObject> passengerPrefabs;
    [SerializeField] private List<GameObject> passengerBulkyPrefabs;

    [SerializeField] private Vector2Int _staStart = new Vector2Int(7, 1);
    [SerializeField] private Vector2Int _staSize = new Vector2Int(6, 5);

    private bool _isStartingStation = true;
    StationColor _stationException = StationColor.Red;

    private readonly int minPassengers = 3;
    [SerializeField] private int chanceBulky = 80; // Doesnt work as well rn, this value doesnt matter bc bulky passengers take so much space

    public void SpawnPassengers()
    {
        _stationException = LevelManager.Instance.nextStation;
        if (_isStartingStation) // Prevents Red Station from spawning at the start of the game
        {
            _stationException = LevelManager.Instance.currStation;
            _isStartingStation = false;
        }
        Debug.Log($"Station Exception: {_stationException}");
        int spawnPotential = GetSpawnPotential();

        int count = Random.Range(minPassengers, spawnPotential);

        for (int i = 0; i < count;)
        {
            int randX = Random.Range(_staStart.x, _staStart.x + _staSize.x);
            int randY = Random.Range(_staStart.y, _staStart.y + _staSize.y);

            GameObject spawnTile = boardManager.grid[randX, randY];
            GameObject bulkyTile = null;
            if (boardManager.grid[randX, randY+1] != null && boardManager.grid[randX, randY + 1].GetComponent<TileData>().tileType == TileTypes.Station)
            {
                bulkyTile = boardManager.grid[randX, randY + 1];
            }
            //BulkyChance
            int passengerChance = Random.Range(0, 100);


            bool canSpawnBulky = spawnTile != null && spawnTile.GetComponent<TileData>().isVacant
                               && bulkyTile != null && bulkyTile.GetComponent<TileData>().isVacant;

            if (passengerChance <= chanceBulky && i + 1 < count && canSpawnBulky)
            {
                SpawnSinglePassenger(spawnTile, passengerBulkyPrefabs);
                spawnTile.GetComponent<TileData>().isVacant = false;
                bulkyTile.GetComponent<TileData>().isVacant = false;

                i += 2;
            }
            else if (spawnTile != null && spawnTile.GetComponent<TileData>().isVacant)
            {
                SpawnSinglePassenger(spawnTile, passengerPrefabs);
                spawnTile.GetComponent<TileData>().isVacant = false;
                i++;
            }

        }

    }

    private void SpawnSinglePassenger(GameObject spawnTile, List<GameObject> passengerList)
    {
        GameObject randPass = passengerList[Random.Range(0, passengerList.Count)];
        PassengerData data = randPass.GetComponent<PassengerData>();

        StationColor randStation = GetRandomStation();

        data.targetStation = randStation;
        data.currTile = TileTypes.Station;

        GameObject passengerSpawn = Instantiate(randPass, spawnTile.transform.position,
                                           Quaternion.identity, stationParent.transform);
        passengerSpawn.transform.localScale = Vector3.one * 0.01f; // Adjust scale if needed
        //spawnedPassengers.Add(passengerSpawn);
    }

    private StationColor GetRandomStation()
    {

        StationColor randStation = StationColor.Red;

        StationColor[] _stationColors = (StationColor[])System.Enum.GetValues(typeof(StationColor));
        do
        {
            int randStationIndex = Random.Range(0, _stationColors.Length);
            randStation = _stationColors[randStationIndex];
        } while (randStation == _stationException);
        return randStation;
    }

    private int GetSpawnPotential()
    {
        int count = 0;
        //Scan Through Station Tiles
        for (int x = 0; x < _staSize.x; x++)
        {
            for (int y = 0; y < _staSize.y; y++)
            {
                GameObject tile = boardManager.grid[_staStart.x + x, _staStart.y + y];
                if (tile != null)
                {
                    TileData data = tile.GetComponent<TileData>();
                    if (data.isVacant && data.canSpawnHere)
                    {
                        count++;
                    }
                }
            }
        }

         return count;
    }

    public void DeletePassengers()
    {
        int _deleteCount = 0;
        int _minusScore = 0;
        foreach (Transform child in stationParent.transform)
        {
            int score = child.GetComponent<PassengerData>().isPriority ? -200 : -100;
            _deleteCount++;
            LevelManager.Instance.AddScore(score);
            _minusScore += score;
            Destroy(child.gameObject); // Delete passenger
        }
        Debug.Log($"Deleted {_deleteCount} passengers. Total {_minusScore} Points.");
    }

}
