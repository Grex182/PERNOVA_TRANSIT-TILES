using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationTiles : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private PassengerSpawner passengerSpawner;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameObject centerPoint;

    // Start is called before the first frame update
    void Start()
    {
        //SetParent();
    }

    public void Initialize()
    {
        // This method can be used to initialize the station tile if needed
        // For example, setting up references or initial states
        SetParent();
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.Instance != null && 
            CheckDirectionAndPosition(LevelManager.Instance.currDirection))
        {
            ResetTilesParent();
        }
        else if (TutorialManager.Instance != null &&
            CheckDirectionAndPosition(TutorialManager.Instance.currDirection) &&
            (TutorialManager.Instance._currentTutorialIndex >= 9 && TutorialManager.Instance._currentTutorialIndex <= 11))
        {
            SetParent();
            passengerSpawner.DeletePassengers();
            boardManager.VacateStationTiles(true);
            passengerSpawner.SpawnPassengersSpecials();
        }
        else if (TutorialManager.Instance != null &&
            CheckDirectionAndPosition(TutorialManager.Instance.currDirection) &&
            TutorialManager.Instance._currentTutorialIndex == 21)
        {
            ResetTilesParent();
        }
    }

    private bool CheckDirectionAndPosition(TrainDirection direction)
    {
        float parentX = transform.parent.position.x;

        return (direction == TrainDirection.Right && parentX > 250) ||
               (direction == TrainDirection.Left && parentX < -250);
    }

    private void ResetTilesParent()
    {
        SetParent();
        passengerSpawner.DeletePassengers();
        boardManager.VacateStationTiles(true);

        if (LevelManager.Instance != null)
        {
            if (LevelManager.Instance.hasRushHourReg)
            {
                passengerSpawner.SpawnPassengersStandard(false);
            }
            else
            {
                passengerSpawner.SpawnPassengers();
            }
        }
        else
        {
            passengerSpawner.SpawnPassengers();
        }
    }

    private void SetParent()
    {  
        GameObject newParent = WorldGenerator.Instance.GetNextStation(WorldGenerator.Instance.stationsParent.transform);
        transform.SetParent(newParent.transform, false);
    }
}
