using UnityEngine;

public class MovingClouds : MonoBehaviour
{
    private Vector3 startPosition;   // Starting position
    private float baseSize = 1f;     // Base size of the cloud
    private float sizeVariation = 0.2f; // How much size changes
    private float speed = 1f;        // Movement speed
    private Vector3 direction = Vector3.right; // Movement direction
    private float displacement = 2f; // How far it moves from start

    private float timer = 0f;
    private Transform cloudTransform;

    private void Awake()
    {
        cloudTransform = transform;
        // Initialize start position if not set
        if (startPosition == Vector3.zero)
        {
            startPosition = cloudTransform.position;
        }
        //Generate Random Cloud Values
        baseSize = Random.Range(0.8f, 1.2f);
        sizeVariation = Random.Range(0.1f, 0.3f);
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        displacement = Random.Range(2f, 10f);
        speed = Random.Range(0.5f, 2f) / displacement;
    }

    private void Update()
    {
        // Increment timer based on time and speed
        timer += Time.deltaTime * speed;

        // Calculate sin wave value (-1 to 1 range)
        float sinValue = Mathf.Sin(timer);

        // Position oscillation using sine wave
        Vector3 currentDisplacement = direction * (sinValue * displacement);
        cloudTransform.position = startPosition + currentDisplacement;

        // Size oscillation using cosine wave (90° offset from position for variety)
        float sizeMultiplier = 1f + (Mathf.Cos(timer * 0.8f) * sizeVariation);
        cloudTransform.localScale = Vector3.one * (baseSize * sizeMultiplier);
    }

}