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
    public PassengerLocation currLocation;

    [Header("Passenger Design")]
    [SerializeField] private SkinnedMeshRenderer _shirtRenderer;
    [SerializeField] private SkinnedMeshRenderer _hairRenderer;
    [SerializeField] private SkinnedMeshRenderer _bottomsRenderer;

    [SerializeField] private Color[] _hairColors = new Color[]
    {
        new Color(0.2f, 0.1f, 0.05f), // Brown
        new Color(0.1f, 0.1f, 0.1f), // Black
        new Color(0.9f, 0.8f, 0.6f), // Blonde
        new Color(1f, 0.8f, 0.6f) // Red
    };

    [SerializeField] private Color[] _bottomsColors = new Color[]
    {
        new Color(0.2f, 0.2f, 0.2f), // Black
        new Color(0.5f, 0.5f, 0.5f), // White
        new Color(0.8f, 0.8f, 0.8f), // Blue
        new Color(0.1f, 0.5f, 0.1f) // Beige
    };


    public void Initialize(string passengerType, int stationNumber, PassengerLocation location)
    {
        Data = PassengerTypes.Passenger.CreatePassenger(passengerType, stationNumber);

        currLocation = location;

        DesignPassengerAppearance();

        ConfigureBehavior();
    }

    private void DesignPassengerAppearance()
    {
        if (_shirtRenderer != null)
        {
            _shirtRenderer.material.color = GetStationColor(Data.StationColor);
        }

        if (_hairRenderer != null)
        {
            _hairRenderer.material.color = _hairColors[Random.Range(0, _hairColors.Length)];
        }

        if (_bottomsRenderer != null)
        {
            _bottomsRenderer.material.color = _bottomsColors[Random.Range(0, _bottomsColors.Length)];
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
