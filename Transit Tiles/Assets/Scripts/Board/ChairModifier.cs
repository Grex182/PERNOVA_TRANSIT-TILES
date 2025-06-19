using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChairModifier : MonoBehaviour
{
    [Header("Colors & Materials")]
    [SerializeField] private Color originalChairColor;
    [SerializeField] public Material hoverMaterial;
    [SerializeField] public Material highlightMaterial;
    [SerializeField] public Material occupiedMaterial;

    private Dictionary<Vector2Int, MeshRenderer> cachedSeats = new();

    private void Start()
    {
        if (GetComponent<Board>().GetComponent<SpawnTiles>().chairTile != null)
        {
            originalChairColor = GetComponent<Board>().GetComponent<SpawnTiles>().chairTile.GetComponent<MeshRenderer>().sharedMaterials[1].color;
        }
        else
        {
            Debug.Log("Nope nothing");
        }
    }

    public void TurnChairBackToOriginalColor(Vector2Int position)
    {
        if (GameManager.instance.Board.GetComponent<SpawnPassengers>().tiles[position.x, position.y].tag == "ChairTile")
        {
            Transform seat = GameManager.instance.Board.GetComponent<SpawnPassengers>().tiles[position.x, position.y].transform.Find("TileSeat(Clone)");
            if (seat != null)
            {
                MeshRenderer renderer = seat.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    renderer.materials[1].color = originalChairColor;
                }
            }
        }
    }

    public void HoverChairColor(Vector2Int position)
    {
        if (GameManager.instance.Board.GetComponent<SpawnPassengers>().tiles[position.x, position.y].tag == "ChairTile")
        {
            var renderer = GetSeatRenderer(position);
            if (renderer != null && renderer.materials.Length > 1)
            {
                //originalChairColor = renderer.materials[1].color;
                renderer.materials[1].color = hoverMaterial.color;
            }
        }
    }

    public void ChangeChairColor(Vector2Int position, Color color)
    {
        if (GameManager.instance.Board.GetComponent<SpawnPassengers>().tiles[position.x, position.y].tag == "ChairTile")
        {
            var renderer = GetSeatRenderer(position);
            if (renderer != null && renderer.materials.Length > 1)
            {
                //NEED TO ADD AN IF STATEMENT HERE FOR THE originalChairColor thing so that it checks if its already their so that it wont be changing everytime its called
                //originalChairColor = renderer.materials[1].color;
                renderer.materials[1].color = color;
            }
            /*            Transform seat = tiles[position.x, position.y].transform.Find("TileSeat(Clone)/Tile_Seat");
                        if (seat != null)
                        {
                            MeshRenderer renderer = seat.GetComponent<MeshRenderer>();

                            originalChairColor = renderer.materials[1].color;

                            if (renderer != null && renderer.materials.Length > 1)
                            {
                                renderer.materials[1].color = color;//new Color32(111, 164, 58, 255);
                            }
                        }*/
        }
    }

    private MeshRenderer GetSeatRenderer(Vector2Int pos)
    {
        if (!cachedSeats.TryGetValue(pos, out var renderer))
        {
            var seat = GameManager.instance.Board.GetComponent<SpawnPassengers>().tiles[pos.x, pos.y].transform.Find("TileSeat(Clone)");
            if (seat != null)
            {
                renderer = seat.GetComponent<MeshRenderer>();
                cachedSeats[pos] = renderer;
            }
        }
        return renderer;
    }
}
