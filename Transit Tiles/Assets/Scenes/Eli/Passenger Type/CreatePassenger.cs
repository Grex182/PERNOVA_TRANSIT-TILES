using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePassenger : MonoBehaviour
{
    // NOTE:
    // - Spawn point #1 and #2 has opposite direction to simulate the top and bottom entrances
    // - Passenger prefab map will be populated further soon

    [Header("References")]
    [SerializeField] private GameObject[] _passengerPrefabs;
    [SerializeField] private Transform[] _spawnPoints;
    private Dictionary<string, GameObject> _passengerPrefabMap;

    [SerializeField] private LayerMask _tileLayerMask;
    private const string ColorProperty = "_Color";
    private const float SpawnHeightOffset = 1f;
    private const float TileCheckRadius = 0.5f;
    private Quaternion passengerRotation;

    #region PASENGER TYPES
    private const string Standard = "Standard";
    private const string BulkyElder = "BulkyElder";
    private const string Elder = "Elder";
    private const string Parent = "Parent";
    private const string Pregnant = "Pregnant";
    private const string Injured = "Injured";
    private const string Noisy = "Noisy";
    private const string Stinky = "Stinky";
    private const string Sleepy = "Sleepy";
    #endregion

    private void Awake()
    {
        _passengerPrefabMap = new Dictionary<string, GameObject> // TODO: Populate once more passenger types are available
        {
            { Standard, _passengerPrefabs[0] },
            { Elder, _passengerPrefabs[1] }
        };
    }
    private void Start()
    {
        SpawnPassenger(GetAvailableSpawnPoint(), Standard);
        SpawnPassenger(GetAvailableSpawnPoint(), Standard);
        SpawnPassenger(GetAvailableSpawnPoint(), Elder);
    }

    public void SpawnPassenger(Vector3 spawnPoint, string passengerType)
    {
        #region NULL-CHECKS
        if (!_passengerPrefabMap.TryGetValue(passengerType, out var passengerPrefab))
        {
            Debug.LogError($"Passenger prefab not found for type: {passengerType}");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Passenger spawn point is not assigned");
            return;
        }
        #endregion

        // Get Passenger Information
        var passengerData = PassengerTypes.Passenger.CreatePassenger(
            passengerType, 
            Random.Range(1, 7) // Set random station
        ); 

        // Spawn Passenger
        var newPassenger = Instantiate(
            passengerPrefab, 
            spawnPoint,
            passengerRotation
        );

        if (!SetPassengerStation(newPassenger, passengerData.StationColor))
        {
            Debug.LogError($"Failed to set color for {passengerType} passenger");
        }
    }

    private bool SetPassengerStation(GameObject passenger, string stationColor)
    {
        #region NULL-CHECKS
        if (!passenger.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            Debug.LogError("No MeshRenderer found on passenger prefab.");
            return false;
        }
        #endregion

        var material = meshRenderer.material;
        material.SetColor(ColorProperty, GetStationColor(stationColor));
        return true;
    }

    private Color GetStationColor(string stationColor)
    {
        switch (stationColor)
        {
            case "Pink": return Color.magenta;
            case "Red": return Color.red;
            case "Orange": return new Color(1f, 0.5f, 0f);
            case "Yellow": return Color.yellow;
            case "Green": return Color.green;
            case "Blue": return Color.blue;
            case "Violet": return new Color(0.5f, 0f, 1f);
            default: return Color.white;
        }
    }

    private Vector3 GetAvailableSpawnPoint()
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            if (!IsTileOccupied(spawnPoint.position))
            {
                spawnPoint.tag = "Occupied"; // Set Tag to Occupied ( set to "Untagged" when tile is not populated)
                passengerRotation = spawnPoint.localRotation;

                return new Vector3(
                    spawnPoint.position.x,
                    spawnPoint.position.y + SpawnHeightOffset,
                    spawnPoint.position.z
                );
            }
        }

        Debug.LogWarning("No available spawn points found");
        return Vector3.zero;
    }

    private bool IsTileOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position,
            TileCheckRadius,
            _tileLayerMask
        );

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Occupied"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var spawnPoint in _spawnPoints)
        {
            Gizmos.DrawWireSphere(spawnPoint.position, TileCheckRadius);
        }
    }
}
