using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;

    [Header("Attack")]
    public float jumpForce = 10f;
    public float jumpCooldown = 2f;
    public float attackDamage = 10f;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private bool hasJumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            // Move towards player
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        // Jump attack if in range
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + jumpCooldown)
        {
            JumpAttack();
        }
    }

    void JumpAttack()
    {
        lastAttackTime = Time.time;
        hasJumped = false;

        // Calculate jump direction towards player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * jumpForce * 0.7f, jumpForce);

        Debug.Log($"{gameObject.name} is jumping to attack!");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage player on contact during jump
        if (collision.gameObject.CompareTag("Player") && !hasJumped)
        {
            PlayerStats.Instance.TakeDamage(attackDamage);
            hasJumped = true;
            Debug.Log("Enemy hit player!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}