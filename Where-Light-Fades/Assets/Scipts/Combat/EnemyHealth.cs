using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    public GameObject remnantPrefab; // Assign the remnant prefab here
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Drop remnant
        if (remnantPrefab != null)
        {
            Instantiate(remnantPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}