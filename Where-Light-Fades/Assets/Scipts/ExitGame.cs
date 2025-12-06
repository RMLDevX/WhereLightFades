using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }
}
