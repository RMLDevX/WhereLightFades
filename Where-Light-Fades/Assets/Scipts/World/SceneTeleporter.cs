using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    public string targetSceneName;
    public string spawnPointName; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Save spawn point name before loading scene
            PlayerPrefs.SetString("SpawnPoint", spawnPointName);
            DontDestroyOnLoad(other.gameObject);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}