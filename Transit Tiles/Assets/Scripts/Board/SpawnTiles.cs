using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTiles : Singleton<SpawnTiles>
{
    [SerializeField] public Material tileMaterial;
                     
    [SerializeField] public GameObject platformTile;
    [SerializeField] public GameObject chairTile;
    [SerializeField] public GameObject trainTile;

    [SerializeField] public float yOffsetFloorTile;

    [SerializeField] public List<GameObject> platformTiles = new List<GameObject>();
    [SerializeField] public List<GameObject> platformTilePrefabs = new List<GameObject>();

    //Generates the board
    public void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        GetComponent<TileSettings>().bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + GetComponent<Board>().boardCenter;

        SpawnPassengers.Instance.tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);
                SpawnPassengers.Instance.tiles[x, y] = GenerateSingleTile(tileSize, x, y);

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.OccupiedTiles, tilePos))
                {
                    SpawnPassengers.Instance.tiles[x, y].layer = LayerMask.NameToLayer("Unavailable");
                }

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.TaggedTrainTiles, tilePos))
                {
                    SpawnPassengers.Instance.tiles[x, y].tag = "TrainTile";
                }
                else if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.TaggedPlatformTiles, tilePos))
                {
                    SpawnPassengers.Instance.tiles[x, y].tag = "PlatformTile";
                    platformTiles.Add(SpawnPassengers.Instance.tiles[x, y]);
                }

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.ChairTiles, tilePos))
                {
                    SpawnPassengers.Instance.tiles[x, y].tag = "ChairTile";

                    GameObject chair = Instantiate(chairTile, new Vector3(GetComponent<TileSettings>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<TileSettings>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
                    chair.transform.parent = SpawnPassengers.Instance.tiles[x, y].transform;
                }
                else if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.PlatformTiles, tilePos))
                {
                    //Instantiate(platformTile, new Vector3(GetTileCenter(x, y).x, yOffsetFloorTile, GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));

                    if (!GetComponent<BoardData>().IsMatchingTileSet(TileSetType.OccupiedTiles, tilePos))
                    {
                        GameObject pt = Instantiate(platformTile, new Vector3(GetComponent<TileSettings>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<TileSettings>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
                        platformTilePrefabs.Add(pt);
                    }
                }
                else
                {
                    if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.OccupiedTiles, tilePos))
                    {
                        Debug.Log("Didnt do it");
                    }
                    else
                    {
                        Instantiate(trainTile, new Vector3(GetComponent<TileSettings>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<TileSettings>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
                    }
                }
            }
        }

        SpawnPassengers.Instance.tiles[0, 0].SetActive(false); // NOTE: TO REMOVE THAT FREAKING 0, 0 THATS NOT BEING ASSIGNED AS UNAVAILABLE TILE LIKE OMG
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        float gapOffsetX = x * (tileSize + GetComponent<TileSettings>().gapSize);
        float gapOffsetY = y * (tileSize + GetComponent<TileSettings>().gapSize);

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(gapOffsetX, GetComponent<TileSettings>().yOffset, gapOffsetY) - GetComponent<TileSettings>().bounds;
        vertices[1] = new Vector3(gapOffsetX, GetComponent<TileSettings>().yOffset, gapOffsetY + tileSize) - GetComponent<TileSettings>().bounds;
        vertices[2] = new Vector3(gapOffsetX + tileSize, GetComponent<TileSettings>().yOffset, gapOffsetY) - GetComponent<TileSettings>().bounds;
        vertices[3] = new Vector3(gapOffsetX + tileSize, GetComponent<TileSettings>().yOffset, gapOffsetY + tileSize) - GetComponent<TileSettings>().bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        BoxCollider boxCollider = tileObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        return tileObject;
    }

    public void CreateMovableTiles()
    {
        for (int i = 0; i < GetComponent<Board>().availableMoves.Count; i++)
        {
            if (SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Unavailable") || SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Occupied"))
            {
                continue; //Skips the tile that is unavailable and continues with the rest
            }

            SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer = LayerMask.NameToLayer("MovableSpot");

            GetComponent<ChairModifier>().ChangeChairColor(GetComponent<Board>().availableMoves[i], GetComponent<ChairModifier>().highlightMaterial.color);
        }
    }
    public void RemoveMovableTiles()
    {
        for (int i = 0; i < GetComponent<Board>().availableMoves.Count; i++)
        {
            if (SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Unavailable") || SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Occupied"))
            {
                continue; //Skips the tile that is unavailable and continues with the rest
            }

            SpawnPassengers.Instance.tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

            GetComponent<ChairModifier>().TurnChairBackToOriginalColor(GetComponent<Board>().availableMoves[i]);
        }

        GetComponent<Board>().availableMoves.Clear();
    }

    public void DisablePlatformTiles()
    {
        foreach (var tile in platformTiles)
        {
            tile.layer = LayerMask.NameToLayer("Unavailable");
        }

        foreach (var pt in platformTilePrefabs)
        {
            pt.GetComponent<MeshRenderer>().enabled = false;
        }

        //Just a backwards count of passengers inside spawnedPassengers list, to remove them if they were destroyed
        for (int i = SpawnPassengers.Instance.spawnedPassengers.Count - 1; i >= 0; i--)
        {
            SpawnPassengers.Instance.spawnedPassengers[i].CheckPosition();
        }

        /*        foreach (var passenger in spawnedPassengers)
                {
                    passenger.CheckPosition();
                }*/

        SpawnPassengers.Instance.passengersSpawned = false;
    }

    public void EnablePlatformTiles()
    {
        foreach (var tile in platformTiles)
        {
            tile.layer = LayerMask.NameToLayer("Tile");
        }

        foreach (var pt in platformTilePrefabs)
        {
            pt.GetComponent<MeshRenderer>().enabled = true;
        }

        SpawnPassengers.Instance.SpawnAllPieces();

        SpawnPassengers.Instance.PositionAllPieces();

        SpawnPassengers.Instance.passengersSpawned = true;
    }

    public Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < SpawnPassengers.Instance.tileCountX; x++)
        {
            for (int y = 0; y < SpawnPassengers.Instance.tileCountY; y++)
            {
                if (SpawnPassengers.Instance.tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one; //Invalid
    }
}
