using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileSetType
{
    OccupiedTiles,
    TaggedTrainTiles,
    TaggedPlatformTiles,
    ChairTiles,
    PlatformTiles
}

public class BoardData : MonoBehaviour
{
    private TileSetType tileSetType;

    //Spawns unavailable tiles
    private readonly HashSet<Vector2Int> occupiedTilesAtStart = new HashSet<Vector2Int>
    {
        new Vector2Int(11, 0),  new Vector2Int(11, 1), new Vector2Int(11, 2),  new Vector2Int(11, 3), new Vector2Int(11, 4),    new Vector2Int(17, 3),  new Vector2Int(17, 5),  new Vector2Int(5, 3),
        new Vector2Int(10, 5),  new Vector2Int(3, 4),  new Vector2Int(3, 5),   new Vector2Int(2, 5),  new Vector2Int(11, 5),    new Vector2Int(16, 3),  new Vector2Int(16, 5),  new Vector2Int(4, 3),
        new Vector2Int(9, 5),   new Vector2Int(2, 2),  new Vector2Int(2, 3),   new Vector2Int(2, 4),  new Vector2Int(1, 5),     new Vector2Int(15, 3),  new Vector2Int(15, 5),  new Vector2Int(6, 4),
        new Vector2Int(8, 5),   new Vector2Int(1, 2),  new Vector2Int(1, 3),   new Vector2Int(1, 4),  new Vector2Int(0, 5),     new Vector2Int(14, 3),  new Vector2Int(14, 5),  new Vector2Int(5, 4),
        new Vector2Int(3, 2),   new Vector2Int(3, 3),  new Vector2Int(0, 2),   new Vector2Int(0, 3),  new Vector2Int(17, 2),    new Vector2Int(13, 3),  new Vector2Int(13, 5),  new Vector2Int(4, 4),
        new Vector2Int(6, 0),   new Vector2Int(6, 1),  new Vector2Int(17, 0),  new Vector2Int(17, 1), new Vector2Int(16, 2),    new Vector2Int(12, 3),  new Vector2Int(12, 5),  new Vector2Int(16, 10),
        new Vector2Int(5, 0),   new Vector2Int(5, 1),  new Vector2Int(16, 0),  new Vector2Int(16, 1), new Vector2Int(15, 2),    new Vector2Int(17, 4),  new Vector2Int(5, 5),   new Vector2Int(17, 10),
        new Vector2Int(4, 0),   new Vector2Int(4, 1),  new Vector2Int(15, 0),  new Vector2Int(15, 1), new Vector2Int(14, 2),    new Vector2Int(16, 4),  new Vector2Int(4, 5),   new Vector2Int(1, 10),
        new Vector2Int(3, 0),   new Vector2Int(3, 1),  new Vector2Int(14, 0),  new Vector2Int(14, 1), new Vector2Int(13, 2),    new Vector2Int(15, 4),  new Vector2Int(6, 2),   new Vector2Int(0, 10),
        new Vector2Int(2, 0),   new Vector2Int(2, 1),  new Vector2Int(13, 0),  new Vector2Int(13, 1), new Vector2Int(12, 2),    new Vector2Int(14, 4),  new Vector2Int(5, 2),
        new Vector2Int(1, 0),   new Vector2Int(1, 1),  new Vector2Int(12, 0),  new Vector2Int(12, 1), new Vector2Int(7, 5),     new Vector2Int(13, 4),  new Vector2Int(4, 2),
        new Vector2Int(0, 0),   new Vector2Int(0, 1),  new Vector2Int(0, 4),   new Vector2Int(0, 5),  new Vector2Int(6, 5),     new Vector2Int(12, 4),  new Vector2Int(6, 3),
    };

