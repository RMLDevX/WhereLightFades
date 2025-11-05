using UnityEngine;

public class TutorialPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float acceleration = 3f;
    public float deceleration = 30f;

    [Header("Jump Movement Settings")]
    public float airMoveSpeedMultiplier = 0.7f;
    public float airAccelerationMultiplier = 0.6f;

    [Header("Camera Boundaries")]
    public float leftBoundaryOffset = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerJump playerJump;
    private Camera mainCamera;
    private Animator animator;
    private float horizontalInput;
    private Vector2 targetVelocity;
    private float leftBoundary;

    // Public property to check if player is moving/running
    public bool IsMoving { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        SetupRigidbody();
        CalculateBoundaries();
    }

    void SetupRigidbody()
    {
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void CalculateBoundaries()
    {
        if (mainCamera != null)
        {
            float cameraLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            leftBoundary = cameraLeft + leftBoundaryOffset;
        }
    }

    void Update()
    {
        GetInput();
        HandleSpriteFlip();
        HandleAnimations();
        CalculateBoundaries();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        IsMoving = Mathf.Abs(horizontalInput) > 0.1f;

        if (transform.position.x <= leftBoundary && horizontalInput < 0)
        {
            horizontalInput = 0;
            IsMoving = false;
        }
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
    }

    void FixedUpdate()
    {
        HandleMovement();
        EnforceBoundaries();
    }

    void HandleMovement()
    {
        float currentMoveSpeed = moveSpeed;
        float currentAcceleration = acceleration;

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

    void EnforceBoundaries()
    {
        if (transform.position.x < leftBoundary)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = leftBoundary;
            transform.position = newPosition;

            if (rb.velocity.x < 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
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

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(leftBoundary, transform.position.y - 10, 0),
                           new Vector3(leftBoundary, transform.position.y + 10, 0));
        }
    }
}