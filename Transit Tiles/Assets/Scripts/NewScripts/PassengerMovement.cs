using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerMovement : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] BoardManager boardManager;
    [SerializeField] private SelectableOutline selectableOutline;
    [SerializeField] private Animator currAnimator;

    [SerializeField] private GameObject trainParent;
    [SerializeField] private GameObject stationParent;

    public GameObject selectedObject = null;
    private GameObject selectedCollision = null;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float yOffset;
    [SerializeField] private float _dragBuffer;
    [SerializeField] public Vector3 MouseDragPos;
    [SerializeField] private Vector3Int directionInput;

    private RaycastHit hit;

    [SerializeField] private bool isFar = false;

    // Update is called once per frame
    void Update()
    {
        if (UiManager.Instance.isPaused || LevelManager.Instance.currState == MovementState.Card) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedObject == null)
            {
                SelectObject();
            }
            else
            {
                DeselectObject();
            }
        }

        if (selectedObject == null) return;

        HandleDragging();
        HandleRotation();

        //Movement Code
        if (isFar)
        {
            HandleMovement();
        }
    }

    private void SelectObject()
    {
        if (selectedObject != null) { return; }

        int layerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));

        //RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 500, layerMask) && hit.collider.CompareTag("Drag"))
        {
            selectedObject = hit.collider.gameObject;
            selectedCollision = selectedObject.GetComponent<PassengerData>().movementCollision;

            // Passenger Outline
            selectedObject.GetComponent<SelectableOutline>().SetHasSelected(true);
            selectedObject.GetComponent<SelectableOutline>().SetOutline(true);

            if (selectedObject.GetComponent<PassengerUI>() != null)
            {
                selectedObject.GetComponent<PassengerUI>().SetMoodletState(true);
            }

            currAnimator = selectedObject.GetComponent<Animator>();

            currAnimator.SetBool("IsSitting", false);
            currAnimator.SetBool("IsSelected", true);
        }
    }

    private void DeselectObject()
    {
        currAnimator.SetBool("IsSelected", false);

        // Passenger Outline
        selectedObject.GetComponent<SelectableOutline>().SetHasSelected(false);
        selectedObject.GetComponent<SelectableOutline>().SetOutline(false);

        // Reset selectedObject
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, 0, selectedObject.transform.position.z);

        PassengerUI selectedUI = selectedObject.GetComponent<PassengerUI>();
        if (selectedUI != null && !selectedUI.animationActive)
        {
            selectedUI.SetMoodletState(false);
        }

        selectedObject = null;
        selectedCollision = null;
        currAnimator = null;
    }

    private void HandleDragging()
    {
        //While selectedObject
        Vector3 newPosition = GetMouseWorldPosition();
        newPosition.y = yOffset;
        MouseDragPos = newPosition;
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, yOffset, selectedObject.transform.position.z);
        directionInput = GetArrowKeyLikeDirection(MouseDragPos, selectedObject.transform.position);

    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.R)) // Rotate
        {
            selectedObject.transform.Rotate(0f, 90f, 0f); //Rotate 90 degrees

            GameObject _charModel = selectedObject.GetComponent<PassengerData>().model;
            _charModel.transform.Rotate(0f, -90f, 0f, Space.Self);

            if (!ValidMove(selectedCollision)) // Check if invalid move
            {
                selectedObject.transform.position -= selectedObject.transform.forward; //Try moving backwards one tile
                _charModel.transform.localPosition += selectedObject.transform.forward;
                if (!ValidMove(selectedCollision)) // Check if invalid move again
                {
                    //If invalid, move forward one tile and rotate back
                    selectedObject.transform.position += selectedObject.transform.forward;
                    _charModel.transform.localPosition -= selectedObject.transform.forward;
                    selectedObject.transform.Rotate(0f, -90f, 0f);
                    _charModel.transform.Rotate(0f, 90f, 0f, Space.Self);
                }
            }
        }
    }

    private void HandleMovement()
    {
        Vector3Int pos = new Vector3Int(
                        Mathf.RoundToInt(selectedObject.transform.position.x),
                        Mathf.RoundToInt(selectedObject.transform.position.y),
                        Mathf.RoundToInt(selectedObject.transform.position.z));

        Vector3Int moveToTile = pos + directionInput;

        selectedObject.transform.position = boardManager.grid[moveToTile.x, moveToTile.z].transform.position + Vector3.up * yOffset;
        GameObject _charModel = selectedObject.GetComponent<PassengerData>().model;
        _charModel.transform.localPosition -= directionInput;

        if (!ValidMove(selectedCollision))
        {
            selectedObject.transform.position = pos;
            _charModel.transform.localPosition += directionInput;
        }
        else
        {
            PassengerData currPassenegrData = selectedObject.GetComponent<PassengerData>();
            TileData _tileData = boardManager.grid[moveToTile.x, moveToTile.z].GetComponent<TileData>();
            TileTypes _type = _tileData.tileType;

            currPassenegrData.currTile = _type;

            switch (_type)
            {
                case TileTypes.Station:
                    currPassenegrData.isSitting = false;
                    //SetParent(selectedObject, stationParent);
                    PassengerData _data = selectedObject.GetComponent<PassengerData>();
                    
                    if (_data.transform.parent.gameObject == trainParent)
                    {
                        _data.ScorePassenger();

                        // Passenger Outline
                        selectedObject.GetComponent<SelectableOutline>().SetHasSelected(false);
                        selectedObject.GetComponent<SelectableOutline>().SetOutline(false);

                        Destroy(selectedObject);
                    }
                        break;

                case TileTypes.Seat:
                    currPassenegrData.isSitting = true;
                    
                    selectedObject.GetComponent<PassengerData>().isBottomSection = _tileData.isBottomSection;
                    

                    
                    SetParent(selectedObject, trainParent);
                    break;

                case TileTypes.Train:
                    currPassenegrData.isSitting = false;
                    SetParent(selectedObject, trainParent);

                    break;
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
                return false;
            }
        }

        return true;
    }

    private void SetParent(GameObject child, GameObject newParent)
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
