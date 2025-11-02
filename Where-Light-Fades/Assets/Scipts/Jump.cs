
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float jumpTime = 0.3f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.2f;
    public float maxGroundAngle = 45f; // Maximum angle considered as ground
    public LayerMask groundLayer;

    [Header("Ceiling Detection")]
    public float ceilingCheckDistance = 0.1f;

    [Header("Physics Settings")]
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 1.5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingCeiling;

    // Jump variables
    private bool isJumping = false;
    private float jumpTimeCounter;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupRigidbody();
    }

    void SetupRigidbody()
    {
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        HandleJump();
        UpdateJumpTimers();
    }

    void HandleJump()
    {
        isGrounded = CheckGrounded();
        isTouchingCeiling = CheckCeiling();

        // Update coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Update jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jump when conditions are met
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            PerformJump();
        }

        // Cancel jump if hitting ceiling
        if (isTouchingCeiling && isJumping)
        {
            CancelJump();
        }

        // Variable jump height (only if not hitting ceiling)
        if (Input.GetKey(KeyCode.Space) && isJumping && !isTouchingCeiling)
        {
            if (jumpTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * 0.8f);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Stop jumping when space is released
        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
        {
            isJumping = false;
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        // Reset jump state when grounded
        if (isGrounded && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
            jumpTimeCounter = jumpTime;
        }
    }

    bool CheckGrounded()
    {
        BoxCollider2D spriteCollider = GetComponentInChildren<BoxCollider2D>();

        if (spriteCollider == null)
        {
            Debug.LogError("No BoxCollider2D found in children!");
            return false;
        }

        Vector2 checkPosition = spriteCollider.bounds.center;
        Vector2 checkSize = new Vector2(spriteCollider.bounds.size.x * 0.8f, groundCheckDistance);
        checkPosition.y = spriteCollider.bounds.min.y - (checkSize.y * 0.5f);

        // Debug visualization
        Debug.DrawLine(checkPosition - new Vector2(checkSize.x * 0.5f, checkSize.y * 0.5f),
                      checkPosition + new Vector2(checkSize.x * 0.5f, checkSize.y * 0.5f),
                      Color.blue, 0.1f);

        Collider2D[] groundHits = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f, groundLayer);

        foreach (Collider2D hit in groundHits)
        {
            if (hit != null && IsTopSurface(hit, spriteCollider.bounds.center.y))
            {
                Debug.Log("Valid top surface found: " + hit.gameObject.name);
                return true;
            }
        }

        return false;
    }

    bool CheckCeiling()
    {
        BoxCollider2D spriteCollider = GetComponentInChildren<BoxCollider2D>();

        if (spriteCollider == null)
        {
            Debug.LogError("No BoxCollider2D found in children!");
            return false;
        }

        Vector2 checkPosition = spriteCollider.bounds.center;
        Vector2 checkSize = new Vector2(spriteCollider.bounds.size.x * 0.8f, ceilingCheckDistance);
        checkPosition.y = spriteCollider.bounds.max.y + (checkSize.y * 0.5f);

        // Debug visualization for ceiling check
        Debug.DrawLine(checkPosition - new Vector2(checkSize.x * 0.5f, checkSize.y * 0.5f),
                      checkPosition + new Vector2(checkSize.x * 0.5f, checkSize.y * 0.5f),
                      Color.red, 0.1f);

        Collider2D[] ceilingHits = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f, groundLayer);

        foreach (Collider2D hit in ceilingHits)
        {
            if (hit != null && IsBottomSurface(hit, spriteCollider.bounds.center.y))
            {
                Debug.Log("Ceiling hit detected: " + hit.gameObject.name);
                return true;
            }
        }

        return false;
    }

    bool IsTopSurface(Collider2D hit, float playerBottomY)
    {
        // Check if the platform is BELOW the player (top surface)
        float platformTop = hit.bounds.max.y;
        float playerBottom = playerBottomY - GetComponentInChildren<BoxCollider2D>().bounds.extents.y;

        // Only consider it ground if platform top is slightly below player bottom
        bool isTopSurface = platformTop <= playerBottom + groundCheckDistance;

        Debug.Log($"Platform Top: {platformTop}, Player Bottom: {playerBottom}, IsTop: {isTopSurface}");

        return isTopSurface;
    }

    bool IsBottomSurface(Collider2D hit, float playerTopY)
    {
        // Check if the platform is ABOVE the player (bottom surface/ceiling)
        float platformBottom = hit.bounds.min.y;
        float playerTop = playerTopY + GetComponentInChildren<BoxCollider2D>().bounds.extents.y;

        // Only consider it ceiling if platform bottom is slightly above player top
        bool isBottomSurface = platformBottom >= playerTop - ceilingCheckDistance;

        Debug.Log($"Platform Bottom: {platformBottom}, Player Top: {playerTop}, IsCeiling: {isBottomSurface}");

        return isBottomSurface;
    }

    void PerformJump()
    {
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        jumpTimeCounter = jumpTime;
    }

    void CancelJump()
    {
        isJumping = false;
        jumpTimeCounter = 0f;

        // Stop upward momentum when hitting ceiling
        if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    void UpdateJumpTimers()
    {
        // Timer updates if needed
    }

    void FixedUpdate()
    {
        ApplyEnhancedGravity();
        LimitVerticalVelocity();
    }

    void ApplyEnhancedGravity()
    {
        if (rb.velocity.y < 0f && !isGrounded)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    void LimitVerticalVelocity()
    {
        if (rb.velocity.y < -25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, -25f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Your existing gizmo code for ground check

        // Add ceiling check visualization
        BoxCollider2D spriteCollider = GetComponentInChildren<BoxCollider2D>();
        if (spriteCollider != null)
        {
            // Ceiling check gizmo
            Vector2 checkPosition = spriteCollider.bounds.center;
            Vector2 checkSize = new Vector2(spriteCollider.bounds.size.x * 0.8f, ceilingCheckDistance);
            checkPosition.y = spriteCollider.bounds.max.y + (checkSize.y * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checkPosition, checkSize);
        }
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsTouchingCeiling()
    {
        return isTouchingCeiling;
    }
}
