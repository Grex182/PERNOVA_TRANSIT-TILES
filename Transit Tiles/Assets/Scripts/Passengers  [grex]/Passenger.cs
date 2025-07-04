using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PassengerType
{
    None = 0,
    Standard = 1,
    Elder = 2,
    Bulky = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class Passenger : MonoBehaviour
{
    public int currentX;
    public int currentY;

    public PassengerType type;

    private const string ColorProperty = "_BaseColor";
    public StationColor assignedColor;

    private Vector3 desiredPosition;
    //[SerializeField] private Vector3 desiredScale = Vector3.one;

    private Animator animator;

    [SerializeField] private bool isInsideTrain = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
/*        if (assignedColor == default) // Only assign randomly if not already assigned
        {
            assignedColor = (StationColor)Random.Range(0, System.Enum.GetValues(typeof(StationColor)).Length);
            //Debug.Log("Assigned Color: " + assignedColor);
            SetPassengerStation();
        }*/

        StartCoroutine(SwitchIdleAnimationCooldown());
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10);
        //transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        //transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Passenger[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Down
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }

            if (board[currentX, i] != null)
            {
                break;
            }
        }

        //Up
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }

            if (board[currentX, i] != null)
            {
                break;
            }
        }

        //Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }

            if (board[i, currentY] != null)
            {
                break;
            }
        }

        //Right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }

            if (board[i, currentY] != null)
            {
                break;
            }
        }

        return r;

        /*        //r means return value
                List<Vector2Int> r = new List<Vector2Int>();

                r.Add(new Vector2Int(3, 3));
                r.Add(new Vector2Int(3, 4));
                r.Add(new Vector2Int(4, 3));
                r.Add(new Vector2Int(4, 4));

                return r;*/
    }

    public virtual void SetPosition(Vector3 worldPosition, bool force = false)
    {
        // Convert the world position to local position relative to parent
        desiredPosition = transform.parent.InverseTransformPoint(worldPosition);

        if (force)
        {
            transform.localPosition = desiredPosition;
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("TrainTile") || other.CompareTag("ChairTile")) && 
            !isInsideTrain && LevelManager.Instance.currState == MovementState.Station)
        {
            isInsideTrain = true;
            Debug.Log("Passenger entered train.");

            return;
        }
        else if (other.CompareTag("PlatformTile") && isInsideTrain && LevelManager.Instance.currState == MovementState.Station)
        {
            bool isStandardPassenger = (type == PassengerType.Standard);

            if (assignedColor == LevelManager.Instance.currColor)
            {
                LevelManager.Instance.AddPublicRating(isStandardPassenger);
            }
            else
            {
                LevelManager.Instance.ReducePublicRating(isStandardPassenger);
            }

            isInsideTrain = false;

            other.gameObject.layer = LayerMask.NameToLayer("Tile");

            SpawnPassengers.Instance.spawnedPassengers.Remove(this);
            Destroy(gameObject);

            return;
        }
    }

    public void SetPassengerStation()
    {
        Transform childTransform = transform.Find("FemaleUpper/f_top_shirt");

        if (childTransform != null && childTransform.TryGetComponent<SkinnedMeshRenderer>(out var meshRenderer))
        {
            var material = meshRenderer.material;
            material.SetColor(ColorProperty, GetColorFromStation(assignedColor));
        }
        else
        {
            Debug.LogWarning($"Missing shirt mesh on {gameObject.name}");
        }
    }

    private Color GetColorFromStation(StationColor color)
    {
        switch (color)
        {
            case StationColor.Red: 
                return LevelManager.Instance.stationColors[0];

            case StationColor.Pink: 
                return LevelManager.Instance.stationColors[1];

            case StationColor.Orange: 
                return LevelManager.Instance.stationColors[2];

            case StationColor.Yellow: 
                return LevelManager.Instance.stationColors[3];

            case StationColor.Green: 
                return LevelManager.Instance.stationColors[4];

            case StationColor.Blue: 
                return LevelManager.Instance.stationColors[5];

            case StationColor.Violet: 
                return LevelManager.Instance.stationColors[6];

            default: return Color.white;
        }
    }

    public void CheckPosition()
    {
        if (!isInsideTrain)
        {
            SpawnPassengers.Instance.spawnedPassengers.Remove(this);

            Destroy(gameObject);

            bool isStandardPassenger = (type == PassengerType.Standard);

            LevelManager.Instance.ReducePublicRating(isStandardPassenger);
        }
    }

    private IEnumerator SwitchIdleAnimationCooldown()
    {
        int randomCooldownNumber = Random.Range(5, 14);

        yield return new WaitForSeconds(randomCooldownNumber);

        int randomNumber = Random.Range(1, 3);

        if (randomNumber == 1)
        {
            animator.SetTrigger("Idle1");
        }
        else if (randomNumber == 2)
        {
            animator.SetTrigger("Idle2");
        }
        StartCoroutine(SwitchIdleAnimationCooldown());
    }

    public void PassengerSelected()
    {
        animator.SetBool("isSelected", true);
        Debug.Log($"{gameObject.name} was clicked!");
    }

    public void PassengerDropped()
    {
        animator.SetBool("isSelected", false);
        //animator.SetTrigger("Idle");
        Debug.Log($"{gameObject.name} was dropped!");
    }

    private static readonly string[] validStationColors = new string[]
    {
        "Red", "Pink",  "Orange", "Yellow", "Green", "Blue", "Violet"
    };
}
