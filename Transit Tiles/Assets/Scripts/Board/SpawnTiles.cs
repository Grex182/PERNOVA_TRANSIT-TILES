using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTiles : MonoBehaviour
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
        GetComponent<SpawnPassengers>().yOffset += transform.position.y;
        GetComponent<SpawnPassengers>().bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + GetComponent<Board>().boardCenter;

        GetComponent<SpawnPassengers>().tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);
                GetComponent<SpawnPassengers>().tiles[x, y] = GenerateSingleTile(tileSize, x, y);

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.OccupiedTiles, tilePos))
                {
                    GetComponent<SpawnPassengers>().tiles[x, y].layer = LayerMask.NameToLayer("Unavailable");
                }

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.TaggedTrainTiles, tilePos))
                {
                    GetComponent<SpawnPassengers>().tiles[x, y].tag = "TrainTile";
                }
                else if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.TaggedPlatformTiles, tilePos))
                {
                    GetComponent<SpawnPassengers>().tiles[x, y].tag = "PlatformTile";
                    platformTiles.Add(GetComponent<SpawnPassengers>().tiles[x, y]);
                }

                if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.ChairTiles, tilePos))
                {
                    GetComponent<SpawnPassengers>().tiles[x, y].tag = "ChairTile";

                    GameObject chair = Instantiate(chairTile, new Vector3(GetComponent<SpawnPassengers>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<SpawnPassengers>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
                    chair.transform.parent = GetComponent<SpawnPassengers>().tiles[x, y].transform;
                }
                else if (GetComponent<BoardData>().IsMatchingTileSet(TileSetType.PlatformTiles, tilePos))
                {
                    //Instantiate(platformTile, new Vector3(GetTileCenter(x, y).x, yOffsetFloorTile, GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));

                    if (!GetComponent<BoardData>().IsMatchingTileSet(TileSetType.OccupiedTiles, tilePos))
                    {
                        GameObject pt = Instantiate(platformTile, new Vector3(GetComponent<SpawnPassengers>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<SpawnPassengers>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
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
                        Instantiate(trainTile, new Vector3(GetComponent<SpawnPassengers>().GetTileCenter(x, y).x, yOffsetFloorTile, GetComponent<SpawnPassengers>().GetTileCenter(x, y).z), Quaternion.Euler(-90, 0, 0));
                    }
                }
            }
        }

        GetComponent<SpawnPassengers>().tiles[0, 0].SetActive(false); //TO REMOVE THAT FREAKING 0, 0 THATS NOT BEING ASSIGNED AS UNAVAILABLE TILE LIKE OMG
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        float gapOffsetX = x * (tileSize + GetComponent<SpawnPassengers>().gapSize);
        float gapOffsetY = y * (tileSize + GetComponent<SpawnPassengers>().gapSize);

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(gapOffsetX, GetComponent<SpawnPassengers>().yOffset, gapOffsetY) - GetComponent<SpawnPassengers>().bounds;
        vertices[1] = new Vector3(gapOffsetX, GetComponent<SpawnPassengers>().yOffset, gapOffsetY + tileSize) - GetComponent<SpawnPassengers>().bounds;
        vertices[2] = new Vector3(gapOffsetX + tileSize, GetComponent<SpawnPassengers>().yOffset, gapOffsetY) - GetComponent<SpawnPassengers>().bounds;
        vertices[3] = new Vector3(gapOffsetX + tileSize, GetComponent<SpawnPassengers>().yOffset, gapOffsetY + tileSize) - GetComponent<SpawnPassengers>().bounds;

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
            if (GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Unavailable") || GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Occupied"))
            {
                continue; //Skips the tile that is unavailable and continues with the rest
            }

            GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer = LayerMask.NameToLayer("MovableSpot");

            GetComponent<ChairModifier>().ChangeChairColor(GetComponent<Board>().availableMoves[i], GetComponent<ChairModifier>().highlightMaterial.color);
        }
    }
    public void RemoveMovableTiles()
    {
        for (int i = 0; i < GetComponent<Board>().availableMoves.Count; i++)
        {
            if (GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Unavailable") || GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer == LayerMask.NameToLayer("Occupied"))
            {
                continue; //Skips the tile that is unavailable and continues with the rest
            }

            GetComponent<SpawnPassengers>().tiles[GetComponent<Board>().availableMoves[i].x, GetComponent<Board>().availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

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
        for (int i = GetComponent<SpawnPassengers>().spawnedPassengers.Count - 1; i >= 0; i--)
        {
            GetComponent<SpawnPassengers>().spawnedPassengers[i].CheckPosition();
        }

        /*        foreach (var passenger in spawnedPassengers)
                {
                    passenger.CheckPosition();
                }*/

        GameManager.instance.StationManager.hasPassengersSpawned = false;
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

        GetComponent<SpawnPassengers>().SpawnAllPieces();

        GetComponent<SpawnPassengers>().PositionAllPieces();

        GameManager.instance.StationManager.hasPassengersSpawned = true;
    }

    public Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < GetComponent<SpawnPassengers>().tileCountX; x++)
        {
            for (int y = 0; y < GetComponent<SpawnPassengers>().tileCountY; y++)
            {
                if (GetComponent<SpawnPassengers>().tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one; //Invalid
    }
}
