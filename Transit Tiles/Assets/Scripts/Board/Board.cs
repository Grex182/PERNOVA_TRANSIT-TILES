using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Train = 0,
    Chair = 1,
    Platform = 2
}

public enum BoardType
{
    MainBoard,
    StationBoard
}

public class Board : Singleton<Board>
{
    [SerializeField] public BoardType boardType;

    [Header("Art")]
    [SerializeField] private float dragOffset = 1.25f;

    [Header("Tile Settings")]
    [SerializeField] public Vector3 boardCenter = Vector3.zero;
    
    [Header("Lists")]
    [SerializeField] private Passenger currentlyDragging;
    public List<Vector2Int> availableMoves = new List<Vector2Int>();
    private Camera currentCamera;
    private Vector2Int currentHover;

    [Header("References")]
    public SpawnTiles spawnTilesScript;
    public TileSettings tileSettingsScript;
    public ChairModifier chairModifierScript;

    private void Awake()
    {
        
    }

    private void Start()
    {
        InitializeBoard();

        if (GetComponent<SpawnTiles>() != null)
        {
            spawnTilesScript.GenerateAllTiles(tileSettingsScript.tileSize, SpawnPassengers.Instance.tileCountX, SpawnPassengers.Instance.tileCountY);
            SpawnPassengers.Instance.SpawnAllPieces();
            SpawnPassengers.Instance.PositionAllPieces();
        }

        if (boardType == BoardType.MainBoard)
        {
            GameManager.Instance.Board = this;
        }
    }

    private void InitializeBoard()
    {
        spawnTilesScript = GetComponent<SpawnTiles>();
        tileSettingsScript = GetComponent<TileSettings>();
        chairModifierScript = GetComponent<ChairModifier>();
    }

    private void Update()
    {
        if (boardType == BoardType.MainBoard)
        {
            //For putting in the highlighting part of the board
            if (!currentCamera)
            {
                currentCamera = Camera.main;
                return;
            }

            RaycastHit info;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, 500, LayerMask.GetMask("Tile", "Hover", "MovableSpot", "Occupied")))
            {
                //Get the indexes of the tile the player hits
                Vector2Int hitPosition = GetComponent<SpawnTiles>().LookupTileIndex(info.transform.gameObject);

                //If hovering over tile after not hovering any tiles
                if (currentHover == -Vector2Int.one)
                {
                    currentHover = hitPosition;

                    if (SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer == LayerMask.NameToLayer("Occupied") && currentlyDragging != null/* && passengers[currentHover.x, currentHover.y] == null*//* && passengers[currentHover.x, currentHover.y] == null*/) //uncomment the && part if tile under passenger should turn green and not stay red
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Occupied");
                        GetComponent<ChairModifier>().ChangeChairColor(currentHover, GetComponent<ChairModifier>().occupiedMaterial.color);
                    }
                    /*
                    else if (SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer == LayerMask.NameToLayer("Occupied") && SpawnPassengers.Instance.passengers[hitPosition.x, hitPosition.y] == null)
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Occupied");
                    }
                    */
                    else if (SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].tag == "PlatformTile" && StationManager.Instance.isTrainMoving)
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Unavailable");
                    }
                    else
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");

                        GetComponent<ChairModifier>().HoverChairColor(hitPosition);
                    }
                }

                //If already hovering tile, change previous one
                if (currentHover != hitPosition)
                {
                    if (ContainsValidMove(ref availableMoves, currentHover))
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("MovableSpot");

                        GetComponent<ChairModifier>().ChangeChairColor(currentHover, GetComponent<ChairModifier>().highlightMaterial.color);
                    }
                    else if (SpawnPassengers.Instance.passengers[currentHover.x, currentHover.y] != null || SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer == LayerMask.NameToLayer("Occupied"))
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Occupied");
                        GetComponent<ChairModifier>().ChangeChairColor(currentHover, GetComponent<ChairModifier>().occupiedMaterial.color);
                        //GetComponent<ChairModifier>().TurnChairBackToOriginalColor(currentHover); //for chair to go back to original color
                    }
