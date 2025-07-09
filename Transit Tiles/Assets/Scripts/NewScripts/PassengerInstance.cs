using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerInstance : MonoBehaviour
{
    public enum PassengerLocation
    {
        Station,
        Train,
        Chair,
        Exit
    }

    // Passenger Data
    public PassengerTypes.Passenger Data { get; private set; }

    [SerializeField] public string _passengerType;
    public PassengerLocation currLocation;
    [SerializeField] public GameObject _collision;
    [SerializeField] private PassengerAppearance _passengerAppearance;
    

    [Header("Passenger Design")]
    [SerializeField] private SkinnedMeshRenderer _shirtRenderer;


    public void Initialize(int stationNumber, PassengerLocation location)
    {
        Data = PassengerTypes.Passenger.CreatePassenger(_passengerType, stationNumber);

        currLocation = location;

        _passengerAppearance.Initialize();

        _shirtRenderer = _passengerAppearance.topChild.GetComponent<SkinnedMeshRenderer>();

        DesignPassengerAppearance();

        ConfigureBehavior();
    }

    private void DesignPassengerAppearance()
    {
        if (_shirtRenderer != null)
        {
            _passengerAppearance.topChild.GetComponent<SkinnedMeshRenderer>().material.color = GetStationColor(Data.StationColor);
        }
    }

    private void ConfigureBehavior()
    {
        // Add any behavior-specific configuration here
        // Example: GetComponent<PassengerMovement>().SetSpeed(Data.TraitType);
    }

    private Color GetStationColor(string stationColor)
    {
        switch (stationColor)
        {
            case "Red": return LevelManager.Instance.stationColors[0];
            case "Pink": return LevelManager.Instance.stationColors[1];
            case "Orange": return LevelManager.Instance.stationColors[2];
            case "Yellow": return LevelManager.Instance.stationColors[3];
            case "Green": return LevelManager.Instance.stationColors[4];
            case "Blue": return LevelManager.Instance.stationColors[5];
            case "Violet": return LevelManager.Instance.stationColors[6];
            default: return Color.white;
        }
    }
}
