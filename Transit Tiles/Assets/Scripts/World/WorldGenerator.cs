using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelManager;
using static SectionMovement;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;
using static UnityEngine.Rendering.CoreUtils;

// Note: Transfer Game flow code to LevelManager
public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance;

    [Header("Prefabs")]
    public GameObject stationsParent;
    [SerializeField] private GameObject[] _stationPrefabs;
    [SerializeField] private GameObject _railPrefab;
    [SerializeField] private GameObject _environmentPrefab;

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
    [SerializeField] private GameObject nextStation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (LevelManager.Instance != null)
        {
            CheckForRecycling(LevelManager.Instance.currDirection);
        }

        if (TutorialManager.Instance != null)
        {
            CheckForRecycling(TutorialManager.Instance.currDirection);
        }
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

            //bool canActivate = i == 0;
            //_spawnedMap[i].SetActive(canActivate);
        }

        // VALUES
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currDirection = TrainDirection.Right;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.currDirection = TrainDirection.Right;
        }
    }

    private void SpawnStation(int index, Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.position;
        GameObject newPlatform = Instantiate(_stationPrefabs[index], spawnPos, stationSpawnPoint.localRotation, stationsParent.transform);

        _spawnedStations.Add(newPlatform);
    }

    private void SpawnRail(int index)
    {
        Vector3 spawnPos = railSpawnPoint.position + new Vector3(index * _platformSpacing, 0, 0);
        GameObject newRail = Instantiate(_railPrefab, spawnPos, railSpawnPoint.localRotation, transform);
        
        _spawnedRails.Add(newRail);
    }


    private void CheckForRecycling(TrainDirection direction)
    {
        if (_spawnedEnvironment == null || _trainObj == null)
            return;

        // RAILS
        for (int i = 0; i < _spawnedRails.Count; i++)
        {
            if(direction == TrainDirection.Right)
            {
                if (_spawnedRails[i] != null &&
            _spawnedRails[i].transform.position.x >= 40)
                {
                    _spawnedRails[i].transform.position = new Vector3(_spawnedRails[i].transform.position.x - 70f, _spawnedRails[i].transform.position.y, _spawnedRails[i].transform.position.z);
                    _spawnedRails[i].GetComponent<SectionMovement>().startPosition.x -= 70f;
                    RandomRails(_spawnedRails[i]);
                }
            }
            else
            {
                if (_spawnedRails[i] != null &&
            _spawnedRails[i].transform.position.x <= -40)
                {
                    _spawnedRails[i].transform.position = new Vector3(_spawnedRails[i].transform.position.x + 70f, _spawnedRails[i].transform.position.y, _spawnedRails[i].transform.position.z);
                    _spawnedRails[i].GetComponent<SectionMovement>().startPosition.x += 70f;
                    RandomRails(_spawnedRails[i]);
                }
            }
            


        }

        _trainObj.GetComponent<AnimateTrain>().SetMovingAnimSpeed(_spawnedEnvironment.GetComponent<SectionMovement>()._speedCurr);
    }

    private void RandomRails(GameObject rail)
    {
        for (int i = 0; i < rail.transform.childCount; i++)
        {
            rail.transform.GetChild(i).gameObject.SetActive(false);
        }

        int randNum = Random.Range(0, rail.transform.childCount);
        rail.transform.GetChild(randNum).gameObject.SetActive(true);
    }

    public void ActivateStations()
    {
        // STATIONS
        StationColor currentStation = LevelManager.Instance != null ?
            LevelManager.Instance.currStation :
            TutorialManager.Instance.currStation;

        StationColor nextStation = LevelManager.Instance != null ?
            StationCycler.GetNextStation(currentStation, ref LevelManager.Instance.currDirection) :
            StationCycler.GetNextStation(currentStation, ref TutorialManager.Instance.currDirection);

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
        
        //_spawnedEnvironment.GetComponent<SectionMovement>().startPosition = environmentSpawnPoint.position;
        //_spawnedEnvironment.GetComponent<SectionMovement>().ResetTravel();
    }

    public GameObject GetNextStation(Transform parent)
    {
        int activeChild = 0;

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (child.activeInHierarchy)
            {
                activeChild++;

                if (TutorialManager.Instance != null)
                {
                    if (activeChild == 2 && TutorialManager.Instance.currDirection == TrainDirection.Right) // Get the second active child
                    {
                        return child;
                    }
                }

                if (activeChild == 2 && LevelManager.Instance.currDirection == TrainDirection.Right) // Get the second active child
                {
                    return child;
                }
                else if (activeChild == 1 && LevelManager.Instance.currDirection == TrainDirection.Left)
                {
                    return child;
                }
            }
        }

        return null;
    }
}
