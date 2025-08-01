using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum PassengerTrait
{
    Standard,
    Bulky,
    Elderly,
    Pregnant,
    Noisy,
    Stinky,
    Sleepy
}

public class PassengerData : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private PassengerUI passengerUi;

    [Header("Enums")]
    public PassengerTrait traitType;
    public StationColor targetStation;
    public TileTypes currTile = TileTypes.Station;

    [Header("Mood")]
    public int moodValue = 3; // 3 = Happy, 2 = Neutral, 1 = Angry [Default is happy]
    public bool isMoodSwung = false;
    public float effectRadius = 4f; // Radius for mood effect

    [Header("Bools")]
    public bool hasNegativeAura;
    public bool canRotate;
    public bool isPriority;
    public bool isAsleep;
    public bool isSitting = false; // Default to standing animation
    public bool isBottomSection = false; // For bottom section seats
    public bool hasCaffeine = false;

    [Header("Animation")]
    public Animator animator;
    private float _animTime = 0f;
    private float _currAnimLength = 2f;

    [Header("Removal")]
    [SerializeField] float _speed = 1f;
    [SerializeField] float _scaleSpeed = 5f;
    [SerializeField] float _scaleAccel = 50f;
    private bool isBeingRemoved = false;

    [Header("Movement")]
    public GameObject movementCollision;
    public GameObject model;
    private Vector3 _modelStartPos;
    private float moveSpeed = 40f; // Adjust for faster/slower movement

    [Header("Negative Effect Rig")]
    public GameObject noisyEffectRig; // The object whose rotation you want to reset
    public GameObject stinkyEffectRig;
    public GameObject sleepyEffectRig;

    [Header("Sleepy Stuff")]
    [SerializeField] private float _inactiveTimer = 3f;
    [SerializeField] private float _groggyTimer = 5f;
    [SerializeField] private float _sleepyTimer = 3f;
    [SerializeField] private bool _isWoke = false;

    private void Awake()
    {
        passengerUi = GetComponent<PassengerUI>();

        passengerUi.SetSpecialColor(targetStation);

        passengerUi.SetColorblindCanvasState();
        passengerUi.SetColorblindCanvas(targetStation);
        
        isMoodSwung = false;
    }

    private void Start()
    {
        _modelStartPos = model.transform.localPosition;

        if (traitType == PassengerTrait.Noisy || traitType == PassengerTrait.Stinky)
        {
            hasNegativeAura = true;
        }

        if (traitType == PassengerTrait.Sleepy)
        {
            hasCaffeine = false;
            sleepyEffectRig.SetActive(false);
        }

        if (traitType == PassengerTrait.Elderly || traitType == PassengerTrait.Pregnant)
        {
            isPriority = true;
        }
    }

    private void Update()
    {
        if (IsPriorityUpset())
        {
            ChangeMoodValue(-1);
            isMoodSwung = true;
        }

        if (hasNegativeAura)
        {
            CheckForCollision();
        }

        if (isSitting)
        {
            transform.rotation = isBottomSection ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            ResetRigRotation();
        }

        if (model.transform.localPosition != _modelStartPos)
        {
            // Smoothly move position
            model.transform.localPosition = Vector3.Lerp(
                model.transform.localPosition,
                _modelStartPos,
                moveSpeed * Time.deltaTime
            );
        }

        if (model.transform.localRotation != Quaternion.Euler(-90, 0, 0))
        {
            ResetRigRotation();
            // Optionally smooth rotation too
            model.transform.localRotation = Quaternion.Lerp(
                model.transform.localRotation,
                Quaternion.Euler(-90, 0, 0),
                moveSpeed * Time.deltaTime
            );
        }

        if (isBeingRemoved)
        {
            transform.localPosition += Vector3.up * _speed * Time.deltaTime;

            _scaleSpeed -= _scaleAccel * Time.deltaTime;
            transform.localScale += Vector3.one * _scaleSpeed * Time.deltaTime;

            if (transform.localScale.z < 0.1f)
            {
                Destroy(gameObject);
            }
        }

        AnimationUpdater();

        if (traitType != PassengerTrait.Sleepy || currTile == TileTypes.Station || hasCaffeine) { return; }
        SetSleepState();
    }

    private bool IsPriorityUpset()
    {
        bool isInStation = currTile == TileTypes.Station;
        bool isCorrectType = isPriority || traitType == PassengerTrait.Sleepy;
        bool inStanding = gameObject.transform.position.y < 0.5f && currTile != TileTypes.Seat;

        return !isInStation && isCorrectType && !isMoodSwung && inStanding;
    }

    public void ResetMoodSwing()
    {
        isMoodSwung = false;
    }

    private void SetSleepState()
    {
        if (gameObject.transform.position.y < 0.5f && !isAsleep && !_isWoke)
        {
            //start countdown
            _sleepyTimer -= Time.deltaTime;

            if (_sleepyTimer <= 0)
            {
                //-----Turn passenger to sleep
                isAsleep = true;
                _sleepyTimer = -_groggyTimer;
                animator.SetBool("IsSleepy", true);
                sleepyEffectRig.SetActive(true);
            }
        }
        else if (_isWoke)
        {
            _sleepyTimer += Time.deltaTime;

            if (_sleepyTimer >= 0)
            {
                //-----Turn passenger awake
                _sleepyTimer = _inactiveTimer;
                _isWoke = false;
                isAsleep = false;
                animator.SetBool("IsSleepy", false);
            }
        }
        else if (gameObject.transform.position.y > 0.5f)
        {
            _sleepyTimer = _inactiveTimer;
        }

    }

    public void WakePassenger()
    {
        sleepyEffectRig.SetActive(false);
        AudioManager.Instance.PlayVoice(GetComponent<PassengerAppearance>().isFemale,
                                            Random.Range(2, 4));
        _isWoke = true;
    }

    public void ScorePassenger(bool isRushHour)
    {
        StationColor color = LevelManager.Instance != null ?
            LevelManager.Instance.currStation :
            TutorialManager.Instance.currStation;
        if (targetStation != color)
        {
            ChangeMoodValue(-2); // Set to angry if wrong station
        }

        int _moodScore = (moodValue * 200) - 400; // Mood Score: 3 = 200, 2 = 0, 1 = -200
        int _priorityScore = isPriority ? 2 : 1;

        int _score = _moodScore * _priorityScore; // if Priority // Mood Score: 3 = 400, 2 = 0, 1 = -400

        if (targetStation == color)
        {
            _score += 250; // Correct Station Bonus
            AudioManager.Instance.PlayVoice(GetComponent<PassengerAppearance>().isFemale,
                                            Random.Range(4, 6));
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.correctDisembarkCount++;
            }
        }
        else
        {
            int targetStationIndex = (int)targetStation;
            int currentStationIndex = LevelManager.Instance != null ? 
                (int)LevelManager.Instance.currStation : 
                (int)TutorialManager.Instance.currStation;

            int distance = Mathf.Abs(targetStationIndex - currentStationIndex);

            _score -= 100 + (distance * 100); // Wrong Station Penalty

            AudioManager.Instance.PlayVoice(GetComponent<PassengerAppearance>().isFemale,
                                            Random.Range(0, 2));
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.hasDisembarkedWrong = true;
            }
        }
        _score = isRushHour ? Mathf.RoundToInt(_score * 1.5f) : _score;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddScore(_score);
        }

        Debug.Log($"Score : {_score} \n mood {_moodScore} * {_priorityScore} + Station Loc");
    }

    public void ChangeMoodValue(int moodChange)
    {
        Debug.Log("ChangeMoodValue called");
        moodValue = Mathf.Clamp(moodValue + moodChange, 1, 3);

        if (passengerUi != null)
        {
            passengerUi.ChangeMoodImg(moodValue);
            Debug.Log($"ChangeMoodImg called");
        }
        else
        {
            Debug.Log("passengerUi is null, cannot change mood image.");
        }
    }

    private void AnimationUpdater()
    {
        if (animator == null) return;

        //Animation
        animator.SetBool("IsSitting", isSitting);

        _animTime += Time.deltaTime;


        if (_animTime >= _currAnimLength)
        {
            //Do Random Animation
            int randomIdle = Random.Range(0, 3);
            animator.SetInteger("IdleVariation", randomIdle);
            _currAnimLength = animator.GetCurrentAnimatorStateInfo(0).length;

            _animTime = 0f;
        }
    }

    private void CheckForCollision()
    {
        Vector3 areaOfEffect = transform.position + Vector3.up * 2.5f; // Adjust for height if needed
        Collider[] colliders = Physics.OverlapSphere(areaOfEffect, effectRadius);

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Drag") && c.gameObject != gameObject && currTile != TileTypes.Station)
            {
                PassengerData otherPassenger = c.GetComponent<PassengerData>();

                if (otherPassenger != null && !otherPassenger.isMoodSwung)
                {
                    // Apply mood effect
                    otherPassenger.isMoodSwung = true;
                    otherPassenger.ChangeMoodValue(-1);

                    Debug.Log($"{name} affecting {otherPassenger.name}");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = hasNegativeAura ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }

    private void ResetRigRotation()
    {
        ResetSingleRig(noisyEffectRig);
        ResetSingleRig(stinkyEffectRig);
        ResetSingleRig(sleepyEffectRig);
    }

    private void ResetSingleRig(GameObject rig)
    {
        if (rig == null) return;

        // Make the object face the same direction as the camera (billboard effect)
        rig.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }


    //Moving Blocked Passengers
    public void GoToPose(Vector3 targetPosition, Quaternion targetRotation, float moveSpeed, float rotateSpeed)
    {
        StartCoroutine(MoveAndRotate(targetPosition, targetRotation, moveSpeed, rotateSpeed));
    }

    private IEnumerator MoveAndRotate(Vector3 targetPosition, Quaternion targetRotation, float moveSpeed, float rotateSpeed)
    {
        Vector3 targetZ = new Vector3(transform.position.x, transform.position.y + 1f, targetPosition.z);

        while (Mathf.Abs(transform.position.z - targetPosition.z) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetZ,
                moveSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f ||
               Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            // Move towards position
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Rotate towards target rotation
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Snap to final position/rotation (optional)
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    //Destroy Passengers
    public void PassengerRemove()
    {
        
        isBeingRemoved = true;
    }
}