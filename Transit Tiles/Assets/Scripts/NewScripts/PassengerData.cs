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
    [SerializeField] public GameObject model;



    public void scorePassenger(bool isPositive)
    {
        //Scoring here
        LevelManager.Instance.AddScore(1);
    }

    private void Update()
    {
        if (model.transform.position != transform.position)
        {

        }
    }
}