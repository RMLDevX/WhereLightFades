using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public Slider manaBar;
    public GameObject statsPanel; 

    private bool statsActivated = false;
    private bool hasPressedF = false;

    void Start()
    {
        
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
        
        yield return new WaitForSeconds(1f);

        ActivateStatsSystem();
    }

    void ActivateStatsSystem()
    {
        statsActivated = true;

        
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }

        
        if (healthBar != null) healthBar.gameObject.SetActive(true);
        if (manaBar != null) manaBar.gameObject.SetActive(true);

        
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.enableLifeDrain = true;
            Debug.Log("Stats system activated - Life drain enabled");
        }
    }
}