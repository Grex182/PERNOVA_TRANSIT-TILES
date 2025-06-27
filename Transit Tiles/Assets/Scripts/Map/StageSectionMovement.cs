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
        if (!StationManager.Instance.hasGameStarted)
        {
            currentSpeed = moveSpeed;
        }
    }

    private void Update()
    {
        float targetSpeed = StationManager.Instance.isTrainMoving ? moveSpeed : 0f;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * deceleration);

        if (StationManager.Instance.isMovingLeft)
        {
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.left * currentSpeed * Time.deltaTime;
        }

        // Trigger EnablePlatformTiles only once when the train stops
        if (!StationManager.Instance.isTrainMoving && currentSpeed > -0.01f && !hasStopped)
        {
            if (!StationManager.Instance.hasGameStarted)
            {
                GameManager.Instance.Board.GetComponent<SpawnTiles>().EnablePlatformTiles();
                StationManager.Instance.UpdateStationColor();
                StartCoroutine(StationManager.Instance.StartStationTimer());
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
        if (StationManager.Instance.isTrainMoving)
        {
            hasStopped = false;
        }
    }
}