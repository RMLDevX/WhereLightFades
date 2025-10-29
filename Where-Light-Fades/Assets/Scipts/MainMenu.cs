using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {

        StartCoroutine(FadeAndLoadScene(1));
        SceneManager.LoadSceneAsync(1);

    }

    public void Credits()
    {

        StartCoroutine(FadeAndLoadScene(2));

        SceneManager.LoadSceneAsync(2);

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

        UnityEngine.UI.Image fadeImage = fadeObject.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}


}
