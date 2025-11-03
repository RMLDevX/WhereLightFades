using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Component Switching")]
    public string componentToRemoveName; // Type name like "PlayerController"
    public string componentToAddName;    // Type name like "Scene2PlayerController"

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
            player.AddComponent(System.Type.GetType(componentToAddName));
            Debug.Log("Added: " + componentToAddName);
        }
    }
}