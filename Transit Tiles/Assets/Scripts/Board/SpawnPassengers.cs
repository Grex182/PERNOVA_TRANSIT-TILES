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

    private void Awake()
    {
        transform.SetParent(station);
    }

    //Spawning Pieces
    public void SpawnAllPieces()
    {
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

        int spawnCount = 0;

        while (spawnCount < 8)
        {
            int randomPositionX = Random.Range(7, 11); // 7, 8, 9
            int randomPositionY = Random.Range(0, 5);  // 0, 1, 2, 3

            if (passengers[randomPositionX, randomPositionY] == null)
            {
                passengers[randomPositionX, randomPositionY] = SpawnSinglePiece(PassengerType.Standard);
                if (GetComponent<Board>().boardType == BoardType.MainBoard)
                    tiles[randomPositionX, randomPositionY].layer = LayerMask.NameToLayer("Occupied");
                spawnCount++;
            }
            // else: position is already taken, try again
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
}