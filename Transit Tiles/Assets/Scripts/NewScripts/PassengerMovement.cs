using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class PassengerMovement : MonoBehaviour
{
    [SerializeField] BoardManager boardManager;

    [SerializeField] private GameObject trainParent;
    [SerializeField] private GameObject stationParent;

    private GameObject selectedObject = null;
    private GameObject selectedCollision = null;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float yOffset;
    [SerializeField] private float _dragBuffer;
    [SerializeField] public Vector3 MouseDragPos;
    [SerializeField] private Vector3Int directionInput;


    [SerializeField] private bool isFar = false;

    // Update is called once per frame
    void Update()
    {
        

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedObject == null)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 500))
                {
                    if (hit.collider.CompareTag("Drag"))
                    {
                        selectedObject = hit.collider.gameObject;
                        selectedCollision = selectedObject.GetComponent<PassengerData>().collision;
                        //selectedObject.gameObject.tag = "Drag";
                        Debug.Log("selected object" + selectedObject);
                        
                    }
                }


            }
        }

        if (selectedObject != null)
        {
            //Let Go
            if (Input.GetMouseButtonUp(0))
            {
                //selectedObject.gameObject.tag = "Static";
                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, 0, selectedObject.transform.position.z);
                selectedObject = null;
                selectedCollision = null;
            }
            else
            {
                //While selectedObject
                Vector3 newPosition = GetMouseWorldPosition();
                newPosition.y = yOffset;
                MouseDragPos = newPosition;
                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, yOffset, selectedObject.transform.position.z);
                directionInput = GetArrowKeyLikeDirection(MouseDragPos, selectedObject.transform.position);
            }

            if (Input.GetMouseButtonDown(1))
            {
                selectedObject.transform.Rotate(0f,90f,0f);
                if (!ValidMove(selectedCollision))
                {
                    selectedObject.transform.position -= selectedObject.transform.forward;
                    if (!ValidMove(selectedCollision))
                    {
                        selectedObject.transform.position += selectedObject.transform.forward;
                        selectedObject.transform.Rotate(0f, -90f, 0f);
                    }
                        
                }
            }


            //Movement Code
            if (isFar)
            {
                Vector3Int pos = new Vector3Int(
                    Mathf.RoundToInt(selectedObject.transform.position.x),
                    Mathf.RoundToInt(selectedObject.transform.position.y),
                    Mathf.RoundToInt(selectedObject.transform.position.z));

                Vector3Int moveToTile = pos + directionInput;

                selectedObject.transform.position = boardManager.grid[moveToTile.x, moveToTile.z].transform.position + Vector3.up * yOffset;
                if (!ValidMove(selectedCollision))
                {
                    selectedObject.transform.position = pos;
                }
                else
                {
                    TileTypes _type = boardManager.grid[moveToTile.x, moveToTile.z].GetComponent<TileData>().tileType;
                    selectedObject.GetComponent<PassengerData>().currTile = _type;



                    switch (_type)
                    {
                        case TileTypes.Station:
                            setParent(selectedObject, stationParent);
                            break;
                        case TileTypes.Seat:
                        case TileTypes.Train:
                            setParent(selectedObject, trainParent);

                            break;
                    }
                }
            }
        }
    }

    bool ValidMove(GameObject collision)
    {
        foreach(Transform children in collision.transform)
        {
            int moveX = Mathf.RoundToInt(children.transform.position.x);
            int moveZ = Mathf.RoundToInt(children.transform.position.z);

            bool bounds = moveX < 0 || moveX >= boardManager.grid.GetLength(0) 
                       || moveZ < 0 || moveZ >= boardManager.grid.GetLength(1);
            bool tileExists = boardManager.grid[moveX, moveZ] == null;
            bool tileVacant = !boardManager.grid[moveX, moveZ].GetComponent<TileData>().isVacant;

            if (bounds || tileExists || tileVacant) 
            {
                Debug.Log($"bounds: {bounds} exists: {tileExists}  isVacant {tileVacant}");
                return false;
            }
        }

        return true;
    }

    private void setParent(GameObject child, GameObject newParent)
    {
        child.transform.SetParent(newParent.transform, true);
    }
    // Helper method to get mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(selectedObject.transform.position).z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    public Vector3Int GetArrowKeyLikeDirection(Vector3 mouseDragPos, Vector3 objectPos)
    {
        // Calculate direction (XZ only)
        Vector3 distance = mouseDragPos - objectPos;
        isFar = Mathf.Abs(distance.x) > _dragBuffer || Mathf.Abs(distance.z) > _dragBuffer;

        
        distance.y = 0;
        Vector3 direction = distance;
        if (direction.magnitude < 0.1f) // Dead zone to prevent tiny movements
            return Vector3Int.zero;
        

        direction.Normalize();

        


        // Compare absolute X and Z to find the dominant axis
        bool xDominates = Mathf.Abs(direction.x) > Mathf.Abs(direction.z);

        // Snap to the closest cardinal direction (like arrow keys)
        if (xDominates)
        {
            return new Vector3Int(
                direction.x > 0 ? 1 : -1,
                0,0
            );
        }
        else
        {
            return new Vector3Int(
                0,0,
                direction.z > 0 ? 1 : -1
            );
        }
    }
}
