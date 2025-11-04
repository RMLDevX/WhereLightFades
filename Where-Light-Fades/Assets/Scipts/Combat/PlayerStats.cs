using UnityEngine;
using UnityEngine.SceneManagement;

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
        Debug.Log("PlayerStats Awake called - Instance: " + Instance);

        // Make this persistent across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene load event
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("PlayerStats set as persistent instance");
        }
        else
        {
            Debug.Log("Destroying duplicate PlayerStats");
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("New scene loaded: " + scene.name);

        // Reset all animations when new scene loads
        ResetAllAnimations();

        // Ensure player is in correct state
        ResetPlayerState();
    }

    void ResetAllAnimations()
    {
        // Find the animator on the player (could be on child objects)
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("Resetting animations for new scene");
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isSlashing", false);
            // Reset any other animation parameters here

            // Force animator to update immediately
            animator.Rebind();
            animator.Update(0f);
        }
        else
        {
            Debug.LogWarning("Animator not found when resetting animations");
        }
    }

    void ResetPlayerState()
    {
        // Reset any movement or combat states
        TutorialPlayerMovement movement = GetComponent<TutorialPlayerMovement>();
        PlayerJump jump = GetComponent<PlayerJump>();
        PlayerCombat combat = GetComponent<PlayerCombat>();

        if (movement != null)
        {
            movement.SetMovement(true);
        }

        // Ensure player is grounded and not in combat state
        if (jump != null)
        {
            // Jump state will reset automatically through ground detection
        }

        // Reset any combat cooldowns or states if needed
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

        // When player dies, you might want to reload scene or go to game over
        // SceneManager.LoadScene("GameOverScene");
    }

    void OnDestroy()
    {
        // Unsubscribe from event when destroyed to prevent memory leaks
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("PlayerStats unsubscribed from scene events");
        }
    }
}