using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public static PassengerSpawner Instance;

    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    public GameObject stationParent;
    public GameObject trainParent;
    [SerializeField] private GameObject[] passengerPrefabs;
    [SerializeField] private GameObject[] passengerBulkyPrefabs;

    [SerializeField] private Vector2Int _staStart = new Vector2Int(7, 1);
    [SerializeField] private Vector2Int _staSize = new Vector2Int(6, 5);

    private bool _isStartingStation = true;
    StationColor _stationException = StationColor.Red;

    private readonly int minPassengers = 3;
    [SerializeField] private int chanceBulky = 80; // Doesnt work as well rn, this value doesnt matter bc bulky passengers take so much space

    private int[] _singleSpawnRates = new int[5] 
    { 
        7, // Normal
        1, // Noisy
        2, // Stinky
        1, // Pregnant
        1 // Sleepy
    };

    private int[] _bulkySpawnRates = new int[2]
    {
        3, // Bulky
        1 // Elder
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnPassengers()
    {
        _stationException = LevelManager.Instance.currStation;
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
                GameObject bulkyPassenger = TypeToSpawn(passengerBulkyPrefabs, _bulkySpawnRates);
                SpawnSinglePassenger(spawnTile, bulkyPassenger); // Fix Later
                spawnTile.GetComponent<TileData>().isVacant = false;
                bulkyTile.GetComponent<TileData>().isVacant = false;

                i += 2;
            }
            else if (spawnTile != null && spawnTile.GetComponent<TileData>().isVacant)
            {
                GameObject singlePassenger = TypeToSpawn(passengerPrefabs, _singleSpawnRates);
                SpawnSinglePassenger(spawnTile, singlePassenger);
                spawnTile.GetComponent<TileData>().isVacant = false;
                i++;
            }
        }
    }

    private GameObject TypeToSpawn(GameObject[] passArray, int[] passWeight)
    {
        int totalRate = 0;
        for (int i = 0; i < passWeight.Length; i++)
        {
            totalRate += passWeight[i];
        }

        int spawnRoll = Random.Range(0, totalRate);

        for (int i = 0; i < passWeight.Length; i++)
        {
            if (spawnRoll < passWeight[i])
            {
                return passArray[i];
            }
            else
            {
                spawnRoll -= passWeight[i];
            }
        }
        
        return null;
    }

    public void GetTotalDisembarkCount()
    {
        int passengerCount = trainParent.transform.childCount;

        for (int i = 0; i < passengerCount; i++)
        {
            GameObject child = trainParent.transform.GetChild(i).gameObject;
            if (child.GetComponent<PassengerData>().targetStation == LevelManager.Instance.currStation)
            {
                LevelManager.Instance.passengerToDisembarkCount++;
            }
        }
    }

    private void SpawnSinglePassenger(GameObject spawnTile, GameObject passenger)
    {
        PassengerData data = passenger.GetComponent<PassengerData>();

        StationColor randStation = GetRandomStation();

        data.targetStation = randStation;
        data.currTile = TileTypes.Station;

        GameObject passengerSpawn = Instantiate(passenger, spawnTile.transform.position,
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

    public void ClearTrainDoors()
    {
        List<GameObject> blockedPassengers = new List<GameObject>();
        //Get Passengers Blocking Train Doors
        GetBlockedPassengers(stationParent.transform, new Vector2Int(8, 6), new Vector2Int(4, 1), new Vector3(0f, 0f, 0f), blockedPassengers);
        GetBlockedPassengers(trainParent.transform, new Vector2Int(8, 7), new Vector2Int(4, 1), new Vector3(0f, 180f, 0f), blockedPassengers);

        for (int i = 0; i < blockedPassengers.Count; i++)
        {
            int posX = 6 + i;
            int posZ = 5;
            if (i > 1) { posX += 4; } // Move to other side of board
            
            blockedPassengers[i].transform.SetParent(stationParent.transform, true);
            blockedPassengers[i].transform.position += Vector3.up * 1f;

            PassengerData _data = blockedPassengers[i].GetComponent<PassengerData>();
            
            _data.currTile = TileTypes.Station;

            blockedPassengers[i].transform.rotation = Quaternion.identity;

            _data.GoToPose(new Vector3(posX, 0f, posZ),Quaternion.identity, 5f,10f);

        }
    }

    private void GetBlockedPassengers(Transform parentObj, Vector2Int pos, Vector2Int size, Vector3 rotation, List<GameObject> list)
    {
        foreach (Transform child in parentObj)
        {
            int childX = Mathf.RoundToInt(child.transform.position.x);
            int childZ = Mathf.RoundToInt(child.transform.position.z);
            PassengerData data = child.GetComponent<PassengerData>();
            float tolerance = 5f;

            bool posCheck = childX >= pos.x && childX < pos.x + size.x &&
                            childZ >= pos.y && childZ < pos.y + size.y;
            bool OrientationCheck = (data.traitType == PassengerTrait.Bulky || data.traitType == PassengerTrait.Elderly) &&
                                     Quaternion.Angle(child.transform.rotation, Quaternion.Euler(rotation)) < tolerance;

            if (posCheck && OrientationCheck)
            {
                list.Add(child.gameObject);
                Debug.Log($"added {child} to blocked passengers list");
            }
        }
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
