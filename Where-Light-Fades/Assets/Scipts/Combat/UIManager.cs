
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public Slider manaBar;

    void Update()
    {
        if (PlayerStats.Instance != null)
        {
            healthBar.maxValue = PlayerStats.Instance.maxHealth;
            healthBar.value = PlayerStats.Instance.currentHealth;

            manaBar.maxValue = PlayerStats.Instance.maxMana;
            manaBar.value = PlayerStats.Instance.currentMana;
        }
    }
}