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

    private float idleSwitchCooldown = 5f;

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
        assignedColor = (StationColor)Random.Range(0, System.Enum.GetValues(typeof(StationColor)).Length);
        Debug.Log("Assigned Color: " + assignedColor);

        SetPassengerStation(gameObject, assignedColor.ToString());

        StartCoroutine(SwitchIdleAnimationCooldown());
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
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

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;

        if (force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrainTile") && !isInsideTrain && !GameManager.instance.StationManager.isTrainMoving || (other.CompareTag("ChairTile") && !isInsideTrain && !GameManager.instance.StationManager.isTrainMoving))
        {
            isInsideTrain = true;

            Debug.Log("Passenger entered train.");
        }
        else if (other.CompareTag("PlatformTile") && isInsideTrain && !GameManager.instance.StationManager.isTrainMoving && !GameManager.instance.StationManager.hasGameStarted)
        {
            if (assignedColor == GameManager.instance.StationManager.stationColor)
            {
                GameManager.instance.ScoreManager.AddScore();
                GameManager.instance.PublicRatingManager.AddPublicRating();
            }
            else
            {
                GameManager.instance.PublicRatingManager.ReducePublicRating();
            }

            isInsideTrain = false;

            GameManager.instance.Board.spawnedPassengers.Remove(this);
            Destroy(gameObject);
        }
    }

    private bool SetPassengerStation(GameObject passenger, string stationColor)
    {
        //could be changed to enum instead, but for now, its by gameObject name
        if (gameObject.name.Contains("Base"))
        {
            Transform childTransform = passenger.transform.Find("FemaleUpper/f_top_shirt"); //This definitely needs to be changed, no finds please (But if theres nothing else, this will do ig)

            SkinnedMeshRenderer childMeshRenderer = childTransform.GetComponent<SkinnedMeshRenderer>();

            var material = childMeshRenderer.material;
            material.SetColor(ColorProperty, GetStationColor(stationColor));
            return true;
        }

        #region NULL-CHECKS
        if (!passenger.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
        {
            Debug.LogError("No MeshRenderer found on passenger prefab.");
            return false;
        }
        #endregion

        return true;
    }

    public void CheckPosition()
    {
        if (!isInsideTrain)
        {
            GameManager.instance.Board.spawnedPassengers.Remove(this);

            Destroy(gameObject);
        }
    }

    private IEnumerator SwitchIdleAnimationCooldown()
    {
        int randomCooldownNumber = Random.Range(5, 14);

        yield return new WaitForSeconds(randomCooldownNumber);

        int randomNumber = Random.Range(1, 3);

        if (randomNumber == 1)
        {
            animator.SetTrigger("isIdle1");
        }
        else if (randomNumber == 2)
        {
            animator.SetTrigger("isIdle2");
        }

        Debug.Log("Animations are starting");

        StartCoroutine(SwitchIdleAnimationCooldown());
    }

    private static readonly string[] validStationColors = new string[]
    {
        "Pink", "Red", "Orange", "Yellow", "Green", "Blue", "Violet"
    };

    private Color GetStationColor(string stationColor)
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
    }
}
