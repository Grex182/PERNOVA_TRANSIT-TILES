using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPassengers : Singleton<SpawnPassengers>
{
    [SerializeField] public GameObject[] prefabs;

    public Passenger[,] passengers;
    [SerializeField] public int tileCountX = 8;
    [SerializeField] public int tileCountY = 8;
    public GameObject[,] tiles;

    [SerializeField] public List<Passenger> spawnedPassengers = new List<Passenger>();

    [SerializeField] private Transform station;

    public int spawnCount = 0;

    [Header("Passenger Type Chance")]
    public int randomPositionX; // 7, 8, 9
    public int randomPositionY;  // 0, 1, 2, 3
    [SerializeField] public int normalPassengerSpawnChance;
    [SerializeField] public int elderPassengerSpawnChance;
    [SerializeField] public int bulkyPassengerSpawnChance;

    [Header("Passenger Effect Chance")]
    [SerializeField] public int noisyPassengerSpawnChance;
    [SerializeField] public int smellyPassengerSpawnChance;

    [Header("Saved Passenger Lists")]
    public static List<PassengerData> savedPassengerData = new List<PassengerData>();
    public static List<Vector2Int> savedPassengerPositions = new List<Vector2Int>();

    [Header("Data Bools")]
    [SerializeField] private bool hasGeneratedData = false;
    [SerializeField] public bool hasAppliedData = false;

    [SerializeField] public bool passengersSpawned = false;

    private void Awake()
    {
        transform.SetParent(station);
    }

    private void Start()
    {
        this.enabled = false;
    }

    private void Update()
    {
        // Only destroy passengers if this is NOT the MainBoard
        if (Board.Instance.boardType == BoardType.StationBoard)
        {
            if (LevelManager.Instance.currState == MovementState.Station && spawnedPassengers.Count > 0 && passengersSpawned)
            {
                for (int x = 0; x < tileCountX; x++)
                {
                    for (int y = 0; y < tileCountY; y++)
                    {
                        if (passengers[x, y] != null)
                        {
                            Destroy(passengers[x, y].gameObject);
                            passengers[x, y] = null;
                        }
                    }
                }

                spawnedPassengers.Clear();
            }
        }
    }

    public void SpawnAllPieces()
    {
        if (passengers == null)
        {
            passengers = new Passenger[tileCountX, tileCountY];
        }

        if (savedPassengerData.Count == 0)
        {
            GenerateRandomPositions();

            foreach (Vector2Int pos in savedPassengerPositions)
            {

                SpawnRandomPassenger(pos);

/*              PassengerType type = PassengerType.Standard; //Can be randomized later
                Passenger p = SpawnSinglePiece(type);

                StationColor stationColor = (StationColor)Random.Range(0, System.Enum.GetValues(typeof(StationColor)).Length);
                p.assignedColor = stationColor;
                p.SetPassengerStation(); //To visually apply it

                savedPassengerData.Add(new PassengerData(type, stationColor.ToString(), pos));

                passengers[pos.x, pos.y] = p;*/

                if (Board.Instance.boardType == BoardType.MainBoard)
                tiles[pos.x, pos.y].layer = LayerMask.NameToLayer("Occupied");
            }

            hasGeneratedData = true;
            Debug.Log("Generated passenger data");
        }
        else
        {
            foreach (PassengerData data in savedPassengerData)
            {
                Passenger p = SpawnSinglePiece(data.type);
                p.assignedColor = GetStationColor(data.assignedColor); // Convert back from Color to enum
                p.SetPassengerStation();

                passengers[data.position.x, data.position.y] = p;
                PositionSinglePiece(data.position.x, data.position.y, true);
                Debug.Log("Re-spawned passengers");
            }

            spawnCount = 0;
            hasAppliedData = true;
        }

        if (Board.Instance.boardType == BoardType.StationBoard)
        {
            if (hasAppliedData)
            {
                ResetData();
            }
        }
        else if (Board.Instance.boardType == BoardType.MainBoard)
        {
            if (hasGeneratedData && hasAppliedData)
            {
                ResetData();
            }
        }
    }

    public void ResetData()
    {
        savedPassengerData.Clear();
        savedPassengerPositions.Clear();
        hasGeneratedData = true;
        hasAppliedData = false;

        if (Board.Instance.boardType == BoardType.StationBoard)
        {
            ResetData();
        }

        Debug.Log("Cleared passenger data and positions");
    }

    public void GenerateRandomPositions()
    {
        while (spawnCount < 8)
        {
            int x = Random.Range(7, 11); // 7, 8, 9, 10
            int y = Random.Range(0, 5);  // 0, 1, 2, 3

            Vector2Int pos = new Vector2Int(x, y);

            if (!savedPassengerPositions.Contains(pos))
            {
                savedPassengerPositions.Add(pos);
                spawnCount++;
            }
        }
    }

    private Passenger SpawnSinglePiece(PassengerType type)
    {
        Passenger passenger = Instantiate(prefabs[(int)type - 1]).GetComponent<Passenger>();
        passenger.transform.SetParent(transform);
        passenger.type = type;
        spawnedPassengers.Add(passenger);

        return passenger;
    }

    private void SpawnRandomPassenger(Vector2Int pos)
    {
        PassengersChecker passengersChecker = PassengersChecker.Instance;
        int randomNum = Random.Range(0, 101);

        if (passengersChecker.currentSpecialPassengers >= passengersChecker.maxSpecialPassengers)
        {
            SpawnPassengerWithData(PassengerType.Standard, pos);
        }
        else
        {
            if (randomNum <= normalPassengerSpawnChance)
            {
                SpawnPassengerWithData(PassengerType.Standard, pos);
                Debug.Log("Spawned Standard Passenger");
            }
            else if (randomNum <= normalPassengerSpawnChance + bulkyPassengerSpawnChance) //Spawn bulky passenger
            {
                SpawnPassengerWithData(PassengerType.Bulky, pos);
                Debug.Log("Spawned Bulky Passenger");
            }
            else if (randomNum <= normalPassengerSpawnChance + bulkyPassengerSpawnChance + elderPassengerSpawnChance) //Spawn elder passenger
            {
                SpawnPassengerWithData(PassengerType.Elder, pos);
                Debug.Log("Spawned Elder Passenger");
            }
        }
    }

    private void SpawnPassengerWithData(PassengerType type, Vector2Int pos)
    {
        Passenger p = SpawnSinglePiece(type);

        if (type == PassengerType.Standard)
        {
            p.gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
        }

        StationColor stationColor = (StationColor)Random.Range(0, System.Enum.GetValues(typeof(StationColor)).Length);
        p.assignedColor = stationColor;
        p.SetPassengerStation(); //To visually apply it

        savedPassengerData.Add(new PassengerData(type, stationColor.ToString(), pos));

        passengers[pos.x, pos.y] = p;
    }

    //Positioning
    public void PositionAllPieces() //snaps all the pieces where they're supposed to be (useful for spawning pieces at the start of game or round)
    {
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if (passengers[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    public void PositionSinglePiece(int x, int y, bool force = false)
    {
        passengers[x, y].currentX = x;
        passengers[x, y].currentY = y;

        // Get the board's world position
        Vector3 boardWorldOrigin = transform.position;

        // Get local offset for the tile
        Vector3 localOffsetFromBoard = Board.Instance.tileSettingsScript.GetTileCenter(x, y);

        // Calculate world tile position
        Vector3 tileWorldPos = boardWorldOrigin + localOffsetFromBoard;

        // Set the position
        passengers[x, y].SetPosition(tileWorldPos, force);
    }

    private StationColor GetStationColor(string stationColor)
    {
        switch (stationColor)
        {
            case "Red": return StationColor.Red;
            case "Pink": return StationColor.Pink;
            case "Orange": return StationColor.Orange;
            case "Yellow": return StationColor.Yellow;
            case "Green": return StationColor.Green;
            case "Blue": return StationColor.Blue;
            case "Violet": return StationColor.Violet;
            default: return StationColor.Red;
        }
    }
}