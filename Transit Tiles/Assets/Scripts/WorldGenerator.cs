using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SectionMovement;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;

// Note: Transfer Game flow code to LevelManager
public enum TrainDirection { Right, Left }

public class WorldGenerator : Singleton<WorldGenerator>
{
    public TrainDirection trainDirection = TrainDirection.Right;
    
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

    [Header("Spawned Sections")]
    private List<GameObject> _platformSections = new List<GameObject>();

    [Header("Bool")]
    private bool hasStation = true;

    // get current station

    private void Start()
    {
        //if (GameManager.Instance.gameState == GameState.GameInit)
        //{
        //    InitializeWorld();
        //}
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

        trainDirection = TrainDirection.Right;
    }

    private void SpawnPlatform(int index, bool isStation = false)
    {
        Vector3 spawnPos = platformSpawnPoint.position + new Vector3(index * _platformSpacing, 0, 0);

        GameObject prefab = isStation ? _stationPrefab : _platformPrefab;
        GameObject newPlatform = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        _platformSections.Add(newPlatform);
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
                //if (LevelManager.Instance.currState == MovementState.Decelerate && !hasStation)
                //{
                //    SwapToStation(_platformSections[i]);
                //    hasStation = true;
                //}
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
