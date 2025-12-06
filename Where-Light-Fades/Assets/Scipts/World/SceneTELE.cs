using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporterHideCanvas : MonoBehaviour
{
    public string targetSceneName;
    public string spawnPointName;
    public GameObject canvasToHide; // assign the canvas you want hidden

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide canvas BEFORE leaving the scene
            if (canvasToHide != null)
                canvasToHide.SetActive(false);

            // Save spawn point name before loading scene
            PlayerPrefs.SetString("SpawnPoint", spawnPointName);

            DontDestroyOnLoad(other.gameObject);

            SceneManager.LoadScene(targetSceneName);
        }
    }
}
