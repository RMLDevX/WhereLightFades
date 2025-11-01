using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 30f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private Vector2 targetVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        SetupRigidbody();
    }

    void SetupRigidbody()
    {
        // REMOVE THIS LINE: rb.gravityScale = 0f;
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
            spriteRenderer.flipX = false;
        else if (horizontalInput < -0.1f)
            spriteRenderer.flipX = true;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        targetVelocity.x = horizontalInput * moveSpeed;
        targetVelocity.y = rb.velocity.y; // Preserve vertical velocity from jumping

        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            rb.velocity = new Vector2(
                Mathf.Lerp(rb.velocity.x, 0, deceleration * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
    }
}