    //Apply TrainTiles tag on tiles at the start
    private readonly HashSet<Vector2Int> tagTrainTilesAtStart = new HashSet<Vector2Int>
    {
        new Vector2Int(11, 7),  new Vector2Int(11, 8),  new Vector2Int(11, 9),  new Vector2Int(11, 10), new Vector2Int(11, 11), new Vector2Int(10, 6),  new Vector2Int(16, 7),  new Vector2Int(12, 7),
        new Vector2Int(10, 7),  new Vector2Int(10, 8),  new Vector2Int(10, 9),  new Vector2Int(10, 10), new Vector2Int(10, 11), new Vector2Int(9, 6),   new Vector2Int(16, 8),  new Vector2Int(12, 8),
        new Vector2Int(9, 7),   new Vector2Int(9, 8),   new Vector2Int(9, 9),   new Vector2Int(9, 10),  new Vector2Int(9, 11),  new Vector2Int(8, 6),   new Vector2Int(16, 9),  new Vector2Int(12, 9),
        new Vector2Int(8, 7),   new Vector2Int(8, 8),   new Vector2Int(8, 9),   new Vector2Int(8, 10),  new Vector2Int(8, 11),  new Vector2Int(7, 6),   new Vector2Int(15, 7),  new Vector2Int(14, 10),
        new Vector2Int(7, 7),   new Vector2Int(7, 8),   new Vector2Int(7, 9),   new Vector2Int(7, 10),  new Vector2Int(7, 11),  new Vector2Int(15, 6),  new Vector2Int(15, 8),  new Vector2Int(13, 10),
        new Vector2Int(6, 7),   new Vector2Int(6, 8),   new Vector2Int(6, 9),   new Vector2Int(6, 10),  new Vector2Int(6, 11),  new Vector2Int(14, 6),  new Vector2Int(15, 9),  new Vector2Int(12, 10),
        new Vector2Int(5, 7),   new Vector2Int(5, 8),   new Vector2Int(5, 9),   new Vector2Int(5, 10),  new Vector2Int(5, 11),  new Vector2Int(13, 6),  new Vector2Int(14, 7),  new Vector2Int(1, 6),
        new Vector2Int(4, 7),   new Vector2Int(4, 8),   new Vector2Int(4, 9),   new Vector2Int(4, 10),  new Vector2Int(4, 11),  new Vector2Int(12, 6),  new Vector2Int(14, 8),  new Vector2Int(0, 6),
        new Vector2Int(3, 7),   new Vector2Int(3, 8),   new Vector2Int(3, 9),   new Vector2Int(3, 10),  new Vector2Int(3, 11),  new Vector2Int(17, 7),  new Vector2Int(14, 9),
        new Vector2Int(2, 7),   new Vector2Int(2, 8),   new Vector2Int(2, 9),   new Vector2Int(2, 10),  new Vector2Int(2, 11),  new Vector2Int(17, 8),  new Vector2Int(13, 7),
        new Vector2Int(1, 7),   new Vector2Int(1, 8),   new Vector2Int(1, 9),   new Vector2Int(1, 11),  new Vector2Int(17, 6),  new Vector2Int(17, 9),  new Vector2Int(13, 8),
        new Vector2Int(0, 7),   new Vector2Int(0, 8),   new Vector2Int(0, 9),   new Vector2Int(0, 11),  new Vector2Int(16, 6),  new Vector2Int(15, 10), new Vector2Int(13, 9),
    };

    private readonly HashSet<Vector2Int> tagPlatformTilesAtStart = new HashSet<Vector2Int>
    {
        new Vector2Int(9, 4),   new Vector2Int(10, 4),  new Vector2Int(8, 4),   new Vector2Int(7, 4),
        new Vector2Int(9, 3),   new Vector2Int(10, 3),  new Vector2Int(8, 3),   new Vector2Int(7, 3),
        new Vector2Int(9, 2),   new Vector2Int(10, 2),  new Vector2Int(8, 2),   new Vector2Int(7, 2),
        new Vector2Int(9, 1),   new Vector2Int(10, 1),  new Vector2Int(8, 1),   new Vector2Int(7, 1),
        new Vector2Int(9, 0),   new Vector2Int(10, 0),  new Vector2Int(8, 0),   new Vector2Int(7, 0),
    };

    private readonly HashSet<Vector2Int> chairTilesAtStart = new HashSet<Vector2Int>
    {
        new Vector2Int(15, 6),  new Vector2Int(6, 6),
        new Vector2Int(14, 6),  new Vector2Int(5, 6),
        new Vector2Int(13, 6),  new Vector2Int(4, 6),
        new Vector2Int(12, 6),  new Vector2Int(3, 6),
        new Vector2Int(11, 6),  new Vector2Int(2, 6),
    };

