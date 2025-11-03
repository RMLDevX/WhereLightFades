using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float safeDistance = 8f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float runSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;

    [Header("Obstacle Detection")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public float wallCheckDistance = 0.5f;
    public float edgeCheckDistance = 0.6f;

    [Header("Dodge Behavior")]
    public float dodgeRange = 2f;
    public float dodgeJumpForce = 12f;
    public float dodgeCooldown = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isRunningAway = false;
    private bool isGrounded = false;
    private float currentRunDirection = 0f;
    private float lastDodgeTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        CheckGrounded();

        // Run away if player is in detection range but not too far
        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            RunAwayFromPlayer();
        }
        else if (distanceToPlayer > safeDistance)
        {
            // Stop running if player is far enough
            StopRunning();
        }

        // Handle edge detection and direction change
        if (isRunningAway && isGrounded)
        {
            HandleEdgeDetection();
        }

        // Check for dodge opportunity (player very close)
        if (isRunningAway && distanceToPlayer <= dodgeRange && isGrounded)
        {
            TryDodgePlayer();
        }

        // Still can attack if player gets too close
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log($"{gameObject.name} is cornered!");
        }
    }

    void FixedUpdate()
    {
        // Handle physics-based movement in FixedUpdate
        if (isRunningAway)
        {
            MoveAwayFromPlayer();
        }
    }

    void RunAwayFromPlayer()
    {
        isRunningAway = true;

        // Calculate initial run direction away from player
        currentRunDirection = GetRunAwayDirection();
        transform.localScale = new Vector3(Mathf.Sign(currentRunDirection), 1f, 1f);
    }

    void StopRunning()
    {
        isRunningAway = false;
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    void MoveAwayFromPlayer()
    {
        if (!isGrounded) return;

        // Check for obstacles in the running direction
        if (!IsPathBlocked(currentRunDirection))
        {
            rb.velocity = new Vector2(currentRunDirection * runSpeed, rb.velocity.y);
        }
    }

    void HandleEdgeDetection()
    {
        // Check if we're at the edge of the platform
        if (IsAtEdge(currentRunDirection))
        {
            ChangeDirection();
        }
    }

    void ChangeDirection()
    {
        // Simply reverse the run direction at edges
        currentRunDirection = -currentRunDirection;
        transform.localScale = new Vector3(Mathf.Sign(currentRunDirection), 1f, 1f);

        Debug.Log($"{gameObject.name} changed direction at edge!");
    }

    void TryDodgePlayer()
    {
        // Only dodge if not on cooldown and player is very close
        if (Time.time >= lastDodgeTime + dodgeCooldown)
        {
            // Jump to dodge the player
            PerformDodgeJump();
        }
    }

    void PerformDodgeJump()
    {
        lastDodgeTime = Time.time;

        // Jump in the current run direction (away from player)
        float horizontalForce = currentRunDirection * runSpeed * 1.5f;
        float verticalForce = dodgeJumpForce;

        rb.velocity = new Vector2(horizontalForce, verticalForce);

        Debug.Log($"{gameObject.name} is dodging the player!");
    }

    float GetRunAwayDirection()
    {
        // Returns -1 if player is on right (run left), 1 if player is on left (run right)
        return player.position.x > transform.position.x ? -1f : 1f;
    }

    void CheckGrounded()
    {
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * 0.1f;
        isGrounded = Physics2D.Raycast(checkPosition, Vector2.down, groundCheckDistance, groundLayer);
    }

    bool IsPathBlocked(float direction)
    {
        Vector2 checkPosition = (Vector2)transform.position;

        // Check for walls
        bool wallAhead = Physics2D.Raycast(checkPosition,
            new Vector2(direction, 0), wallCheckDistance, groundLayer);

        // Check for pits
        Vector2 edgeCheckPosition = checkPosition + new Vector2(direction * edgeCheckDistance, 0);
        bool groundAhead = Physics2D.Raycast(edgeCheckPosition,
            Vector2.down, groundCheckDistance * 2f, groundLayer);

        return wallAhead || !groundAhead;
    }

    bool IsAtEdge(float direction)
    {
        if (!isGrounded) return false;

        // Check if there's ground ahead in the movement direction
        Vector2 edgeCheckPosition = (Vector2)transform.position + new Vector2(direction * edgeCheckDistance, 0);
        bool groundAhead = Physics2D.Raycast(edgeCheckPosition,
            Vector2.down, groundCheckDistance * 2f, groundLayer);

        return !groundAhead;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage player only if they get too close (cornered enemy)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats.Instance.TakeDamage(attackDamage);
            Debug.Log("Enemy hit player while cornered!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        // Draw dodge range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, dodgeRange);

        // Draw ground check
        Gizmos.color = Color.blue;
        Vector2 groundCheckPos = (Vector2)transform.position + Vector2.down * 0.1f;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector2.down * groundCheckDistance);

        // Draw edge check
        if (Application.isPlaying && isRunningAway)
        {
            Gizmos.color = Color.cyan;
            Vector2 edgeCheckPos = (Vector2)transform.position + new Vector2(currentRunDirection * edgeCheckDistance, 0);
            Gizmos.DrawLine(edgeCheckPos, edgeCheckPos + Vector2.down * groundCheckDistance * 2f);
        }

        // Draw wall check
        Gizmos.color = Color.white;
        Vector2 wallCheckPos = (Vector2)transform.position;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + new Vector2(currentRunDirection * wallCheckDistance, 0));
    }
}