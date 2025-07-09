using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private List<GameObject> passengerPrefabs;
    [SerializeField] private List<GameObject> passengerBulkyPrefabs;

    private readonly int minPassengers = 3;
    [SerializeField] private int chanceBulky = 80; // Doesnt work as well rn, this value doesnt matter bc bulky passengers take so much space

    private List<GameObject> spawnedPassengers = new List<GameObject>();

}
