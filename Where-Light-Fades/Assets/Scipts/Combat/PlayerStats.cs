using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("UI")]
    [Tooltip("If null, will search for GameObject with 'DeathPanel' tag")]
    public GameObject deathPanel;
    public Button restartButton;
    [Tooltip("Delay before showing death panel (for death animation)")]
    public float deathPanelDelay = 1.5f;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float maxMana = 50f;

    [Header("Combat Settings")]
    public float slashDamage = 20f;
    public float slashManaCost = 0f;

    [Header("Life Drain Settings")]
    public float healthDrainRate = 1f;

    [Header("World Effects")]
    public float parallelWorldHealthDrainMultiplier = 3f;
    public float parallelWorldManaDrainMultiplier = 2f;

    [Header("Restart Settings")]
    public int initialSceneIndex = 0;
    public Vector3 initialPosition = Vector3.zero;

    // Runtime variables
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentMana;
    [HideInInspector] public bool enableLifeDrain = false;
    [HideInInspector] public bool isInParallelWorld = false;

    private float previousMana;
    private bool isDead = false;
    private static bool isCompleteRestart = false;

    void Awake()
    {
        // Handle singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize stats
        InitializeStats();

        Debug.Log($"PlayerStats created. Scene: {initialSceneIndex}, Position: {initialPosition}");
    }

    void InitializeStats()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        enableLifeDrain = false;
        isInParallelWorld = false;
        isDead = false;
        previousMana = maxMana;
    }

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        if (deathPanel == null)
        {
            FindDeathPanel();
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        SetupRestartButton();
    }

    void FindDeathPanel()
    {
        if (deathPanel != null && !deathPanel.activeInHierarchy)
        {
            return;
        }

        // Search in Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                if (child.CompareTag("DeathPanel"))
                {
                    deathPanel = child.gameObject;
                    Debug.Log($"Found DeathPanel: {deathPanel.name}");
                    return;
                }
            }
        }

        // Search by tag
        GameObject deathPanelObj = GameObject.FindGameObjectWithTag("DeathPanel");
        if (deathPanelObj != null)
        {
            deathPanel = deathPanelObj;
            Debug.Log($"Found DeathPanel by tag: {deathPanel.name}");
        }
    }

    void SetupRestartButton()
    {
        if (deathPanel != null)
        {
            restartButton = deathPanel.GetComponentInChildren<Button>(true);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(FullGameRestart);
                Debug.Log($"Restart button setup: {restartButton.name}");
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name} (Index: {scene.buildIndex})");

        // Re-initialize UI
        InitializeUI();

        // If this is a complete restart, position player at initial position
        if (isCompleteRestart && scene.buildIndex == initialSceneIndex)
        {
            StartCoroutine(SetupAfterRestart());
        }
    }

    IEnumerator SetupAfterRestart()
    {
        // Wait for all scene objects to initialize
        yield return null;
        yield return null;

        // Reset everything
        ResetAllAnimations();
        ResetPlayerComponents();
        TeleportPlayerToPosition(initialPosition);

        // Force normal world
        ParallelWorldManager worldManager = FindObjectOfType<ParallelWorldManager>();
        if (worldManager != null && worldManager.isParallelWorldActive)
        {
            worldManager.ForceSwitchToNormalWorld();
        }

        // Clear the restart flag
        isCompleteRestart = false;

        Debug.Log("=== RESTART SETUP COMPLETE ===");
    }

    void TeleportPlayerToPosition(Vector3 position)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = false;
            rb.simulated = true;
        }

        transform.position = position;
        Debug.Log($"Player teleported to: {position}");
    }

    void ResetAllAnimations()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isSlashing", false);
            animator.ResetTrigger("Die");
            animator.Play("Idle", 0, 0f);
        }
    }

    void ResetPlayerComponents()
    {
        isDead = false;

        // Enable movement
        TutorialPlayerMovement movement = GetComponent<TutorialPlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
            movement.SetMovement(true);
        }

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.enabled = true;
        }

        PlayerJump jump = GetComponent<PlayerJump>();
        if (jump != null)
        {
            jump.enabled = true;
        }

        // Reset physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = false;
            rb.simulated = true;
        }

        // Re-enable collider if it was disabled
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    void Update()
    {
        if (isDead) return;

        CheckManaRestored();

        // Health drain
        if (enableLifeDrain && currentHealth > 0)
        {
            float drainRate = healthDrainRate * Time.deltaTime;

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

        // Mana drain
        if (isInParallelWorld && enableLifeDrain && currentMana > 0)
        {
            float manaDrain = parallelWorldManaDrainMultiplier * Time.deltaTime;
            currentMana = Mathf.Max(0, currentMana - manaDrain);

            if (currentMana <= 0)
            {
                currentMana = 0;
                SwitchToNormalWorld();
            }
        }

        previousMana = currentMana;
    }

    void CheckManaRestored()
    {
        if (previousMana <= 0 && currentMana > 0)
        {
            EnableParallelWorldSwitching();
        }
    }

    void SwitchToNormalWorld()
    {
        if (isInParallelWorld)
        {
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
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}");

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

        if (oldMana <= 0 && currentMana > 0)
        {
            EnableParallelWorldSwitching();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("=== PLAYER DIED ===");

        // Disable controls
        TutorialPlayerMovement movement = GetComponent<TutorialPlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        PlayerJump jump = GetComponent<PlayerJump>();
        if (jump != null) jump.enabled = false;

        // Freeze physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
        }

        // Death animation
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        StartCoroutine(ShowDeathPanelCoroutine());
    }

    IEnumerator ShowDeathPanelCoroutine()
    {
        yield return new WaitForSeconds(deathPanelDelay);

        if (deathPanel == null)
        {
            FindDeathPanel();
        }

        if (deathPanel != null)
        {
            Time.timeScale = 0f;
            deathPanel.SetActive(true);
            Debug.Log("DeathPanel shown");

            if (restartButton == null)
            {
                SetupRestartButton();
            }

            if (restartButton != null)
            {
                restartButton.interactable = true;
            }
        }
        else
        {
            Debug.LogError("DeathPanel not found! Restarting immediately.");
            FullGameRestart();
        }
    }

    /// <summary>
    /// Complete game restart - Destroys all persistent objects and reloads from scratch
    /// </summary>
    public void FullGameRestart()
    {
        Debug.Log("=== FULL GAME RESTART INITIATED ===");
        StartCoroutine(RestartEntireGame());
    }

    IEnumerator RestartEntireGame()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Hide death panel
        if (deathPanel != null && deathPanel.activeSelf)
        {
            deathPanel.SetActive(false);
        }

        // Set restart flag
        isCompleteRestart = true;

        // Reset stats
        InitializeStats();

        Debug.Log("Destroying all DontDestroyOnLoad objects...");

        // Find and destroy ALL DontDestroyOnLoad objects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Check if object is in DontDestroyOnLoad scene
            if (obj.scene.name == "DontDestroyOnLoad" || obj.scene.buildIndex == -1)
            {
                Debug.Log($"Destroying persistent object: {obj.name}");
                Destroy(obj);
            }
        }

        // Clear the singleton reference
        Instance = null;

        // Force unload unused assets
        yield return Resources.UnloadUnusedAssets();

        // Force garbage collection
        System.GC.Collect();

        Debug.Log($"Loading initial scene: {initialSceneIndex}");

        // Load the initial scene with Single mode to completely reload
        SceneManager.LoadScene(initialSceneIndex, LoadSceneMode.Single);

        Debug.Log("=== GAME RESTART COMPLETE - Fresh Start ===");
    }

    public void SetInitialRestartSettings(int sceneIndex, Vector3 position)
    {
        initialSceneIndex = sceneIndex;
        initialPosition = position;
        Debug.Log($"Initial settings: Scene {sceneIndex}, Position {position}");
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}