using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 30f;

    [Header("Jump Movement Settings")]
    public float airMoveSpeedMultiplier = 0.7f; // 70% speed in air
    public float airAccelerationMultiplier = 0.6f; // 60% acceleration in air

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerJump playerJump;
    private float horizontalInput;
    private Vector2 targetVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();
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
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
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