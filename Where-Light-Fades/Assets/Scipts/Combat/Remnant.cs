using UnityEngine;

public class Remnant : MonoBehaviour
{
    [Header("Remnant Settings")]
    public float healthRestore = 15f;
    public float manaRestore = 10f;
    public float magnetRange = 3f;
    public float magnetSpeed = 8f;
    public float collectRange = 0.5f;

    [Header("Drop Effect")]
    public float throwForceX = 2f;
    public float throwForceY = 4f;
    public float dropDelay = 0.5f; // Time before it can be collected

    private Transform player;
    private Rigidbody2D rb;
    private bool isBeingPulled = false;
    private bool canBeCollected = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        // Apply throw effect
        float randomDirection = Random.Range(-1f, 1f) > 0 ? 1f : -1f;
        rb.velocity = new Vector2(throwForceX * randomDirection, throwForceY);
        rb.gravityScale = 1f;

        // Enable collection after delay
        Invoke("EnableCollection", dropDelay);
    }

    void EnableCollection()
    {
        canBeCollected = true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (!canBeCollected || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Start magnetizing when player is in range
        if (distance <= magnetRange)
        {
            isBeingPulled = true;
        }

        // Move towards player
        if (isBeingPulled)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);

            // Check if close enough to collect
            if (distance <= collectRange)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        // Restore player stats
        PlayerStats.Instance.Heal(healthRestore);
        PlayerStats.Instance.RestoreMana(manaRestore);

        Debug.Log($"Remnant collected! Restored {healthRestore} HP and {manaRestore} Mana");

        Destroy(gameObject);
    }

    // Visualize ranges in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}