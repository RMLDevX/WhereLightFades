using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Instant kill
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.TakeDamage(999999f);
        }
    }
}