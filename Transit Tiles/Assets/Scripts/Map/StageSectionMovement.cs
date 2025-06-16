using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSectionMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 2f; // Units per second
    [SerializeField] private float deceleration = 0.5f; // How fast it slows down
    [SerializeField] private float currentSpeed = 0f;

    private bool hasStopped = false;

    private void Awake()
    {
        if (!GameManager.instance.StationManager.hasGameStarted)
        {
            currentSpeed = moveSpeed;
        }
    }

    private void Update()
    {
        float targetSpeed = GameManager.instance.StationManager.isTrainMoving ? moveSpeed : 0f;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * deceleration);

        if (!GameManager.instance.StationManager.isMovingRight)
        {
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.left * currentSpeed * Time.deltaTime;
        }

        // Trigger EnablePlatformTiles only once when the train stops
        if (!GameManager.instance.StationManager.isTrainMoving && currentSpeed > -0.01f && !hasStopped)
        {
            if (!GameManager.instance.StationManager.hasGameStarted)
            {
                GameManager.instance.Board.GetComponent<SpawnTiles>().EnablePlatformTiles();
                GameManager.instance.StationManager.UpdateStationColor();
                StartCoroutine(GameManager.instance.StationManager.StartStationTimer());
            }

            hasStopped = true;
        }
/*        else if (GameManager.instance.StationManager.isMovingRight && !GameManager.instance.StationManager.isTrainMoving && currentSpeed < 0.01f && !hasStopped)
        {
            if (!GameManager.instance.StationManager.hasGameStarted)
            {
                GameManager.instance.Board.EnablePlatformTiles();
                StartCoroutine(GameManager.instance.StationManager.StartStationTimer());
            }

            hasStopped = true;
        }*/

        // Reset the flag if the train starts moving again
        if (GameManager.instance.StationManager.isTrainMoving)
        {
            hasStopped = false;
        }
    }
}