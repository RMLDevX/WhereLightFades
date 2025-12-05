using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("UI")]
    public GameObject deathPanel;
    public Button restartButton;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxMana = 50f;
    public float currentMana = 50f;

    [Header("Combat Settings")]
    public float slashDamage = 20f;
    public float slashManaCost = 0f;

    [Header("Life Drain Settings")]
    public float healthDrainRate = 1f;
    public bool enableLifeDrain = false;

    [Header("World Effects")]
    public bool isInParallelWorld = false;
    public float parallelWorldHealthDrainMultiplier = 3f;
    public float parallelWorldManaDrainMultiplier = 2f;

    [Header("Restart Settings")]
    public int initialSceneIndex = 0;
    public Vector3 initialPosition = Vector3.zero;
    public bool useInitialPositionOnRestart = true;

    private float previousMana;
    private bool isDead = false;
    private Vector3 respawnPosition;
    private int respawnSceneIndex;

    // Track if we're currently teleporting to a new scene
    private bool isTeleporting = false;
    private int targetSceneForTeleport = -1;
    private Vector3 targetPositionForTeleport = Vector3.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Set initial values
            respawnSceneIndex = initialSceneIndex;
            respawnPosition = initialPosition;

            Debug.Log("PlayerStats initialized. Initial scene: " + initialSceneIndex + ", position: " + initialPosition);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        previousMana = currentMana;

        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        else if (deathPanel != null)
        {
            restartButton = deathPanel.GetComponentInChildren<Button>();
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name + " (Index: " + scene.buildIndex + ")");

        // If we're teleporting to a specific scene/position
        if (isTeleporting && scene.buildIndex == targetSceneForTeleport)
        {
            Debug.Log("Teleporting player to position: " + targetPositionForTeleport);

            // Teleport the player to the target position
            TeleportPlayerToPosition(targetPositionForTeleport);

            // Reset teleport flags
            isTeleporting = false;
            targetSceneForTeleport = -1;
            targetPositionForTeleport = Vector3.zero;
        }
        else
        {
            // Normal scene load - find spawn point
            FindAndSetSpawnPoint(scene);
        }

        ResetAllAnimations();
        ResetPlayerState();

        // Make sure UI is hidden
        if (deathPanel != null && deathPanel.activeSelf)
        {
            deathPanel.SetActive(false);
        }

        // Make sure the player GameObject is active and in the correct scene
        EnsurePlayerInCorrectScene();
    }

    void FindAndSetSpawnPoint(Scene scene)
    {
        // Try to find spawn point in the scene
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            TeleportPlayerToPosition(spawnPoint.transform.position);
            respawnPosition = spawnPoint.transform.position;
            respawnSceneIndex = scene.buildIndex;
            Debug.Log("Found spawn point at: " + spawnPoint.transform.position);
        }
        else
        {
            // Try to find player start position
            GameObject playerStart = GameObject.Find("PlayerStart");
            if (playerStart != null)
            {
                TeleportPlayerToPosition(playerStart.transform.position);
                respawnPosition = playerStart.transform.position;
                respawnSceneIndex = scene.buildIndex;
                Debug.Log("Found PlayerStart at: " + playerStart.transform.position);
            }
            else
            {
                // If no spawn found, use default position (0, 0, 0)
                TeleportPlayerToPosition(Vector3.zero);
                respawnPosition = Vector3.zero;
                respawnSceneIndex = scene.buildIndex;
                Debug.LogWarning("No spawn point found. Using (0, 0, 0)");
            }
        }
    }

    void TeleportPlayerToPosition(Vector3 position)
    {
        Debug.Log("Teleporting player to: " + position);

        // Reset any physics state
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = false;
            rb.simulated = true;
        }

        // Set the position
        transform.position = position;

        // Force immediate position update
        transform.hasChanged = true;
    }

    void EnsurePlayerInCorrectScene()
    {
        // Get the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // If player is not in the active scene, move it there
        if (gameObject.scene != currentScene)
        {
            Debug.Log("Moving player from scene '" + gameObject.scene.name +
                     "' to active scene '" + currentScene.name + "'");

            // Move the player GameObject to the active scene
            SceneManager.MoveGameObjectToScene(gameObject, currentScene);
        }
    }

    void ResetAllAnimations()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isSlashing", false);
            animator.ResetTrigger("Die");
            animator.Rebind();
            animator.Update(0f);
        }
    }

    void ResetPlayerState()
    {
        isDead = false;

        // Re-enable all player components
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

        // Reset Rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
            rb.simulated = true;
        }

        // Reset stats
        currentHealth = maxHealth;
        currentMana = maxMana;
        enableLifeDrain = false;
        isInParallelWorld = false;
    }

    void Update()
    {
        if (isDead) return;

        CheckManaRestored();

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
            Debug.Log("Mana depleted! Auto-switching to normal world");
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
        if (isDead) return;

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

        if (oldMana <= 0 && currentMana > 0)
        {
            EnableParallelWorldSwitching();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died");

        // Disable all player controls
        TutorialPlayerMovement movement = GetComponent<TutorialPlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        PlayerJump jump = GetComponent<PlayerJump>();
        if (jump != null) jump.enabled = false;

        // Freeze the player
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
        }

        // Trigger death animation
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Show death panel after animation
        Invoke("ShowDeathPanel", 1f);
    }

    void ShowDeathPanel()
    {
        Time.timeScale = 0f;
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);

            // Ensure restart button is properly set up
            if (restartButton == null)
            {
                restartButton = deathPanel.GetComponentInChildren<Button>();
                if (restartButton != null)
                {
                    restartButton.onClick.RemoveAllListeners();
                    restartButton.onClick.AddListener(RestartGame);
                }
            }
        }
        else
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Debug.Log("=== RESTART GAME ===");

        // Reset time scale
        Time.timeScale = 1f;

        // Hide death panel
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // Determine which scene and position to use
        int targetScene = useInitialPositionOnRestart ? initialSceneIndex : respawnSceneIndex;
        Vector3 targetPosition = useInitialPositionOnRestart ? initialPosition : respawnPosition;

        Debug.Log("Target Scene: " + targetScene + ", Target Position: " + targetPosition);

        // Set teleport flags
        isTeleporting = true;
        targetSceneForTeleport = targetScene;
        targetPositionForTeleport = targetPosition;

        // Load the target scene - this will trigger OnSceneLoaded
        SceneManager.LoadScene(targetScene);
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
        respawnSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"Checkpoint set at {position} in scene {respawnSceneIndex}");
    }

    public void SetInitialRestartSettings(int sceneIndex, Vector3 position)
    {
        initialSceneIndex = sceneIndex;
        initialPosition = position;
        Debug.Log($"Initial restart settings: Scene {sceneIndex}, Position {position}");
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}