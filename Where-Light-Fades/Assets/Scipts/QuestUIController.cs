using UnityEngine;

public class QuestUIController : MonoBehaviour
{
    public GameObject questScrollPanel;

    public void ToggleScroll()
    {
        bool isActive = questScrollPanel.activeSelf;
        questScrollPanel.SetActive(!isActive);
    }
}
