using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Magic Settings")]
    public GameObject magicProjectilePrefab;
    public Transform magicSpawnPoint;
    public float projectileSpeed = 10f;

    void Update()
    {
        // E key for slash attack
        if (Input.GetKeyDown(KeyCode.E))
        {
            SlashAttack();
        }

        // R key for magic attack
        if (Input.GetKeyDown(KeyCode.R))
        {
            MagicAttack();
        }
    }

    void SlashAttack()
    {
        // Check if we have enough mana
        if (!PlayerStats.Instance.UseMana(PlayerStats.Instance.slashManaCost))
            return;

        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(PlayerStats.Instance.slashDamage);
            }
        }

        Debug.Log("Slash Attack!");
    }

    void MagicAttack()
    {
        // Check if we have enough mana
        if (!PlayerStats.Instance.UseMana(PlayerStats.Instance.magicManaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }

        // Spawn projectile
        GameObject projectile = Instantiate(magicProjectilePrefab, magicSpawnPoint.position, Quaternion.identity);

        // Get direction player is facing
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(direction * projectileSpeed, 0f);

        Debug.Log("Magic Attack!");
    }

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}