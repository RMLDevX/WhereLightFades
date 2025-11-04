using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public Slider manaBar;
    public GameObject statsPanel; // Reference to your UI panel

    private bool statsActivated = false;
    private bool hasPressedF = false;

    void Start()
    {
        // Hide UI immediately when game starts
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }

        // Hide individual bars as backup
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (manaBar != null) manaBar.gameObject.SetActive(false);

        // Disable life drain until first F press
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.enableLifeDrain = false;
        }
    }

    void Update()
    {
        // Activate stats and life drain on first F press
        if (Input.GetKeyDown(KeyCode.F) && !hasPressedF)
        {
            hasPressedF = true;
            StartCoroutine(ActivateStatsSystemWithDelay());
        }

        // Update UI if stats are active
        if (statsActivated && PlayerStats.Instance != null)
        {
            healthBar.maxValue = PlayerStats.Instance.maxHealth;
            healthBar.value = PlayerStats.Instance.currentHealth;

            manaBar.maxValue = PlayerStats.Instance.maxMana;
            manaBar.value = PlayerStats.Instance.currentMana;
        }
    }

    IEnumerator ActivateStatsSystemWithDelay()
    {
        // Wait for 1 second before showing UI and activating drain
        yield return new WaitForSeconds(1f);

        ActivateStatsSystem();
    }

    void ActivateStatsSystem()
    {
        statsActivated = true;

        // Show UI elements
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }

        // Ensure individual bars are visible
        if (healthBar != null) healthBar.gameObject.SetActive(true);
        if (manaBar != null) manaBar.gameObject.SetActive(true);

        // Enable life drain system
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.enableLifeDrain = true;
            Debug.Log("Stats system activated - Life drain enabled");
        }
    }
}