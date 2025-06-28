using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SectionMovement;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;
using static UnityEngine.Rendering.CoreUtils;

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
    private readonly int maxSections = 7;
    private List<GameObject> _spawnedSections = new List<GameObject>();
    private List<GameObject> _spawnedRails = new List<GameObject>();

    [Header("Section Slots")]
    [SerializeField] public Transform[] _sectionSlots;

    [Header("States")]
    public bool hasStation = false;
    

    // get current station

    private void Update()
    {
        CheckForRecycling();
    }

    public void InitializeWorld()
    {
        foreach (var section in _spawnedSections)
        {
            Destroy(section);
        }
        _spawnedSections.Clear();

        foreach (var rail in _spawnedRails)
        {
            Destroy(rail);
        }
        _spawnedRails.Clear();

        for (int i = 0; i < maxSections; i++)
        {
            SpawnPlatform(i, _sectionSlots[i]);
        }

        for (int i = 0; i < maxSections; i++)
        {
            SpawnRail(i);
        }

        trainDirection = TrainDirection.Right;
    }

    private void SpawnPlatform(int index, Transform slotPosition)
    {
        Vector3 spawnPos = slotPosition.position;
        GameObject newPlatform = Instantiate(_platformPrefab, spawnPos, Quaternion.identity, transform);
        SectionMovement sectionMovement = newPlatform.GetComponent<SectionMovement>();

        int nextSlotIndex = (index + 1) % _sectionSlots.Length;
        sectionMovement.SetTarget(_sectionSlots[nextSlotIndex]);

        SetSectionToPlatform(sectionMovement);
        _spawnedSections.Add(newPlatform);
    }

    private void SpawnRail(int index)
    {
        int randNum = Random.Range(0, _spawnedRails.Count);
        Vector3 spawnPos = railSpawnPoint.position + new Vector3(index * _platformSpacing, 0, 0);
        GameObject newRail = Instantiate(_railPrefab[randNum], spawnPos, railSpawnPoint.localRotation, transform);
        
        _spawnedRails.Add(newRail);
    }


    private void CheckForRecycling()
    {
        for (int i = 0; i < _spawnedSections.Count; i++)
        {
            SectionMovement sectionMovement = _spawnedSections[i].GetComponent<SectionMovement>();

            if (_spawnedSections[i].transform.position.x >= _sectionSlots[_sectionSlots.Length - 1].position.x)
            {
                // Move section to the beginning of the chain
                Vector3 newPos = _spawnedSections[i].transform.position;
                newPos.x = _sectionSlots[0].position.x;
                _spawnedSections[i].transform.position = newPos;

                // Set new target (next slot in sequence)
                int newSlotIndex = 1;
                sectionMovement.SetTarget(_sectionSlots[newSlotIndex]);

                //// Optionally change section type when recycled
                //if (!hasStation && Random.Range(0, 5) == 0)
                //{
                //    SetSectionToStation(sectionMovement);
                //    hasStation = true;
                //}
                //else
                //{
                //    SetSectionToPlatform(sectionMovement);
                //}
            }
            else if (sectionMovement.HasReachedTarget)
            {
                // Always update target based on current position in the chain
                int currentSlotIndex = FindCurrentSlotIndex(_spawnedSections[i].transform.position);
                int nextSlotIndex = (currentSlotIndex + 1) % _sectionSlots.Length;
                sectionMovement.SetTarget(_sectionSlots[nextSlotIndex]);

                sectionMovement.HasReachedTarget = false;
            }
        }

        // Rail recycling remains the same
        float totalRailLength = _spawnedRails.Count * _platformSpacing;
        for (int i = 0; i < _spawnedRails.Count; i++)
        {
            if (_spawnedRails[i].transform.position.x >= _despawnXPos)
            {
                Vector3 newPos = _spawnedRails[i].transform.position;
                newPos.x -= totalRailLength;
                _spawnedRails[i].transform.position = newPos;
            }
        }
    }

    private int FindCurrentSlotIndex(Vector3 position)
    {
        // Find which slot this position is closest to
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < _sectionSlots.Length; i++)
        {
            float distance = Vector3.Distance(position, _sectionSlots[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void SetSectionToPlatform(SectionMovement section)
    {
        section._platformObj.SetActive(true);
        section._defaultStationObj.SetActive(false);
        section._railObj.SetActive(false);
    }

    private void SetSectionToStation(SectionMovement section)
    {
        section._platformObj.SetActive(false);
        section._defaultStationObj.SetActive(true);
        section._railObj.SetActive(false);
    }

    private void SetSectionToRail(SectionMovement section)
    {
        section._platformObj.SetActive(false);
        section._defaultStationObj.SetActive(false);
        section._railObj.SetActive(true);
    }
}