    private readonly HashSet<Vector2Int> platformTilesAtStart = new HashSet<Vector2Int>
    {
        new Vector2Int(17, 0),  new Vector2Int(17, 1),  new Vector2Int(17, 2),  new Vector2Int(17, 3),  new Vector2Int(17, 4),  new Vector2Int(17, 5),
        new Vector2Int(16, 0),  new Vector2Int(16, 1),  new Vector2Int(16, 2),  new Vector2Int(16, 3),  new Vector2Int(16, 4),  new Vector2Int(16, 5),
        new Vector2Int(15, 0),  new Vector2Int(15, 1),  new Vector2Int(15, 2),  new Vector2Int(15, 3),  new Vector2Int(15, 4),  new Vector2Int(15, 5),
        new Vector2Int(14, 0),  new Vector2Int(14, 1),  new Vector2Int(14, 2),  new Vector2Int(14, 3),  new Vector2Int(14, 4),  new Vector2Int(14, 5),
        new Vector2Int(13, 0),  new Vector2Int(13, 1),  new Vector2Int(13, 2),  new Vector2Int(13, 3),  new Vector2Int(13, 4),  new Vector2Int(13, 5),
        new Vector2Int(12, 0),  new Vector2Int(12, 1),  new Vector2Int(12, 2),  new Vector2Int(12, 3),  new Vector2Int(12, 4),  new Vector2Int(12, 5),
        new Vector2Int(11, 0),  new Vector2Int(11, 1),  new Vector2Int(11, 2),  new Vector2Int(11, 3),  new Vector2Int(11, 4),  new Vector2Int(11, 5),
        new Vector2Int(10, 0),  new Vector2Int(10, 1),  new Vector2Int(10, 2),  new Vector2Int(10, 3),  new Vector2Int(10, 4),  new Vector2Int(10, 5),
        new Vector2Int(9, 0),   new Vector2Int(9, 1),   new Vector2Int(9, 2),   new Vector2Int(9, 3),   new Vector2Int(9, 4),   new Vector2Int(9, 5),
        new Vector2Int(8, 0),   new Vector2Int(8, 1),   new Vector2Int(8, 2),   new Vector2Int(8, 3),   new Vector2Int(8, 4),   new Vector2Int(8, 5),
        new Vector2Int(7, 0),   new Vector2Int(7, 1),   new Vector2Int(7, 2),   new Vector2Int(7, 3),   new Vector2Int(7, 4),   new Vector2Int(7, 5),
        new Vector2Int(6, 0),   new Vector2Int(6, 1),   new Vector2Int(6, 2),   new Vector2Int(6, 3),   new Vector2Int(6, 4),   new Vector2Int(6, 5),
        new Vector2Int(5, 0),   new Vector2Int(5, 1),   new Vector2Int(5, 2),   new Vector2Int(5, 3),   new Vector2Int(5, 4),   new Vector2Int(5, 5),
        new Vector2Int(4, 0),   new Vector2Int(4, 1),   new Vector2Int(4, 2),   new Vector2Int(4, 3),   new Vector2Int(4, 4),   new Vector2Int(4, 5),
        new Vector2Int(3, 0),   new Vector2Int(3, 1),   new Vector2Int(3, 2),   new Vector2Int(3, 3),   new Vector2Int(3, 4),   new Vector2Int(3, 5),
        new Vector2Int(2, 0),   new Vector2Int(2, 1),   new Vector2Int(2, 2),   new Vector2Int(2, 3),   new Vector2Int(2, 4),   new Vector2Int(2, 5),
        new Vector2Int(1, 0),   new Vector2Int(1, 1),   new Vector2Int(1, 2),   new Vector2Int(1, 3),   new Vector2Int(1, 4),   new Vector2Int(1, 5),
        new Vector2Int(0, 0),   new Vector2Int(0, 1),   new Vector2Int(0, 2),   new Vector2Int(0, 3),   new Vector2Int(0, 4),   new Vector2Int(0, 5),
    };

    public bool IsMatchingTileSet(TileSetType type, Vector2Int position)
    {
        HashSet<Vector2Int> tileSet;

        switch (type)
        {
            case TileSetType.OccupiedTiles:
                tileSet = occupiedTilesAtStart;
                break;
            case TileSetType.TaggedTrainTiles:
                tileSet = tagTrainTilesAtStart;
                break;
            case TileSetType.TaggedPlatformTiles:
                tileSet = tagPlatformTilesAtStart;
                break;
            case TileSetType.ChairTiles:
                tileSet = chairTilesAtStart;
                break;
            case TileSetType.PlatformTiles:
                tileSet = platformTilesAtStart;
                break;
            default:
                Debug.LogWarning("Unrecognized TileSetType: " + type);
                return false;
        }

        return tileSet.Contains(position);
    }
}
