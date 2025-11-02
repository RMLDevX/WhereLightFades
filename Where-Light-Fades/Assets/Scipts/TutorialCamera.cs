using UnityEngine;

public class TutorialSimpleSmoothCamera : MonoBehaviour
{
    public Transform target;
    public float smoothness = 0.1f;
    public Vector3 offset = new Vector3(0, 1, -10);

    private Vector3 velocity = Vector3.zero;
    private TutorialPlayerMovement playerMovement;
    private float highestXPosition;

    void Start()
    {
        if (target != null)
        {
            playerMovement = target.GetComponent<TutorialPlayerMovement>();
            // Initialize with player's starting position
            Vector3 startPosition = target.position + offset;
            transform.position = startPosition;
            highestXPosition = target.position.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition;

        // Check if player is facing right AND has moved beyond the current camera X position
        if (playerMovement != null && playerMovement.IsFacingRight() && target.position.x > highestXPosition)
        {
            // Follow player when facing right and moving forward
            targetPosition = target.position + offset;
            highestXPosition = target.position.x; // Update the highest X position
        }
        else
        {
            // Stay at current X position but follow Y movement
            targetPosition = new Vector3(highestXPosition + offset.x, target.position.y + offset.y, offset.z);
        }

        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }
}