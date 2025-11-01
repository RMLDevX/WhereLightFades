using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;

    public void PlayGame()
    {
        StartCoroutine(FadeAndLoadScene(1));
    }

    public void Credits()
    {
        StartCoroutine(FadeAndLoadScene(3));
    }

    public void Exit()
    {
        Application.Quit();
    }

    IEnumerator FadeAndLoadScene(int sceneIndex)
    {
        
        GameObject fadeObject = new GameObject("FadeOverlay");
        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        Image fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        
        asyncLoad.allowSceneActivation = true;
    }
}