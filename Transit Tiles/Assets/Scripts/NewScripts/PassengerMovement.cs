using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerMovement : MonoBehaviour
{
    private GameObject selectedObject = null;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float yOffset;
    [SerializeField] private float _dragBuffer;
    [SerializeField] public Vector3 MouseDragPos;
    [SerializeField] private Vector2Int directionInput;


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
                        Debug.Log("selected object" + selectedObject);
                    }
                }


            }
        }

        if (selectedObject != null)
        {
            if (Input.GetMouseButtonUp(0))
            {
                
                Vector3 newPosition = GetMouseWorldPosition();
                newPosition.x = Mathf.Round(newPosition.x);
                newPosition.y = Mathf.Round(newPosition.y);
                newPosition.z = Mathf.Round(newPosition.z);
                selectedObject.transform.position = newPosition;

                selectedObject = null;
            }
            else
            {
                Vector3 newPosition = GetMouseWorldPosition();
                newPosition.y = yOffset;
                selectedObject.transform.position = newPosition;
                directionInput = GetArrowKeyLikeDirection(MouseDragPos, selectedObject.transform.position);
            }


        }
    }


    // Helper method to get mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(selectedObject.transform.position).z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    private RaycastHit CastRay()
    {
        Vector3 screenMousePosFar = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            mainCamera.farClipPlane);
        Vector3 screenMousePosNear = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            mainCamera.nearClipPlane);

        Vector3 worldMousePosfar = Camera.main.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = Camera.main.ScreenToWorldPoint(screenMousePosNear);

        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosfar - worldMousePosfar, out hit, 300f);

        return hit;
    }

    public Vector2Int GetArrowKeyLikeDirection(Vector3 mouseDragPos, Vector3 objectPos)
    {
        // Calculate direction (XZ only)
        Vector3 direction = mouseDragPos - objectPos;
        direction.y = 0;

        if (direction.magnitude < 0.1f) // Dead zone to prevent tiny movements
            return Vector2Int.zero;
        //isFar = Mathf.Abs(direction.x) > _dragBuffer || Mathf.Abs(direction.z) > _dragBuffer;

        direction.Normalize();

        


        // Compare absolute X and Z to find the dominant axis
        bool xDominates = Mathf.Abs(direction.x) > Mathf.Abs(direction.z);

        // Snap to the closest cardinal direction (like arrow keys)
        if (xDominates)
        {
            return new Vector2Int(
                direction.x > 0 ? 1 : -1,
                0
            );
        }
        else
        {
            return new Vector2Int(
                0,
                direction.z > 0 ? 1 : -1
            );
        }
    }
}
