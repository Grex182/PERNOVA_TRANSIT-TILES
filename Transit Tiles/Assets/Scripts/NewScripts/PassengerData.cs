using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    [SerializeField] private Animator animator;

    [Header("Enums")]
    public PassengerTrait traitType;
    public StationColor targetStation;
    public TileTypes currTile = TileTypes.Station;

    [Header("Ints")]
    public int moodValue = 3; // 3 = Happy, 2 = Neutral, 1 = Angry [Default is happy]

    [Header("Bools")]
    public bool hasNegativeAura;
    public bool canRotate;
    public bool isPriority;
    public bool isAsleep;
    public bool isSitting = false; // Default to standing animation
    public bool isBottomSection = false; // For bottom section seats

    private float _animTime = 0f;

    [SerializeField] public GameObject collision;
    

    [Header("Movement")]
    [SerializeField] public GameObject model;
    private Vector3 _modelStartPos;
    private float moveSpeed = 40f; // Adjust for faster/slower movement

    private void Start()
    {
        _modelStartPos = model.transform.localPosition;

        if (traitType == PassengerTrait.Noisy || traitType == PassengerTrait.Stinky)
        {
            hasNegativeAura = true;
        }
    }

    private void Update()
    {

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
        if (model.transform.localRotation != Quaternion.identity)
        {
            // Optionally smooth rotation too
            model.transform.localRotation = Quaternion.Lerp(
                model.transform.localRotation,
                Quaternion.identity,
                moveSpeed * Time.deltaTime
            );
        } 
        
        AnimationUpdater();
    }

    public void ScorePassenger()
    {
        //Scoring here

        int _moodScore = (moodValue * 2) - 400;
        int _priorityScore = isPriority ? 2 : 1;
        
        int _score = (100 + _moodScore) * _priorityScore;

        if (targetStation == LevelManager.Instance.currStation)
        {
            _score += 100; // Bonus for reaching the correct station
        }
        else
        {
            _score -= 800; // Bonus for reaching the correct station
        }
        LevelManager.Instance.AddScore(_score);
    }

    private void ChangeMoodValue(int moodChange)
    {
        moodValue += moodChange;
        if (moodValue < 1) moodValue = 1; // Angry
        if (moodValue > 3) moodValue = 3; // Happy
        
        if (passengerUi != null)
        {
            passengerUi.ChangeMoodImg(moodValue);
        }

        if (passengerUi.moodletCoroutine != null)
        {
            StopCoroutine(passengerUi.moodletCoroutine);
        }

        passengerUi.moodletCoroutine = StartCoroutine(passengerUi.ActivateMoodlet());
    }

    private void AnimationUpdater()
    {
        if (animator == null) return;
        //Animation
        animator.SetBool("IsSitting", isSitting);

        _animTime += Time.deltaTime;

        if (_animTime >= 2)
        {
            //Do Random Animation
            int randomIdle = Random.Range(0, 3);
            animator.SetInteger("IdleVariation", randomIdle);
            Debug.Log("Anim called");

            _animTime = 0f;
        }
    }

}