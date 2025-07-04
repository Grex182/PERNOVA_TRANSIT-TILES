using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;

public class SectionMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float _time;
    [SerializeField] private readonly float _distTravel = 430f;
    [SerializeField] private readonly float _distAcceleration = 25f;
    [SerializeField] private readonly float _distDeceleration = 25f;

    [Header("Calculated Values")]
    [SerializeField] private float _speedConst;
    [SerializeField] public float _speedCurr; 
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] public float _decelTimer;
    [SerializeField] private float _accelTimer = 0;
    [SerializeField] private float _elapsedTime = 0;
    [SerializeField] private float _distanceCovered;
    [SerializeField] public float _displacement = 0;
    [SerializeField] public Vector3 startPosition;
    [SerializeField] private int _phase = 3;
    [SerializeField] private bool isTraveling = false;

    // Start is called before the first frame update
    void Start()
    {
        GetMotionValues();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (LevelManager.Instance.isTraveling && !isTraveling)
        {
            _phase = 0; // Set phase to Acceleration
            isTraveling = true; // Set traveling state to true
        }
        if (isTraveling)
        {
            switch (_phase)
            {
                case 0: // Acceleration Phase
                    AccelerationPhase();
                    break;

                case 1: // Travel Phase
                    TravelPhase();
                    break;

                case 2: // Deceleration Phase
                    DecelerationPhase();
                    break;

                case 3: // Stop Phase
                    StopPhase();
                    break;
            }
        }
    }

    private void GetMotionValues()
    {
        _time = LevelManager.Instance._travelPhaseTimer;

        //Get Constant Speed for Travel Phase
        _speedConst = _distTravel / _time;
        _speedCurr = 0f;
        //acceleration formula
        _acceleration = Mathf.Pow(_speedConst, 2.0f) / (2.0f * _distAcceleration);
        _accelTimer = _distAcceleration * 2.0f / _speedConst;
        //deceleration formula
        _deceleration = Mathf.Pow(_speedConst, 2.0f) / (2.0f * _distDeceleration);
        _decelTimer = _distDeceleration * 2.0f / _speedConst; // Calculate deceleration time

        LevelManager.Instance.decelerationTimer = _decelTimer; // Set deceleration timer in LevelManager

        //set starting position for movement
        startPosition = transform.position;
    }

    public void ResetTravel()
    {
        //set starting position for movement
        startPosition = transform.position;

        // Reset elapsed time and distance covered
        _elapsedTime = 0f;
        _distanceCovered = 0f;
        _displacement = 0f; // Reset displacement
    }


    private void AccelerationPhase()
    {
        if (_elapsedTime < _accelTimer)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;

            //calculate distance covered over time
            _distanceCovered = 0.5f * _acceleration * (Mathf.Pow(_elapsedTime, 2));
            _speedCurr = (_elapsedTime * _acceleration)/_speedConst; // Update current speed based on elapsed time and acceleration

            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;
        }
        else
        {
            transform.position = startPosition + Vector3.right * _distAcceleration;

            ResetTravel(); // Reset elapsed time and distance covered for travel phase

            _phase = 1; // Move to Travel Phase
        }
    }

    private void TravelPhase()
    {
        if (_elapsedTime < _time)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;
            //calculate distance covered over time
            _distanceCovered = _speedConst * _elapsedTime;
            _speedCurr = 1f; // Update current speed to constant speed
            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;
        }
        else
        {
            //Move object from initial position to exact target distance
            transform.position = startPosition + Vector3.right * _distTravel;

            ResetTravel(); // Reset elapsed time and distance covered for travel phase

            _phase = 2; // Move to Deceleration Phase
        }
    }

    private void DecelerationPhase()
    {
        if (_elapsedTime < _decelTimer)
        {
            //Time used for calculations
            _elapsedTime += Time.deltaTime;

            //calculate distance covered over time
            _distanceCovered = (_speedConst * _elapsedTime) - (0.5f * _deceleration * (Mathf.Pow(_elapsedTime, 2)));
            _speedCurr = (_speedConst - (_deceleration * _elapsedTime))/ _speedConst; // Update current speed based on elapsed time and deceleration
            //Move object from initial position to target distance
            transform.position = startPosition + Vector3.right * _distanceCovered;
            //Debug.Log("Deceleration Phase Ongoing");
        }
        else
        {
            transform.position = startPosition + Vector3.right * _distDeceleration;

            ResetTravel(); // Reset elapsed time and distance covered for travel phase


            _phase = 3 ;

        }
    }

    private void StopPhase()
    {
        // Reset position to start position
        transform.position = startPosition;
        LevelManager.Instance.hasTraveled = true; // Mark travel as completed
        isTraveling = false; // Reset traveling state

    }
}
