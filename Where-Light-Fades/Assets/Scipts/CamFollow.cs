using UnityEngine;

public class SimpleSmoothCamera : MonoBehaviour
{
    public Transform target;
    public float smoothness = 0.1f;
    public Vector3 offset = new Vector3(0, 1, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }
}