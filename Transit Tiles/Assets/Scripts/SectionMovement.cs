using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    private void Update()
    {
        HandleMovementState();

        float targetSpeed = isTrainMoving ? _maxSpeed : 0f;

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _unit);

        if (WorldGenerator.Instance.trainDirection == TrainDirection.Right)
        {
            transform.position += Vector3.right * _currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.left * _currentSpeed * Time.deltaTime;
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
}
