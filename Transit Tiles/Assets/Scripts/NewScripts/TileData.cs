using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileTypes
{
    Train,
    Seat,
    Station
}

public class TileData : MonoBehaviour
{
    [SerializeField] private TileTypes _tileType;
    [SerializeField] private bool _isBottomSection;
    [SerializeField] private bool _isExitLane;

    public TileTypes TileType => _tileType;
    public bool IsBottomSection => _isBottomSection;
    public bool IsExitLane => _isExitLane;
    public bool IsVacant { get; internal set; } = true;
    public bool IsDirty { get; internal set; } = false;

    public Vector2 GridPos { get; private set; }
    
    //Not sure if I am still using this
    public class Tile
    {
        public TileTypes TileType { get; private set; }
        public Vector2 GridPosition { get; private set; }

        // Bool properties

        public bool IsBottomSection { get; private set; }
        public bool IsExitLane { get; set; }
        public bool IsVacant { get; set; }
        public bool IsDirty { get; set; }

        #region SET BASE
        public Tile(TileTypes tileType, Vector2 gridPosition, bool isBottomSection, bool isExitLane, bool isVacant = true, bool isDirty = false)
        {
            TileType = tileType;
            GridPosition = gridPosition;

            IsBottomSection = isBottomSection;
            IsExitLane = isExitLane;
            IsVacant = isVacant;
            IsDirty = isDirty;
        }
        #endregion
    }
    
    public void SetTilePos()
    {
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        GridPos = gridPos;
    }
    
    public Tile GetTile()
    {
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        GridPos = gridPos;
        return new Tile(TileType ,gridPos, IsBottomSection, IsExitLane, IsVacant, IsDirty);
    }
    
}
