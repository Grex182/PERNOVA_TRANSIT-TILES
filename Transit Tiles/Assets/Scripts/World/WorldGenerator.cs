using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static LevelManager;
using static SectionMovement;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;
using static UnityEngine.Rendering.CoreUtils;

// Note: Transfer Game flow code to LevelManager
public class WorldGenerator : Singleton<WorldGenerator>
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] _stationPrefabs;
    [SerializeField] private GameObject[] _railPrefabs;
    [SerializeField] private GameObject _environmentPrefab;

    //[Header("Materials")] // NOTE: Temporary (Remove when different models are done)
    //[SerializeField] private Material platformMat;
    //[SerializeField] private Material stationMat;

    [Header("Positions")]
    // Station and Rail Spawn Points
    [SerializeField] Transform stationCenterPoint;
    [SerializeField] Transform stationSpawnPoint;
    [SerializeField] Transform railSpawnPoint;
    [SerializeField] private readonly float _platformSpacing = 10f;
    // Environment Spawn Points
    [SerializeField] private Transform environmentSpawnPoint;


    [Header("Spawned Sections")]
    private readonly int maxRails = 7;
    private List<GameObject> _spawnedStations = new List<GameObject>();
    private List<GameObject> _spawnedRails = new List<GameObject>();
    [SerializeField] private List<GameObject> _spawnedMap = new List<GameObject>();
    [SerializeField] private GameObject _spawnedEnvironment;

    [Header("Objects")]
    [SerializeField] private GameObject _trainObj;

    private void Start()
    {
        InitializeWorld();
    }

    private void Update()
    {
        CheckForRecycling();
    }

    public void InitializeWorld()
    {
        // RAILS
        foreach (var rail in _spawnedRails)
        {
            Destroy(rail);
        }
        _spawnedRails.Clear();

        for (int i = 0; i < maxRails; i++)
        {
            SpawnRail(i);
        }

        // STATIONS
        foreach (var station in _spawnedStations)
        {
            Destroy(station);
        }
        _spawnedStations.Clear();

        for (int i = 0; i < _stationPrefabs.Length; i++)
        {
            Transform spawnPos = i == 0 ? stationCenterPoint : stationSpawnPoint;
            SpawnStation(i, spawnPos);

            // Only activate the first two station
            bool canActivate = i == 0 || i == 1;
            _spawnedStations[i].SetActive(canActivate); 
        }

        // ENVIRONMENTS
        _spawnedEnvironment = Instantiate(_environmentPrefab, environmentSpawnPoint.position, Quaternion.identity, transform);
        
        foreach (var environment in _spawnedMap)
        {
            Destroy(environment);
        }
        _spawnedMap.Clear();

        for (int i = 0; i < _spawnedEnvironment.transform.childCount; i++)
        {
            _spawnedMap.Add(_spawnedEnvironment.transform.GetChild(i).gameObject);

            bool canActivate = i == 0;
            _spawnedMap[i].SetActive(canActivate);
        }

        // VALUES
        LevelManager.Instance.currDirection = TrainDirection.Right;
    }

    private void SpawnStation(int index, Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.position;
        GameObject newPlatform = Instantiate(_stationPrefabs[index], spawnPos, stationSpawnPoint.localRotation, transform);

        _spawnedStations.Add(newPlatform);
    }

    private void SpawnRail(int index)
    {
        int randNum = Random.Range(0, _spawnedRails.Count);
        Vector3 spawnPos = railSpawnPoint.position + new Vector3(index * _platformSpacing, 0, 0);
        GameObject newRail = Instantiate(_railPrefabs[randNum], spawnPos, railSpawnPoint.localRotation, transform);
        
        _spawnedRails.Add(newRail);
    }


    private void CheckForRecycling()
    {
        if (_spawnedEnvironment == null || _trainObj == null)
            return;

        // RAILS
        for (int i = 0; i < _spawnedRails.Count; i++)
        {
            if (_spawnedRails[i] != null &&
            _spawnedRails[i].transform.position.x >= 40)
            {
                _spawnedRails[i].transform.position = new Vector3(_spawnedRails[i].transform.position.x - 70f, _spawnedRails[i].transform.position.y, _spawnedRails[i].transform.position.z);
                _spawnedRails[i].GetComponent<SectionMovement>().startPosition.x -= 70f;
            }
        }

        _trainObj.GetComponent<AnimateTrain>().SetMovingAnimSpeed(_spawnedEnvironment.GetComponent<SectionMovement>()._speedCurr);
    }

    public void ActivateStations()
    {
        // STATIONS
        CurrentStation currentStation = LevelManager.Instance.currStation;
        CurrentStation nextStation = StationCycler.GetNextStation(currentStation, LevelManager.Instance.currDirection);

        for (int i = 0; i < _spawnedStations.Count; i++)
        {
            if (_spawnedStations[i] != null)
            {
                // Activate if it's current or next station, deactivate others
                bool shouldBeActive =
                    (i == (int)currentStation) ||
                    (i == (int)nextStation);

               
                _spawnedStations[i].SetActive(shouldBeActive);
            }
        }

        for (int i = 0; i < _spawnedMap.Count; i++)
        {
            if (_spawnedMap[i].activeSelf)
            {
                _spawnedMap[i + 1].SetActive(true);
                _spawnedMap[i].SetActive(false);

                //_spawnedEnvironments[i - 1].SetActive(true);
                //_spawnedEnvironments[i].SetActive(false);
                break;
            }
        }

        _spawnedEnvironment.GetComponent<SectionMovement>().startPosition = environmentSpawnPoint.position;
        _spawnedEnvironment.GetComponent<SectionMovement>().ResetTravel();
    }
}
