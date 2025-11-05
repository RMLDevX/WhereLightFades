using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float safeDistance = 8f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;

    [Header("Patrol Behavior")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public float wallCheckDistance = 0.5f;
    public float edgeCheckDistance = 0.6f;
    public float patrolChangeTime = 3f;

    [Header("Shock Reaction")]
    public float shockDuration = 0.5f;

    [Header("Physics")]
    public bool usePhysicsMovement = true; // Toggle between physics and direct movement

    private Transform player;
    private Rigidbody2D rb;
    private bool isRunningAway = false;
    private bool isGrounded = false;
    private bool isInShock = false;
    private bool hasFreakedOut = false;
    private float currentDirection = 1f;
    private float lastDirectionChangeTime = 0f;
    private bool isPatrolling = true;
    private float shockEndTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configure Rigidbody2D to prevent unwanted rotation
        if (rb != null)
        {
            rb.freezeRotation = true; // Prevent falling over
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        UpdateFacingDirection();
    }

    void Update()
    {
        if (player == null) return;

        CheckGrounded();

        // Handle shock state first
        if (isInShock)
        {
            if (Time.time >= shockEndTime)
            {
                isInShock = false;
                StartRunningAway();
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Normal patrol behavior when player is far
        if (distanceToPlayer > safeDistance)
        {
            isRunningAway = false;
            isPatrolling = true;
            hasFreakedOut = false;
            Patrol();
        }
        // Player detected - go into shock first, then run away
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            if (!isRunningAway && !isInShock)
            {
                EnterShock();
            }
            else if (isRunningAway)
            {
                UpdateRunAwayDirection();
            }
        }
        // Stop and freak out when player is very close (only once)
        else if (distanceToPlayer <= attackRange && !hasFreakedOut)
        {
            FreakOut();
        }
        // If player is in attack range but already freaked out, just run away
        else if (distanceToPlayer <= attackRange && hasFreakedOut)
        {
            isRunningAway = true;
            isPatrolling = false;
            UpdateRunAwayDirection();
        }

        // Handle obstacle detection for both patrol and running
        if (isGrounded && (isPatrolling || isRunningAway))
        {
            HandleObstacleDetection();
        }

        // Apply movement (choose one method)
        if (!usePhysicsMovement && !isInShock)
        {
            ApplyDirectMovement();
        }
    }

    void FixedUpdate()
    {
        // Only use physics movement if enabled
        if (usePhysicsMovement && !isInShock)
        {
            ApplyPhysicsMovement();
        }
    }

    void ApplyPhysicsMovement()
    {
        if (!isGrounded) return;

        Vector2 targetVelocity = Vector2.zero;

        if (isPatrolling)
        {
            targetVelocity = new Vector2(currentDirection * walkSpeed, rb.velocity.y);
        }
        else if (isRunningAway)
        {
            targetVelocity = new Vector2(currentDirection * runSpeed, rb.velocity.y);
        }
        else
        {
            // Stop horizontal movement but maintain vertical physics (falling, etc.)
            targetVelocity = new Vector2(0f, rb.velocity.y);
        }

        rb.velocity = targetVelocity;
    }

    void ApplyDirectMovement()
    {
        if (!isGrounded) return;

        Vector3 movement = Vector3.zero;

        if (isPatrolling)
        {
            movement = new Vector3(currentDirection * walkSpeed * Time.deltaTime, 0, 0);
        }
        else if (isRunningAway)
        {
            movement = new Vector3(currentDirection * runSpeed * Time.deltaTime, 0, 0);
        }

        transform.Translate(movement);
    }

    void EnterShock()
    {
        isInShock = true;
        isPatrolling = false;
        isRunningAway = false;
        shockEndTime = Time.time + shockDuration;

        // Stop immediately
        if (rb != null && usePhysicsMovement)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        Debug.Log($"{gameObject.name} is shocked! Player detected!");
    }

    void StartRunningAway()
    {
        isRunningAway = true;
        isPatrolling = false;
        UpdateRunAwayDirection();
        Debug.Log($"{gameObject.name} is running away in panic!");
    }

    void UpdateRunAwayDirection()
    {
        currentDirection = GetRunAwayDirection();
        UpdateFacingDirection();
    }

    void Patrol()
    {
        if (Time.time >= lastDirectionChangeTime + patrolChangeTime)
        {
            ChangeDirection();
            lastDirectionChangeTime = Time.time;
        }
    }

    void FreakOut()
    {
        isRunningAway = false;
        isPatrolling = false;
        hasFreakedOut = true;

        // Stop movement
        if (rb != null && usePhysicsMovement)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        Debug.Log($"{gameObject.name} is freaking out! Player too close!");
        Invoke(nameof(StartRunningAway), 0.8f);
    }

    void HandleObstacleDetection()
    {
        if (IsPathBlocked(currentDirection))
        {
            ChangeDirection();

            if (isRunningAway)
            {
                Debug.Log($"{gameObject.name} panicked and turned around!");
            }
        }
    }

    void ChangeDirection()
    {
        currentDirection = -currentDirection;
        UpdateFacingDirection();
        lastDirectionChangeTime = Time.time;
    }

    void UpdateFacingDirection()
    {
        transform.localScale = new Vector3(Mathf.Sign(currentDirection), 1f, 1f);
    }

    float GetRunAwayDirection()
    {
        return player.position.x > transform.position.x ? -1f : 1f;
    }

    void CheckGrounded()
    {
        Vector2 checkPosition = (Vector2)transform.position + Vector2.down * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    bool IsPathBlocked(float direction)
    {
        Vector2 checkPosition = (Vector2)transform.position;

        // Check for walls
        bool wallAhead = Physics2D.Raycast(checkPosition,
            new Vector2(direction, 0), wallCheckDistance, groundLayer);

        // Check for pits - use a downward raycast in front of the enemy
        Vector2 edgeCheckPosition = checkPosition + new Vector2(direction * edgeCheckDistance, -0.1f);
        bool groundAhead = Physics2D.Raycast(edgeCheckPosition,
            Vector2.down, groundCheckDistance, groundLayer);

        return wallAhead || !groundAhead;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage player only if they get too close (cornered enemy)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Optional: Add a cooldown to prevent rapid damage
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
                Debug.Log("Enemy hit player while cornered!");
            }
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

        // Draw ground check
        Gizmos.color = Color.blue;
        Vector2 groundCheckPos = (Vector2)transform.position + Vector2.down * 0.1f;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector2.down * groundCheckDistance);

        // Draw edge check
        Gizmos.color = Color.cyan;
        Vector2 edgeCheckPos = (Vector2)transform.position + new Vector2(currentDirection * edgeCheckDistance, -0.1f);
        Gizmos.DrawLine(edgeCheckPos, edgeCheckPos + Vector2.down * groundCheckDistance);

        // Draw wall check
        Gizmos.color = Color.white;
        Vector2 wallCheckPos = (Vector2)transform.position;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + new Vector2(currentDirection * wallCheckDistance, 0));

        // Visualize state
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"State: {(isInShock ? "SHOCK" : (isRunningAway ? "RUNNING" : (isPatrolling ? "PATROL" : "FREAKOUT")))}\nGrounded: {isGrounded}", style);
#endif
    }
}