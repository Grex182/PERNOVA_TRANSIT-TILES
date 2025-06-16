using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StationColor
{
    Red,
    Pink,
    Orange,
    Yellow,
    Green,
    Blue,
    Violet
}

public class StationManager : Singleton<StationManager>
{
    [Header("Colors")]
    [SerializeField] public StationColor stationColor;

    [Header("Timers & Integers")]
    [SerializeField] private float stationTime;
    [SerializeField] private float trainDecelerationDelay = 1f;

    [Header("Booleans")]
    [SerializeField] public bool isTrainMoving = false;
    [SerializeField] public bool hasGameStarted = true;
    [SerializeField] public bool isMovingRight = false;

    public int currentStationIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    private void Start()
    {
        GameManager.instance.StationManager = this;

        stationColor = StationColor.Red;

        Debug.Log("Number of Stations: " + System.Enum.GetValues(typeof(StationColor)).Length);

        StartCoroutine(StartStationTimer());
    }

    public IEnumerator StartStationTimer()
    {
        yield return new WaitForSeconds(stationTime);

        GameManager.instance.Board.GetComponent<SpawnTiles>().DisablePlatformTiles();
        isTrainMoving = true;

        if (hasGameStarted)
        {
            hasGameStarted = false;
        }

        Debug.Log("Train is now moving");

        //StartCoroutine(TravelTimer());
    }

/*    public void DecelerateTrain()
    {
        StartCoroutine(DecelerationDelay());
    }*/

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

            isMovingRight = true;
        }
        else if (currentStationIndex <= 0)
        {
            currentStationIndex = 0; // go one step after first
            direction = 1;

            isMovingRight = false;
        }

        // Set new station color
        stationColor = (StationColor)currentStationIndex;

        Debug.Log("The Train has arrived at: " + stationColor + "Station.");
    }
}
