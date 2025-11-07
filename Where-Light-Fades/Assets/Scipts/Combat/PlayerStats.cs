using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; // Singleton pattern

    [Header("UI")]
    public GameObject deathPanel;

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
    public bool enableLifeDrain = false;

    [Header("World Effects")]
    public bool isInParallelWorld = false;
    public float parallelWorldHealthDrainMultiplier = 3f;
    public float parallelWorldManaDrainMultiplier = 2f;

    private float previousMana;

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

    void Start()
    {
        previousMana = currentMana;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("New scene loaded: " + scene.name);

        ResetAllAnimations();
        ResetPlayerState();
    }

    void ResetAllAnimations()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("Resetting animations for new scene");
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isSlashing", false);
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
        // Check if mana was restored
        CheckManaRestored();

        // Only drain life if system is activated
        if (enableLifeDrain && currentHealth > 0)
        {
            float drainRate = healthDrainRate * Time.deltaTime;

            // Apply multiplier if in parallel world
            if (isInParallelWorld)
            {
                drainRate *= parallelWorldHealthDrainMultiplier;
            }

            currentHealth -= drainRate;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        // Only drain mana if in parallel world AND system is activated
        if (isInParallelWorld && enableLifeDrain && currentMana > 0)
        {
            float manaDrain = parallelWorldManaDrainMultiplier * Time.deltaTime;
            currentMana = Mathf.Max(0, currentMana - manaDrain);

            // Auto-switch to normal world when mana reaches 0
            if (currentMana <= 0)
            {
                currentMana = 0;
                SwitchToNormalWorld();
            }
        }

        // Update previous mana for next frame
        previousMana = currentMana;
    }

    void CheckManaRestored()
    {
        // If mana increased from 0 to positive, re-enable parallel world switching
        if (previousMana <= 0 && currentMana > 0)
        {
            EnableParallelWorldSwitching();
        }
    }

    void SwitchToNormalWorld()
    {
        if (isInParallelWorld)
        {
            Debug.Log("Mana depleted! Auto-switching to normal world");

            // Find ParallelWorldManager and force switch to normal world
            ParallelWorldManager worldManager = FindObjectOfType<ParallelWorldManager>();
            if (worldManager != null && worldManager.isParallelWorldActive)
            {
                worldManager.ForceSwitchToNormalWorld();
            }
        }
    }

    void EnableParallelWorldSwitching()
    {
        ParallelWorldManager worldManager = FindObjectOfType<ParallelWorldManager>();
        if (worldManager != null)
        {
            worldManager.EnableParallelWorldSwitching();
        }
    }

    public void SetParallelWorldState(bool inParallelWorld)
    {
        isInParallelWorld = inParallelWorld;
        Debug.Log("Parallel world state: " + inParallelWorld);
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
        float oldMana = currentMana;
        currentMana = Mathf.Min(currentMana + amount, maxMana);

        // Manually check for mana restoration when using this method
        if (oldMana <= 0 && currentMana > 0)
        {
            EnableParallelWorldSwitching();
        }
    }

    void Die()
    {
        if (currentHealth > 0) return; // Prevent multiple calls

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            Time.timeScale = 0f; // optional freeze
        }

        Debug.Log("Die method called - Current Health: " + currentHealth);

        // Disable all player controls
        TutorialPlayerMovement movement = GetComponent<TutorialPlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
            Debug.Log("Movement disabled");
        }

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.enabled = false;
            Debug.Log("Combat disabled");
        }

        PlayerJump jump = GetComponent<PlayerJump>();
        if (jump != null)
        {
            jump.enabled = false;
            Debug.Log("Jump disabled");
        }

        // Freeze the player in place
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // This stops physics from affecting the player
            rb.simulated = false; // This completely stops rigidbody simulation
        }

        // Keep collider enabled so player doesn't fall through ground
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = true;

        // Trigger the death animation
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("Animator found, triggering Die animation");
            animator.SetTrigger("Die");

            // Clear singleton and destroy
            if (Instance == this)
            {
                Instance = null;
            }

            Destroy(gameObject, 2f);
            Debug.Log("Destroy scheduled in 2 seconds");
        }
        else
        {
            Debug.LogError("No Animator found!");
            Destroy(gameObject);
        }
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