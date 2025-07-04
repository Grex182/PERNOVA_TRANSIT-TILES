using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PassengerType
{
    Standard = 1,
    Elder = 2,
    Bulky
}

public enum PassengerEffects
{
    None = 0,
    Noisy = 1,
    Smelly = 2,
}

public class Passenger : MonoBehaviour
{
    public int currentX;
    public int currentY;

    public PassengerType type;
    public PassengerEffects effect;

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
        StartCoroutine(SwitchIdleAnimationCooldown());
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10);
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
        if (other.CompareTag("TrainTile") && !isInsideTrain && !StationManager.Instance.isTrainMoving || (other.CompareTag("ChairTile") && !isInsideTrain && !StationManager.Instance.isTrainMoving))
        {
            isInsideTrain = true;
            Debug.Log("Passenger entered train.");
        }
        else if (other.CompareTag("PlatformTile") && isInsideTrain && !StationManager.Instance.isTrainMoving && !StationManager.Instance.hasGameStarted)
        {
            bool isStandardPassenger = (type == PassengerType.Standard);

            if (assignedColor == StationManager.Instance.stationColor)
            {
                //LevelManager.Instance.AddPublicRating(isStandardPassenger);
            }
            else
            {
                //LevelManager.Instance.ReducePublicRating(isStandardPassenger);
            }

            isInsideTrain = false;

            other.gameObject.layer = LayerMask.NameToLayer("Tile");

            SpawnPassengers.Instance.spawnedPassengers.Remove(this);
            Destroy(gameObject);
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
            case StationColor.Pink: return Color.magenta;
            case StationColor.Red: return Color.red;
            case StationColor.Orange: return new Color(1f, 0.5f, 0f);
            case StationColor.Yellow: return Color.yellow;
            case StationColor.Green: return Color.green;
            case StationColor.Blue: return Color.blue;
            case StationColor.Violet: return new Color(0.5f, 0f, 1f);
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

            //LevelManager.Instance.ReducePublicRating(isStandardPassenger);
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
        "Pink", "Red", "Orange", "Yellow", "Green", "Blue", "Violet"
    };

/*    private Color GetStationColor(string stationColor)
    {
        switch (stationColor)
        {
            case "Pink": return Color.magenta;
            case "Red": return Color.red;
            case "Orange": return new Color(1f, 0.5f, 0f);
            case "Yellow": return Color.yellow;
            case "Green": return Color.green;
            case "Blue": return Color.blue;
            case "Violet": return new Color(0.5f, 0f, 1f);
            default: return Color.white;
        }
    }*/
}
