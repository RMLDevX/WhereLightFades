using UnityEngine;

public class TutorialPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 30f;

    [Header("Jump Movement Settings")]
    public float airMoveSpeedMultiplier = 0.7f; // 70% speed in air
    public float airAccelerationMultiplier = 0.6f; // 60% acceleration in air

    [Header("Camera Boundaries")]
    public float leftBoundaryOffset = 0.5f; // How far from left edge player can go

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerJump playerJump;
    private Camera mainCamera;
    private float horizontalInput;
    private Vector2 targetVelocity;
    private float leftBoundary;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();
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
            // Calculate left boundary based on camera view
            float cameraLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            leftBoundary = cameraLeft + leftBoundaryOffset;
        }
    }

    void Update()
    {
        GetInput();
        HandleSpriteFlip();
        CalculateBoundaries(); // Update boundaries in case camera moves
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // Prevent moving left if at boundary
        if (transform.position.x <= leftBoundary && horizontalInput < 0)
        {
            horizontalInput = 0;
        }
    }

    void HandleSpriteFlip()
    {
        if (horizontalInput > 0.1f)
            spriteRenderer.flipX = false;
        else if (horizontalInput < -0.1f)
            spriteRenderer.flipX = true;
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

    void EnforceBoundaries()
    {
        // Prevent player from going beyond left boundary
        if (transform.position.x < leftBoundary)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = leftBoundary;
            transform.position = newPosition;

            // Stop leftward velocity
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

    // Visualize boundary in Scene view
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