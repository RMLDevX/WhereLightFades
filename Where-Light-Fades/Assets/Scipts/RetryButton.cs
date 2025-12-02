using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
    public string sceneToLoad;

    public void Retry()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
