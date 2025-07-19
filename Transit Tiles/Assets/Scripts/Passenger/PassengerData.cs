using System.Collections;
using System.Collections.Generic;
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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private float _animTime = 0f;
    private float _currAnimLength = 2f;

    [Header("Movement")]
    public GameObject movementCollision;
    public GameObject model;
    private Vector3 _modelStartPos;
    private float moveSpeed = 40f; // Adjust for faster/slower movement

    private void Start()
    {
        _modelStartPos = model.transform.localPosition;

        if (traitType == PassengerTrait.Noisy || traitType == PassengerTrait.Stinky)
        {
            hasNegativeAura = true;
        }

        passengerUi = GetComponent<PassengerUI>();
    }

    private void Update()
    {
        if (hasNegativeAura)
        {
            CheckForCollision();
        }

        if (isSitting)
        {
            transform.rotation = isBottomSection ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
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
            // Optionally smooth rotation too
            model.transform.localRotation = Quaternion.Lerp(
                model.transform.localRotation,
                Quaternion.Euler(-90, 0, 0),
                moveSpeed * Time.deltaTime
            );
        } 
        
        AnimationUpdater();
    }

    public void ScorePassenger()
    {
        if (targetStation != LevelManager.Instance.currStation)
        {
            ChangeMoodValue(1); // Set to angry if wrong station
        }

        int _moodScore = (moodValue * 200) - 400; // Mood Score: 3 = 200, 2 = 0, 1 = -200
        int _priorityScore = isPriority ? 2 : 1;
        
        int _score = _moodScore * _priorityScore; // if Priority // Mood Score: 3 = 400, 2 = 0, 1 = -400

        if (targetStation == LevelManager.Instance.currStation)
        {
            _score += 250; // Correct Station Bonus
            AudioManager.Instance.PlayVoice(GetComponent<PassengerAppearance>().isFemale,
                                            Random.Range(4, 6));

            LevelManager.Instance.correctDisembarkCount++;
        }
        else
        {
            int targetStationIndex = (int)targetStation;
            int currentStationIndex = (int)LevelManager.Instance.currStation;

            int distance = Mathf.Abs(targetStationIndex - currentStationIndex);

            _score -= 100 + (distance * 100); // Wrong Station Penalty

            AudioManager.Instance.PlayVoice(GetComponent<PassengerAppearance>().isFemale,
                                            Random.Range(0, 2));

            LevelManager.Instance.hasDisembarkedWrong = true;
        }
        LevelManager.Instance.AddScore(_score);

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

        foreach(Collider c in colliders)
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
}