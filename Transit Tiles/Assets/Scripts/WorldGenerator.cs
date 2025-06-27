using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SectionMovement;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;

// Note: Transfer Game flow code to LevelManager
public enum MovementState
{
    Accelerate,
    Decelerate,
    Stationary,
    Moving
}

public class WorldGenerator : Singleton<WorldGenerator>
{
    public enum TrainDirection { Right, Left }

    private TrainDirection _direction = TrainDirection.Right;
    public MovementState currState = MovementState.Stationary;

    [Header("Prefabs")]
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private GameObject _stationPrefab;
    [SerializeField] private GameObject[] _railPrefab;

    //[Header("Materials")] // NOTE: Temporary (Remove when different models are done)
    //[SerializeField] private Material platformMat;
    //[SerializeField] private Material stationMat;

    [Header("Positions")]
    [SerializeField] Transform platformSpawnPoint;
    [SerializeField] Transform railSpawnPoint;
    [SerializeField] private float _platformSpacing = 10f;
    [SerializeField] private float _despawnXPos = 40f;
    [SerializeField] private float _decelerationRate = 0.5f;

    private List<GameObject> _platformSections = new List<GameObject>();
    private readonly float _phaseTimer = 20.0f;
    private readonly float _speedTimer = 3.0f;
    public float currTimer { get; private set; }

    public float CurrentSpeed { get; private set; }

    private bool hasStation = true;

    public bool stateChanged = false;
    // get current station

    private void Start()
    {
        //if (GameManager.Instance.gameState == GameState.GameInit)
        //{
        //    InitializeWorld();
        //}

        
        StartCoroutine(MovementCycle());
    }

    private void Update()
    {
        CheckForRecycling();
    }

    public void InitializeWorld()
    {
        foreach (var section in _platformSections)
        {
            Destroy(section);
        }
        _platformSections.Clear();

        int platformCount = 7;

        for (int i = 0; i < platformCount; i++)
        {
            SpawnPlatform(i);
        }

        currTimer = 0f;
    }

    private void SpawnPlatform(int index, bool isStation = false)
    {
        Vector3 spawnPos = platformSpawnPoint.position + new Vector3(index * _platformSpacing, 0, 0);

        GameObject prefab = isStation ? _stationPrefab : _platformPrefab;
        GameObject newPlatform = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        _platformSections.Add(newPlatform);
    }

    private IEnumerator MovementCycle()
    {
        Debug.Log("Movement cycle started");
        while (true) // NOTE: Replace with GameManager.Instance.gameState != GameManager.Instance.GameState.GameEnded
        {
            //currTimer = 0f;

            //// Station Phase
            //currState = MovementState.Stationary;
            //currTimer = _phaseTimer;
            //Debug.Log("Station Phase");
            //yield return new WaitForSeconds(currTimer);
            //currTimer = 0f;
            //stateChanged = true; 
            //currTimer = 0f;

            //// card phase
            //currState = MovementState.Stationary;
            //currTimer = _phaseTimer;
            //stateChanged = false;
            //Debug.Log("card phase");
            //yield return new WaitForSeconds(3f);

            stateChanged = true;
            currTimer = 0f;

            // Leaving Station
            currState = MovementState.Accelerate;
            currTimer = _speedTimer;
            stateChanged = false;
            Debug.Log("Accelerating");
            yield return new WaitForSeconds(currTimer);

            stateChanged = true;
            currTimer = 0f;

            // Travel Phase
            currState = MovementState.Moving;
            currTimer = _phaseTimer;
            stateChanged = false;
            Debug.Log("Travel Phase");
            yield return new WaitForSeconds(currTimer);

            stateChanged = true;
            currTimer = 0f;

            // Arriving Station
            currState = MovementState.Decelerate;
            currTimer = _speedTimer;
            stateChanged = false;
            Debug.Log("Decelerating");
            yield return new WaitForSeconds(currTimer);
        }
    }

    private void CheckForRecycling()
    {
        float totalLength = _platformSections.Count * _platformSpacing;

        for (int i = 0; i < _platformSections.Count; i++)
        {
            if (_platformSections[i].transform.position.x >= _despawnXPos)
            {
                // Calculate exact new position based on section width
                Vector3 newPos = _platformSections[i].transform.position;
                newPos.x -= totalLength;

                // Move the section (no destroy/instantiate)
                _platformSections[i].transform.position = newPos;

                // Optional: Change to station if needed
                if (currState == MovementState.Decelerate && !hasStation)
                {
                    SwapToStation(_platformSections[i]);
                    hasStation = true;
                }
            }
        }
    }

    private void SwapToStation(GameObject section)
    {
        // Cache position/rotation
        Vector3 pos = section.transform.position;
        Quaternion rot = section.transform.rotation;
        Transform parent = section.transform.parent;

        // Replace with station
        Destroy(section);
        GameObject newStation = Instantiate(_stationPrefab, pos, rot, parent);

        // Update reference in list
        int index = _platformSections.IndexOf(section);
        _platformSections[index] = newStation;
    }
}
