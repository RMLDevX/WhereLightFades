using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public float slashCooldown = 0.5f;

    [Header("Damage Timing")]
    public float damageDelay = 0.2f;

    private Animator animator;
    private TutorialPlayerMovement playerMovement;
    private bool canSlash = true;
    private float slashCooldownTimer = 0f;
    private bool isRunningDuringAttack = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<TutorialPlayerMovement>();
    }

    void Update()
    {
        // Update cooldown timer
        if (!canSlash)
        {
            slashCooldownTimer -= Time.deltaTime;
            if (slashCooldownTimer <= 0f)
            {
                canSlash = true;
            }
        }

        // E key for slash attack
        if (Input.GetKeyDown(KeyCode.E) && canSlash)
        {
            SlashAttack();
        }

        // Check if player starts running during attack wind-up
        if (!canSlash && playerMovement != null)
        {
            // Check if player is running (you might need to adjust this condition based on your movement script)
            bool isRunning = animator.GetBool("isRunning");
            bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;

            if (isRunning || isMoving)
            {
                isRunningDuringAttack = true;
            }
        }
    }

    void SlashAttack()
    {
        // Check if we have enough mana
        if (!PlayerStats.Instance.UseMana(PlayerStats.Instance.slashManaCost))
            return;

        // Reset running flag at start of attack
        isRunningDuringAttack = false;

        // Start slashing animation immediately
        if (animator != null)
        {
            animator.SetBool("isSlashing", true);
        }

        // Start cooldown
        canSlash = false;
        slashCooldownTimer = slashCooldown;

        // Apply damage after delay
        Invoke("PerformDamage", damageDelay);

        // Reset animation after total attack duration
        Invoke("ResetSlashAnimation", 0.3f);

        Debug.Log("Slash Attack!");
    }

    void PerformDamage()
    {
        // Cancel damage if player was running during the attack wind-up
        if (isRunningDuringAttack)
        {
            Debug.Log("Damage canceled - player was running during attack!");
            return;
        }

        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(PlayerStats.Instance.slashDamage);
                Debug.Log("Damage applied to enemy!");
            }
        }
    }

    void ResetSlashAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isSlashing", false);
        }

        // Reset running flag when attack completes
        isRunningDuringAttack = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}