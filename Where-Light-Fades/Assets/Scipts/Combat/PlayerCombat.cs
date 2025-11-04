using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    void Update()
    {
        // E key for slash attack
        if (Input.GetKeyDown(KeyCode.E))
        {
            SlashAttack();
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

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}