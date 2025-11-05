using UnityEngine;

public class EnemyAI2 : MonoBehaviour
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
    public float shockDuration = 0.5f; // Time to stand still in shock

    private Transform player;
    private Rigidbody2D rb;
    private bool isRunningAway = false;
    private bool isGrounded = false;
    private bool isInShock = false;
    private bool hasFreakedOut = false; // Track if freakout already happened
    private float currentDirection = 1f; // 1 for right, -1 for left
    private float lastDirectionChangeTime = 0f;
    private bool isPatrolling = true;
    private float shockEndTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentDirection = Random.Range(0, 2) == 0 ? -1f : 1f; // Start with random direction
        UpdateVisualDirection();

        // Fix: Lock rotation to prevent falling over
        rb.freezeRotation = true;

        // Optional: Also constrain rotation if needed
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Ignore collisions with player
        SetupCollisionIgnore();
    }

    void SetupCollisionIgnore()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
            Collider2D enemyCollider = GetComponent<Collider2D>();

            if (playerCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        CheckGrounded();

        // Handle shock state first
        if (isInShock)
        {
            if (Time.time >= shockEndTime)
            {
                isInShock = false;
                StartRunningAway();
            }
            return; // Don't process other states while in shock
        }

        // Normal patrol behavior when player is far
        if (distanceToPlayer > safeDistance)
        {
            isRunningAway = false;
            isPatrolling = true;
            hasFreakedOut = false; // Reset freakout when player is far enough
            Patrol();
        }
        // Player detected - go into shock first, then run away
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            if (!isRunningAway && !isInShock && !hasFreakedOut)
            {
                EnterShock();
            }
            else if (isRunningAway)
            {
                // Continue running away, update direction if needed
                currentDirection = GetRunAwayDirection();
                UpdateVisualDirection();
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
            currentDirection = GetRunAwayDirection();
            UpdateVisualDirection();
        }

        // Handle edge and wall detection for both patrol and running
        if (isGrounded && (isPatrolling || isRunningAway))
        {
            HandleObstacleDetection();
        }
    }

    void FixedUpdate()
    {
        // Handle physics-based movement in FixedUpdate
        if (isPatrolling && !isInShock)
        {
            rb.velocity = new Vector2(currentDirection * walkSpeed, rb.velocity.y);
        }
        else if (isRunningAway && !isInShock)
        {
            rb.velocity = new Vector2(currentDirection * runSpeed, rb.velocity.y);
        }
        else if (isInShock || (!isPatrolling && !isRunningAway))
        {
            // Shock state or freak out - stop moving
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        // Fix: Ensure rotation stays at zero (additional safety)
        rb.rotation = 0f;
        transform.rotation = Quaternion.identity;
    }

    void EnterShock()
    {
        isInShock = true;
        isPatrolling = false;
        isRunningAway = false;
        shockEndTime = Time.time + shockDuration;

        Debug.Log($"{gameObject.name} is shocked! Player detected!");
    }

    void StartRunningAway()
    {
        isRunningAway = true;
        isPatrolling = false;

        // Calculate run direction away from player
        currentDirection = GetRunAwayDirection();
        UpdateVisualDirection();

        Debug.Log($"{gameObject.name} is running away in panic!");
    }

    void Patrol()
    {
        // Change direction periodically or when hitting obstacles
        if (Time.time >= lastDirectionChangeTime + patrolChangeTime)
        {
            ChangeDirection();
            lastDirectionChangeTime = Time.time;
        }
    }

    void FreakOut()
    {
        // Stop moving and mark as freaked out (only happens once)
        isRunningAway = false;
        isPatrolling = false;
        hasFreakedOut = true;

        rb.velocity = new Vector2(0f, rb.velocity.y);
        Debug.Log($"{gameObject.name} is freaking out! Player too close!");

        // Optional: Add some shaking or visual effect here
        // Then after a brief moment, start running away
        Invoke(nameof(StartRunningAway), 0.8f); // Start running after 0.8 seconds of freaking out
    }

    void HandleObstacleDetection()
    {
        // Check for walls or edges and change direction if needed
        if (IsPathBlocked(currentDirection))
        {
            ChangeDirection();

            // If running away and hit obstacle, continue running in new direction (still faster)
            if (isRunningAway)
            {
                Debug.Log($"{gameObject.name} panicked and turned around!");
            }
        }
    }

    void ChangeDirection()
    {
        currentDirection = -currentDirection;
        UpdateVisualDirection();
        lastDirectionChangeTime = Time.time;
    }

    float GetRunAwayDirection()
    {
        // Returns -1 if player is on right (run left), 1 if player is on left (run right)
        return player.position.x > transform.position.x ? -1f : 1f;
    }

    void UpdateVisualDirection()
    {
        // Only flip on X axis, preserve Y and Z scale
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Sign(currentDirection) * Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
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

        // Check for pits - FIXED: Start the edge check from lower position
        Vector2 edgeCheckPosition = checkPosition + new Vector2(direction * edgeCheckDistance, -0.2f);
        bool groundAhead = Physics2D.Raycast(edgeCheckPosition,
            Vector2.down, groundCheckDistance, groundLayer);

        return wallAhead || !groundAhead;
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

        // Draw edge check - FIXED: Show the corrected position
        Gizmos.color = Color.cyan;
        Vector2 edgeCheckPos = (Vector2)transform.position + new Vector2(currentDirection * edgeCheckDistance, -0.2f);
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
            $"State: {(isInShock ? "SHOCK" : (isRunningAway ? "RUNNING" : (isPatrolling ? "PATROL" : "FREAKOUT")))}\nFreakedOut: {hasFreakedOut}", style);
#endif
    }
}