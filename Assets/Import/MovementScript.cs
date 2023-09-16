using UnityEngine;

public class RandomCharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 45.0f; // Speed of rotation in degrees per second
    public float moveDuration = 1.0f;    // Duration of movement in seconds
    public float waitDuration = 1.0f;    // Duration of waiting in seconds

    private float moveTimer = 0.0f;
    private float waitTimer = 0.0f;
    private Vector3 targetDirection;

    void Start()
    {
        // Start by moving straight
        targetDirection = transform.forward;
        moveTimer = moveDuration;
    }

    void Update()
    {
        if (moveTimer > 0)
        {
            // Move in the current direction
            transform.Translate(targetDirection * moveSpeed * Time.deltaTime);
            moveTimer -= Time.deltaTime;
        }
        else if (waitTimer > 0)
        {
            // Wait still
            waitTimer -= Time.deltaTime;
        }
        else
        {
            // Generate a new random rotation offset
            float randomRotation = Random.Range(-rotationSpeed, rotationSpeed);

            // Rotate the character by the random offset
            transform.Rotate(Vector3.up * randomRotation);

            // Set the new direction to move
            targetDirection = transform.forward;

            // Reset timers
            moveTimer = moveDuration;
            waitTimer = waitDuration;
        }
    }
}
