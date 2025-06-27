using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class SectionMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float _currentSpeed = 0f;
    [SerializeField] private readonly float _maxSpeed = 20f;

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
    }

    private void Update()
    {
        HandleMovementState();

        Vector3 newPosition = transform.position;
        newPosition.x += _currentSpeed * Time.deltaTime;
        transform.position = newPosition;
    }

    private void HandleMovementState()
    {
        switch (WorldGenerator.Instance.currState)
        {
            case MovementState.Accelerate:
                _currentSpeed = 5;
                break;

            case MovementState.Decelerate:
                _currentSpeed = 5;
                break;

            case MovementState.Stationary:
                _currentSpeed = 0;
                break;

            case MovementState.Moving:
                _currentSpeed = _maxSpeed;
                break;
        }
    }
}