/*                    else if (GetComponent<SpawnPassengers>().tiles[currentHover.x, currentHover.y].layer == LayerMask.NameToLayer("Occupied") && GetComponent<SpawnPassengers>().passengers[currentHover.x, currentHover.y] == null) //for bulky spot of bulky passengers
                    {
                        GetComponent<SpawnPassengers>().tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Occupied");
                    }*/
                    else if (SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer == LayerMask.NameToLayer("Unavailable") && StationManager.Instance.isTrainMoving)
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Unavailable");
                    }
                    else
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");

                        GetComponent<ChairModifier>().TurnChairBackToOriginalColor(currentHover);
                    }

                    currentHover = hitPosition;

                    if (SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer == LayerMask.NameToLayer("Occupied") && currentlyDragging != null/* && passengers[currentHover.x, currentHover.y] == null*//* && passengers[currentHover.x, currentHover.y] == null*/) //uncomment the && part if tile under passenger should turn green and not stay red
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Occupied");

                        GetComponent<ChairModifier>().ChangeChairColor(currentHover, GetComponent<ChairModifier>().occupiedMaterial.color);
                    }
/*                    else if (GetComponent<SpawnPassengers>().tiles[hitPosition.x, hitPosition.y].layer == LayerMask.NameToLayer("Occupied") && GetComponent<SpawnPassengers>().passengers[hitPosition.x, hitPosition.y] == null)
                    {
                        GetComponent<SpawnPassengers>().tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Occupied");
                    }*/
                    else if (StationManager.Instance.isTrainMoving)
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Unavailable");
                    }
                    else
                    {
                        SpawnPassengers.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");

                        GetComponent<ChairModifier>().HoverChairColor(hitPosition);
                    }
                }

                //If press down on mouse
                if (Input.GetMouseButtonDown(0))
                {
                    if (SpawnPassengers.Instance.passengers[hitPosition.x, hitPosition.y] != null)
                    {
                        currentlyDragging = SpawnPassengers.Instance.passengers[hitPosition.x, hitPosition.y];

                        currentlyDragging.PassengerSelected();

                        //Get list of where passenger can go
                        availableMoves = currentlyDragging.GetAvailableMoves(ref SpawnPassengers.Instance.passengers, SpawnPassengers.Instance.tileCountX, SpawnPassengers.Instance.tileCountY);
                        GetComponent<SpawnTiles>().CreateMovableTiles();
                    }
                }

                //If releasing mouse button
                if (currentlyDragging != null && Input.GetMouseButtonUp(0))
                {
                    Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                    SpawnPassengers.Instance.tiles[previousPosition.x, previousPosition.y].layer = LayerMask.NameToLayer("Tile");

                    GetComponent<ChairModifier>().TurnChairBackToOriginalColor(previousPosition);

                    bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);

                    /*                if (currentlyDragging.type == PassengerType.Bulky)
                                    {
                                        tiles[previousPosition.x - 1, previousPosition.y].layer = LayerMask.NameToLayer("Tile");

                                        if (currentlyDragging.currentX - 1 >= 0 && currentlyDragging.currentX - 1 < tiles.GetLength(0) && currentlyDragging.currentY >= 0 && currentlyDragging.currentY < tiles.GetLength(1))
                                        {
                                            tiles[currentlyDragging.currentX - 1, currentlyDragging.currentY].layer = LayerMask.NameToLayer("Occupied");
                                        }
                                        else
                                        {
                                            validMove = false;
                                        }
                                    }*/

                    //go back to previous position
                    if (!validMove)
                    {
                        /*                    if (currentlyDragging.type == PassengerType.Bulky)
                                            {
                                                currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                                                tiles[previousPosition.x, previousPosition.y].layer = LayerMask.NameToLayer("Occupied");
                                                tiles[previousPosition.x - 1, previousPosition.y].layer = LayerMask.NameToLayer("Occupied");
                                                tiles[currentlyDragging.currentX, currentlyDragging.currentY].layer = LayerMask.NameToLayer("Tile");
                                                Debug.Log("Changed the position of bulky person back to previous position");
                                            }*/

                        currentlyDragging.SetPosition(GetComponent<TileSettings>().GetTileCenter(previousPosition.x, previousPosition.y));
                        SpawnPassengers.Instance.tiles[previousPosition.x, previousPosition.y].layer = LayerMask.NameToLayer("Occupied");
                        GetComponent<ChairModifier>().ChangeChairColor(previousPosition, GetComponent<ChairModifier>().occupiedMaterial.color);
                    }
                    else if (validMove)
                    {
                        SpawnPassengers.Instance.tiles[currentlyDragging.currentX, currentlyDragging.currentY].layer = LayerMask.NameToLayer("Unavailable"); //WHY DOES THIS WORK CHECK CHECK CHECK

                        GetComponent<ChairModifier>().ChangeChairColor(currentHover, GetComponent<ChairModifier>().hoverMaterial.color);
                    }

                    GetComponent<SpawnTiles>().RemoveMovableTiles();

                    currentlyDragging.PassengerDropped();

                    currentlyDragging = null;
                }
            }
            else
            {
                //CHECK
                //Removes the movabletiles from train if player is still holding on to a passenger
                if (currentlyDragging == null || (!currentlyDragging && currentHover != -Vector2Int.one && StationManager.Instance.isTrainMoving))
                {
                    currentlyDragging = null;
                    GetComponent<SpawnTiles>().RemoveMovableTiles();
                }

                //if going out of bounds, change previous tile
                if (currentHover != -Vector2Int.one)
                {
                    if (ContainsValidMove(ref availableMoves, currentHover))
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("MovableSpot");
                    }
                    else if (SpawnPassengers.Instance.passengers[currentHover.x, currentHover.y] != null)
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Occupied");
                    }
                    else if (SpawnPassengers.Instance.passengers[currentHover.x, currentHover.y] == null && SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer == LayerMask.NameToLayer("Occupied"))
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Occupied");
                    }
                    else
                    {
                        SpawnPassengers.Instance.tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");

                        GetComponent<ChairModifier>().TurnChairBackToOriginalColor(currentHover);
                    }

                    currentHover = -Vector2Int.one;
                }

                if (currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    currentlyDragging.SetPosition(GetComponent<TileSettings>().GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));

                    currentlyDragging.PassengerDropped();
                    currentlyDragging = null;
                    GetComponent<SpawnTiles>().RemoveMovableTiles();
                }
            }

            //IF dragging a piece
            if (currentlyDragging)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * GetComponent<TileSettings>().yOffset);
                float distance = 0.0f;
                if (horizontalPlane.Raycast(ray, out distance))
                    currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);

                /*            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                            float distance = 0.0f;
                            if (horizontalPlane.Raycast(ray, out distance))
                            {
                                RaycastHit hit;
                                Ray hoverRay = currentCamera.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(hoverRay, out hit, 100, LayerMask.GetMask("MovableSpot")))
                                {
                                    Vector2Int movePos = LookupTileIndex(hit.transform.gameObject);

                                    // Snap to center of the valid MovableSpot tile
                                    currentlyDragging.SetPosition(GetTileCenter(movePos.x, movePos.y) + Vector3.up * dragOffset);
                                }
                                else
                                {
                                    // Optionally keep the piece at its original position if not over MovableSpot
                                    currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY) + Vector3.up * dragOffset);
                                }
                            }*/
            }
        }
    }

    private bool MoveTo(Passenger passenger, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2(x, y)))
        {
            return false;
        }

        //Block movement if the tile's layer is "Unavailable"
        if (SpawnPassengers.Instance.tiles[x, y].layer == LayerMask.NameToLayer("Unavailable") || SpawnPassengers.Instance.tiles[x, y].layer == LayerMask.NameToLayer("Occupied"))
        {
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(passenger.currentX, passenger.currentY);

        //Is there another piece on target position?
        if (SpawnPassengers.Instance.passengers[x, y] != null)
        {
            //op means other passenger
            Passenger op = SpawnPassengers.Instance.passengers[x, y];

            return false;
        }

        SpawnPassengers.Instance.passengers[x, y] = passenger;
        SpawnPassengers.Instance.passengers[previousPosition.x, previousPosition.y] = null;

        SpawnPassengers.Instance.PositionSinglePiece(x, y);

        return true;
    }

    //Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }

    //Possible Debug stuff (Might need to move to GameManager?)
    // Eli comment: Initialization stuff should be in game manager under GameInit 
    // For example: make Initialize() function in this script, in gamemanager, call Board.Instance.Initialize()
    // Grex: Some of the commented stuff are for bulky
}