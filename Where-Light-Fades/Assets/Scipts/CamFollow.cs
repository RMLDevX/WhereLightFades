using UnityEngine;

public class Scene2Camera : MonoBehaviour
{
    public float smoothness = 0.1f;
    public Vector3 offset = new Vector3(0, 1, -10);
    private Transform target;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Automatically find the player
        FindPlayer();
    }

    void Update()
    {
        // If target is null, try to find player again
        if (target == null)
        {
            FindPlayer();
            return;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("Player found for camera!");
        }
    }
}