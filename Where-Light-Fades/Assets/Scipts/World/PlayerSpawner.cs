using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Component Switching")]
    public string componentToRemoveName = "TutorialPlayerMovement";
    public string componentToAddName = "PlayerMovement";

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Get spawn point name from teleporter
            string spawnPointName = PlayerPrefs.GetString("SpawnPoint", "");

            if (!string.IsNullOrEmpty(spawnPointName))
            {
                // Find the spawn point GameObject
                GameObject spawnPoint = GameObject.Find(spawnPointName);
                if (spawnPoint != null)
                {
                    // Move player to spawn point
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log("Player spawned at: " + spawnPointName);
                }
            }

            // Switch components
            SwitchPlayerComponents(player);

            // Auto-activate UI in new scene
            ActivateUIInNewScene();

            // Clear the saved spawn point
            PlayerPrefs.DeleteKey("SpawnPoint");
        }
    }

    private void SwitchPlayerComponents(GameObject player)
    {
        // Remove old component by name
        if (!string.IsNullOrEmpty(componentToRemoveName))
        {
            Component oldComponent = player.GetComponent(componentToRemoveName);
            if (oldComponent != null)
            {
                Destroy(oldComponent);
                Debug.Log("Removed: " + componentToRemoveName);
            }
        }

        // Add new component by name
        if (!string.IsNullOrEmpty(componentToAddName))
        {
            System.Type componentType = System.Type.GetType(componentToAddName);
            if (componentType != null)
            {
                player.AddComponent(componentType);
                Debug.Log("Added: " + componentToAddName);
            }
            else
            {
                Debug.LogError("Component type not found: " + componentToAddName);
            }
        }
    }

    private void ActivateUIInNewScene()
    {
        // Find UIManager in the new scene and force activate UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            // Use reflection to call the activation method
            System.Reflection.MethodInfo method = uiManager.GetType().GetMethod("ActivateStatsSystem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(uiManager, null);
                Debug.Log("UI automatically activated in new scene");
            }

            // ALSO update the player reference in UI manager if needed
            UpdateUIPlayerReferences(uiManager);
        }
    }

    private void UpdateUIPlayerReferences(UIManager uiManager)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Try to set player reference through different methods
        // Method 1: Using SerializedField (if applicable)
        var uiManagerType = uiManager.GetType();
        var playerField = uiManagerType.GetField("player",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        if (playerField != null)
        {
            playerField.SetValue(uiManager, player);
            Debug.Log("Updated player reference in UIManager");
        }

        // Method 2: Call a public setup method if it exists
        var setupMethod = uiManagerType.GetMethod("SetupPlayerReferences");
        if (setupMethod != null)
        {
            setupMethod.Invoke(uiManager, new object[] { player });
            Debug.Log("Called SetupPlayerReferences method");
        }
    }
}