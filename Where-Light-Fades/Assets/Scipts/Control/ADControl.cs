using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float acceleration = 30f;
    public float deceleration = 30f;

    [Header("Jump Movement Settings")]
    public float airMoveSpeedMultiplier = 0.7f; 
    public float airAccelerationMultiplier = 0.6f; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerJump playerJump;
    private Animator animator;
    private float horizontalInput;
    private Vector2 targetVelocity;

    // Public property to check if player is moving/running
    public bool IsMoving { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();
        animator = GetComponent<Animator>();
        SetupRigidbody();
    }

    void SetupRigidbody()
    {
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        GetInput();
        HandleSpriteFlip();
        HandleAnimations(); // Added from Code1
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        IsMoving = Mathf.Abs(horizontalInput) > 0.1f; // Added from Code1
    }

    void HandleSpriteFlip()
    {
        if (horizontalInput > 0.1f)
        {
            spriteRenderer.flipX = false;
            FlipAttackPoints(false);
        }
        else if (horizontalInput < -0.1f)
        {
            spriteRenderer.flipX = true;
            FlipAttackPoints(true);
        }
    }

    // Added from Code1 - Animation Handling
    void HandleAnimations()
    {
        if (animator == null) return;
        if (playerJump == null) return;

        // Check if currently slashing
        bool isSlashing = animator.GetBool("isSlashing");

        // Only show running when moving AND grounded AND not jumping
        bool isGrounded = playerJump.IsGrounded();
        bool isJumping = !isGrounded;

        bool isRunning = IsMoving && isGrounded && !isJumping;

        animator.SetBool("isRunning", isRunning && !isSlashing);
    }

    void FlipAttackPoints(bool facingLeft)
    {
        Transform attackPoint = transform.Find("AttackPoint");
        if (attackPoint != null)
        {
            Vector3 pos = attackPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * (facingLeft ? -1 : 1);
            attackPoint.localPosition = pos;
        }

        Transform magicSpawnPoint = transform.Find("MagicSpawnPoint");
        if (magicSpawnPoint != null)
        {
            Vector3 pos = magicSpawnPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * (facingLeft ? -1 : 1);
            magicSpawnPoint.localPosition = pos;
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float currentMoveSpeed = moveSpeed;
        float currentAcceleration = acceleration;

        // Apply air movement modifiers when not grounded
        if (playerJump != null && !playerJump.IsGrounded())
        {
            currentMoveSpeed *= airMoveSpeedMultiplier;
            currentAcceleration *= airAccelerationMultiplier;
        }

        targetVelocity.x = horizontalInput * currentMoveSpeed;
        targetVelocity.y = rb.velocity.y;

        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);

        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            rb.velocity = new Vector2(
                Mathf.Lerp(rb.velocity.x, 0, deceleration * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
    }

    public void SetMovement(bool state)
    {
        enabled = state;
        if (!state) rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public bool IsFacingRight()
    {
        return !spriteRenderer.flipX;
    }
}