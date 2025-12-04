using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public GameObject questImage;

    public void OpenQuest()
    {
        questImage.SetActive(true);
        Time.timeScale = 0f; // freeze if you want
    }

    public void CloseQuest()
    {
        questImage.SetActive(false);
        Time.timeScale = 1f; // unfreeze
    }
}
