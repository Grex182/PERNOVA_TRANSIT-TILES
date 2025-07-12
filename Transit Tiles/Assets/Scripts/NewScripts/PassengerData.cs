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
    [Header("Enums")]
    public PassengerTrait traitType;
    public StationColor targetStation; // Also used as target station
    public TileTypes currTile = TileTypes.Station;

    [Header("Ints")]
    public int moodValue = 3; // 3 = Happy, 2 = Neutral, 1 = Angry [Default is happy]

    [Header("Bools")]
    public bool hasNegativeAura;
    public bool canRotate;
    public bool isPriority;
    public bool isAsleep;

    [SerializeField] public GameObject collision;
    

    [Header("Movement")]
    [SerializeField] public GameObject model;
    private Vector3 _modelStartPos;
    private float moveSpeed = 40f; // Adjust for faster/slower movement


    public void scorePassenger(bool isPositive)
    {
        //Scoring here
        LevelManager.Instance.AddScore(1);
    }

    private void Start()
    {
        _modelStartPos = model.transform.localPosition;
    }

    private void Update()
    {
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

    }
}