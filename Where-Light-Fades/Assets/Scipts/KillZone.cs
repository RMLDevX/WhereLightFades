using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered KillZone");

            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(999999f);
                Debug.Log("Damage applied to player");
            }
            else
            {
                // Fallback to singleton instance
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.TakeDamage(999999f);
                    Debug.Log("Damage applied via singleton");
                }
                else
                {
                    Debug.LogError("PlayerStats instance not found!");
                }
            }
        }
    }
}