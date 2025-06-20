using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPassengers : MonoBehaviour
{
    [SerializeField] public float tileSize = 1f;
    [SerializeField] public float gapSize = 0.1f;

    [SerializeField] public float yOffset = 0.2f;
    [SerializeField] public float yPositionOffset;

    [SerializeField] public GameObject[] prefabs;

    public Passenger[,] passengers;
    [SerializeField] public int tileCountX = 8;
    [SerializeField] public int tileCountY = 8;
    public GameObject[,] tiles;

    [SerializeField] public List<Passenger> spawnedPassengers = new List<Passenger>();
    public Vector3 bounds;

    [SerializeField] private Transform station;

    public int spawnCount = 0;

    public int randomPositionX; // 7, 8, 9
    public int randomPositionY;  // 0, 1, 2, 3

    public static List<PassengerData> savedPassengerData = new List<PassengerData>();

    public static List<Vector2Int> savedPassengerPositions = new List<Vector2Int>();

    [SerializeField] private bool hasGeneratedData = false;
    [SerializeField] public bool hasAppliedData = false;

    private void Awake()
    {
        transform.SetParent(station);
    }

    private void Update()
    {
        // Only destroy passengers if this is NOT the MainBoard
        if (GetComponent<Board>().boardType == BoardType.StationBoard)
        {
            if (!GameManager.instance.StationManager.isTrainMoving && spawnedPassengers.Count > 0 && GameManager.instance.StationManager.hasPassengersSpawned)
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
/*                savedPassengerData.Clear();
                savedPassengerPositions.Clear();*/
            }
        }
    }

    //Spawning Pieces
    public void SpawnAllPieces()
    {
        Board board = GetComponent<Board>();

        //if we want to have a random amount of passengers be spawned, and ig 8 passengers will be the maximum spawns
        /*        if (passengers == null)
                {
                    passengers = new Passenger[tileCountX, tileCountY];
                }

                for (int i = 0; i < 8; i++)
                {
                    int randomPositionX = Random.Range(7, 10);
                    int randomPositionY = Random.Range(0, 4);

                    if (passengers[randomPositionX, randomPositionY] == null)
                    {
                        passengers[randomPositionX, randomPositionY] = SpawnSinglePiece(PassengerType.Standard); //The enum inside the parenthesis here should also be randomized once other types of passengers have been made
                        tiles[randomPositionX, randomPositionY].layer = LayerMask.NameToLayer("Occupied");
                    }

                    //passengers[randomPositionX, randomPositionY] = SpawnSinglePiece(PassengerType.Standard); //The enum inside the parenthesis here should also be randomized once other types of passengers have been made
                }*/

        //if we want to ALWAYS have 8 passengers be spawned
        if (passengers == null)
        {
            passengers = new Passenger[tileCountX, tileCountY];
        }

        if (savedPassengerData.Count == 0)
        {
            // First-time spawn — generate and save data
            GenerateRandomPositions();

            foreach (Vector2Int pos in savedPassengerPositions)
            {
                PassengerType type = PassengerType.Standard; // You can randomize this later
                Passenger p = SpawnSinglePiece(type);

                // Assign random StationColor and set it
                StationColor stationColor = (StationColor)Random.Range(0, System.Enum.GetValues(typeof(StationColor)).Length);
                p.assignedColor = stationColor;
                p.SetPassengerStation(); // to visually apply it

                // Save data
                savedPassengerData.Add(new PassengerData(type, stationColor.ToString(), pos));

                passengers[pos.x, pos.y] = p;

                if (GetComponent<Board>().boardType == BoardType.MainBoard)
                tiles[pos.x, pos.y].layer = LayerMask.NameToLayer("Occupied");
                Debug.Log("Applied passenger data");
            }

            hasGeneratedData = true;
            Debug.Log("Generated passenger data");
        }
        else
        {
            // Re-spawn from saved data
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

        if (board.boardType == BoardType.StationBoard)
        {
            if (hasAppliedData && GameManager.instance.Board.GetComponent<SpawnPassengers>().hasAppliedData)
            {
                ResetData();
            }
        }
        else if (board.boardType == BoardType.MainBoard)
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

        if (GetComponent<Board>().boardType == BoardType.StationBoard)
            GameManager.instance.Board.GetComponent<SpawnPassengers>().ResetData();

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
        //passenger.transform.localScale = Vector3.one;
        passenger.transform.SetParent(transform);
        passenger.type = type;
        spawnedPassengers.Add(passenger);

        return passenger;
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
        Vector3 localOffsetFromBoard = GetTileCenter(x, y);

        // Calculate world tile position
        Vector3 tileWorldPos = boardWorldOrigin + localOffsetFromBoard;

        // Set the position
        passengers[x, y].SetPosition(tileWorldPos, force);
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        float xPos = x * (tileSize + gapSize);
        float zPos = y * (tileSize + gapSize);

        return new Vector3(xPos, yOffset + yPositionOffset, zPos) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    private StationColor GetStationColor(string stationColor)
    {
        switch (stationColor)
        {
            case "Pink": return StationColor.Pink;
            case "Red": return StationColor.Red;
            case "Orange": return StationColor.Orange;
            case "Yellow": return StationColor.Yellow;
            case "Green": return StationColor.Green;
            case "Blue": return StationColor.Blue;
            case "Violet": return StationColor.Violet;
            default: return StationColor.Red;
        }
    }
}