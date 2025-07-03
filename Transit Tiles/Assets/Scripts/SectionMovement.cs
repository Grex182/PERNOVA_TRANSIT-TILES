using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.CullingGroup;

public class SectionMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float _currentSpeed = 0f;
    [SerializeField] private readonly float _maxSpeed = 20f;
    [SerializeField] private float _unit = 0.5f;
    private bool isTrainMoving = false;

    [Header("Game Object References")]
    [SerializeField] public GameObject _platformObj;
    [SerializeField] public GameObject _defaultStationObj;
    [SerializeField] public GameObject _railObj;

    [SerializeField] private Transform _targetPosition;
    [SerializeField] private float _stopDistance = 0.01f;
    public bool HasReachedTarget = false;
    public bool isStation = false;

    private void Start()
    {
        if (GameManager.Instance.gameState == GameState.GameInit)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        _currentSpeed = 0f;
        isTrainMoving = false;
        HasReachedTarget = false;
    }

    private void Update()
    {
        HandleMovementState();
        MoveTowardsTarget();

        //float targetSpeed = isTrainMoving ? _maxSpeed : 0f;

        //_currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _unit);

        //if (WorldGenerator.Instance.trainDirection == TrainDirection.Right)
        //{
        //    transform.position += Vector3.right * _currentSpeed * Time.deltaTime;
        //}
        //else
        //{
        //    transform.position += Vector3.left * _currentSpeed * Time.deltaTime;
        //}
    }

    private void MoveTowardsTarget()
    {
        if (_targetPosition == null) return;

        Vector3 direction = _targetPosition.position - transform.position;
        float distanceToTarget = direction.magnitude;

        bool shouldStop = distanceToTarget < _stopDistance;

        float targetSpeed = isTrainMoving ? _maxSpeed : 0f;

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _unit);

        // Only move if we have distance to cover and train is moving
        if (distanceToTarget > _stopDistance && _currentSpeed > 0.1f && isTrainMoving)
        {
            // Normalize direction and move
            direction.Normalize();

            if (LevelManager.Instance.currDirection == TrainDirection.Right)
            {
                transform.position += Vector3.right * _currentSpeed;
            }
            else
            {
                transform.position += Vector3.left * _currentSpeed;
            }
        }
        else if (shouldStop)
        {
            // Snap to target when close enough
            transform.position = _targetPosition.position;
            HasReachedTarget = true;
        }
    }

    private void HandleMovementState()
    {
        switch (LevelManager.Instance.currState)
        {
            case MovementState.Station:
                _currentSpeed = 0f;
                break;

            case MovementState.Decelerate:
                isTrainMoving = false;
                break;

            case MovementState.Accelerate:
                isTrainMoving = true;
                break;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _targetPosition = newTarget;
    }
}
