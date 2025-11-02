// For platforms that should only exist in parallel world:
using UnityEngine;

public class ParallelPlatform : MonoBehaviour
{
    private Collider2D platformCollider;
    private ParallelWorldManager worldManager;

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
        worldManager = FindObjectOfType<ParallelWorldManager>();

        // Start disabled if in parallel world
        if (transform.parent.name.Contains("Parallel"))
        {
            platformCollider.enabled = false;
        }
    }

    void Update()
    {
        // Enable collider only in parallel world
        platformCollider.enabled = worldManager.isParallelWorldActive;
    }
}