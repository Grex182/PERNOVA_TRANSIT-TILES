using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationManager : Singleton<StationManager>
{
    [Header("Timers & Integers")]
    [SerializeField] private float stationTime;
    [SerializeField] private float trainDecelerationDelay = 1f;

    [Header("Booleans")]
    [SerializeField] public bool isTrainMoving = false;
    [SerializeField] public bool hasGameStarted = true;
    [SerializeField] public bool isMovingLeft = false;
    [SerializeField] public bool hasPassengersSpawned = false;

    public int currentStationIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    private void Start()
    {

        //Debug.Log("Number of Stations: " + System.Enum.GetValues(typeof(StationColor)).Length);

    }

    public IEnumerator DecelerationDelay(GameObject stageSection)
    {
        yield return new WaitForSeconds(trainDecelerationDelay);

        if (stageSection != null)
        Destroy(stageSection);

        //GameManager.instance.Board.EnablePlatformTiles();
        isTrainMoving = false;

        Debug.Log("Train has stopped");
    }

    public void UpdateStationColor()
    {
        // Get total number of station colors
        int totalStations = System.Enum.GetValues(typeof(StationColor)).Length;

        // Update index
        currentStationIndex += direction;

        // If we hit the bounds, reverse direction
        if (currentStationIndex >= totalStations - 1)
        {
            currentStationIndex = totalStations - 1; // go one step before last
            direction = -1;

            isMovingLeft = true;
        }
        else if (currentStationIndex <= 0)
        {
            currentStationIndex = 0; // go one step after first
            direction = 1;

            isMovingLeft = false;
        }

    }
}
