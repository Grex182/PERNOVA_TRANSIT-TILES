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
        if (LevelManager.Instance.currDirection == TrainDirection.Right)
        {
            if (transform.parent.transform.position.x > 250)
            {
                ResetTilesParent();
            }
        }
        else
        {
            if (transform.parent.transform.position.x < -250)
            {
                ResetTilesParent();
            }
        }
        
        
    }

    private void ResetTilesParent()
    {
        SetParent();
        passengerSpawner.DeletePassengers();
        boardManager.VacateStationTiles(true);
        passengerSpawner.SpawnPassengers();
    }

    private void SetParent()
    {  
            GameObject newParent = WorldGenerator.Instance.GetNextStation(WorldGenerator.Instance.stationsParent.transform);
            transform.SetParent(newParent.transform, false);
    }
}
