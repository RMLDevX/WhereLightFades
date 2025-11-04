using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; // Singleton pattern

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxMana = 50f;
    public float currentMana = 50f;

    [Header("Combat Settings")]
    public float slashDamage = 20f;
    public float slashManaCost = 0f;

    [Header("Life Drain Settings")]
    public float healthDrainRate = 1f; // HP lost per second
    public bool enableLifeDrain = true;

    void Awake()
    {
        // Make this persistent across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Continuous health drain
        if (enableLifeDrain && currentHealth > 0)
        {
            currentHealth -= healthDrainRate * Time.deltaTime;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Add death logic here (restart level, game over screen, etc.)
    }
}