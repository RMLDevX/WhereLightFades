using UnityEngine;

public class ParallelNPC : MonoBehaviour
{
    private ParallelWorldManager worldManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D npcCollider;

    void Start()
    {
        worldManager = FindObjectOfType<ParallelWorldManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();

        // Start hidden if in parallel world
        if (transform.parent.name.Contains("Parallel"))
        {
            SetNPCVisible(false);
        }
    }

    void Update()
    {
        bool shouldBeVisible = worldManager.isParallelWorldActive;
        SetNPCVisible(shouldBeVisible);
    }

    void SetNPCVisible(bool visible)
    {
        spriteRenderer.enabled = visible;
        npcCollider.enabled = visible;
    }
}