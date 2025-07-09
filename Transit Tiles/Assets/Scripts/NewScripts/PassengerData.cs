using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public enum PassengerTrait
{
    Standard,
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

    [Header("Ints")]
    public int moodValue = 3; // 3 = Happy, 2 = Neutral, 1 = Angry [Default is happy]

    [Header("Bools")]
    public bool hasNegativeAura;
    public bool canRotate;
    public bool isPriority;
    public bool isAsleep;

    public GameObject collision;
    public PassengerAppearance passengerAppearance;
}