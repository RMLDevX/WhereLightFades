using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 15; // Bullet damage
    public float knockbackForce = 12f; // Knockback force to apply
    public AudioClip hitSound; // Sound to play when hitting the spike zone
    public float soundVolume = 1.0f; // Volume of the sound

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {

                stats.TakeDamage(damage);
            }
            ApplySpikeZoneEffect(collision);
            Destroy(gameObject); // Destroy bullet after hitting player
        }

        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject); // Destroy bullet on walls
        }

       
    }

    private void ApplySpikeZoneEffect(Collider2D spikeZoneCollider)
    {
        PlayerStats playerStats = spikeZoneCollider.GetComponentInParent<PlayerStats>();
        if (playerStats == null) return;

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }

        Rigidbody2D rb = playerStats.GetComponent<Rigidbody2D>();
        Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();

        if (rb != null && bulletRb != null)
        {
            // Calculate knockback direction using bullet's travel direction
            // First, calculate direction from the bullet's position
            Vector2 knockbackDirection = (playerStats.transform.position - transform.position).normalized;

            // Apply knockback force in the direction away from the bullet's travel path
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }


}
