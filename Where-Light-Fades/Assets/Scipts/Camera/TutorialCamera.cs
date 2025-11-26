using UnityEngine;

public class TutorialSimpleSmoothCamera : MonoBehaviour
{
    public Transform target;
    public float smoothness = 0.1f;
    public Vector3 offset = new Vector3(0, 1, -10);
    public float rightLimit = 50f;

    private Vector3 velocity = Vector3.zero;
    private TutorialPlayerMovement playerMovement;
    private float highestXPosition;

    void Start()
    {
        if (target != null)
        {
            playerMovement = target.GetComponent<TutorialPlayerMovement>();
            Vector3 startPosition = target.position + offset;
            transform.position = startPosition;
            highestXPosition = target.position.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition;

        // Calculate desired position based on player movement
        if (playerMovement != null && playerMovement.IsFacingRight() && target.position.x > highestXPosition)
        {
            highestXPosition = target.position.x;
        }

        // Apply right limit to the highestXPosition
        if (highestXPosition > rightLimit)
        {
            highestXPosition = rightLimit;
        }

        // Always follow Y, but X is limited by our highestXPosition (which now has the right limit applied)
        targetPosition = new Vector3(highestXPosition + offset.x, target.position.y + offset.y, offset.z);

        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(rightLimit, -100, 0), new Vector3(rightLimit, 100, 0));
    }
}