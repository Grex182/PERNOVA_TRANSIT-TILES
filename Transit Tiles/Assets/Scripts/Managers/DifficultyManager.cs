using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LightingManager lightingManager;
    [SerializeField] private BillboardMovement billboard;

    [Header("Difficulty References")]
    private float difficulty = 1f;
    private float difficultyMultiplier = 1.05f;

    [Header("Spawn Rate")]
    public bool isRushHour; // No use yet
    [SerializeField] private int rushHourChance = 5;
    [SerializeField] private float rushHourMultiplier = 1.4f;
    [SerializeField] private int baselineRushHourChance = 5;

    public float passengerMultiplier = 0.5f;

    [Header("Station Duration")]
    [SerializeField] private float StationTimer = 15f;

    [Header("Trash")]
    public int trashSpawnChance = 4;

    public void UpdateDifficulty()
    {
        //Change Difficulty
        difficulty *= difficultyMultiplier;

        //Get Rush Hour 100%
        float time = lightingManager.TimeOfDay;
        bool isRushHourTime = IsInTimeRange(time, 7f, 9f) || IsInTimeRange(time, 17f, 19f);

        if (isRushHourTime) 
        { 
            DoRushHour(true); 
        }

        //Get Rush Hour Chance
        int rushRoll = Random.Range(0, 100);
        bool isRushHourChance = (IsInTimeRange(time, 12f, 14f) || IsInTimeRange(time, 22f, 24f)) || rushRoll < rushHourChance;

        if (isRushHourChance)
        {
            DoRushHour(true);
            rushHourChance = Mathf.RoundToInt(difficulty * baselineRushHourChance);
        }

        //Do OffPeak
        if (!isRushHourChance && !isRushHourTime) 
        {
            DoRushHour(false);
            rushHourChance = Mathf.RoundToInt(rushHourChance * rushHourMultiplier);
        }

        trashSpawnChance = Mathf.FloorToInt(trashSpawnChance + Mathf.Sqrt(difficulty));
        trashSpawnChance = Mathf.Clamp(trashSpawnChance, 1, 90);

        StationTimer = 15f - Mathf.Sqrt(difficulty);
        StationTimer = Mathf.Clamp(StationTimer, 8, 15);

    }

    private void DoRushHour(bool RushHour)
    {
        billboard.ChangeMessage(RushHour);
        isRushHour = RushHour;
        Debug.Log("Rush Hour " + RushHour + " ~Chance: " + rushHourChance);
        passengerMultiplier = RushHour ? 1f : 0.5f;
    }

    private bool IsInTimeRange(float time, float min, float max)
    {
        bool inRange = (time < min && time > max);
        return inRange;
    }
}
