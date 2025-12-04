using UnityEngine;
using System.Collections;

public class Hazard : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 15f;
    public float damageInterval = 0.5f;
    public bool canDamage = true;

    [Header("Knockback Settings")]
    public bool knockback = true;
    public Vector2 knockbackDirection = new Vector2(0, 1);
    public float knockbackForce = 12f;
    public bool resetVerticalVelocity = true;

    [Header("Audio Settings")]
    public AudioClip damageSound; // Sound when player gets damaged
    public AudioClip hitSound; // Sound when object is hit (optional)
    public float soundVolume = 1.0f;
    public bool playSoundOnDamage = true;
    public bool playSoundOnCollision = false;

    [Header("Visual Effects")]
    public GameObject damageEffectPrefab;
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Randomization")]
    public bool randomizePitch = false;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    // Private variables
    private float nextDamageTime = 0f;
    private AudioSource audioSource;

    void Start()
    {
        // Create or get AudioSource component
        SetupAudioSource();

        // Make sure this object is on the Hazard layer
        if (gameObject.layer != LayerMask.NameToLayer("Hazard"))
        {
            Debug.LogWarning($"{gameObject.name} should be on the 'Hazard' layer for better collision management");
        }
    }

    void SetupAudioSource()
    {
        // Try to get existing AudioSource
        audioSource = GetComponent<AudioSource>();

        // If no AudioSource exists, create one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.8f; // 3D sound (adjust as needed)
            audioSource.maxDistance = 20f;
            audioSource.volume = soundVolume;
        }

        // Configure audio source
        audioSource.volume = soundVolume;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (playSoundOnCollision && hitSound != null)
        {
            PlaySound(hitSound);
        }

        HandleDamage(collision.gameObject, collision.GetContact(0).point);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleDamage(collision.gameObject, collision.GetContact(0).point);
    }

    void HandleDamage(GameObject target, Vector2 contactPoint)
    {
        if (!canDamage) return;
        if (Time.time < nextDamageTime) return;
        if (!target.CompareTag("Player")) return;

        PlayerStats playerStats = target.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        // Apply damage
        playerStats.TakeDamage(damage);
        nextDamageTime = Time.time + damageInterval;

        // Play damage sound
        if (playSoundOnDamage && damageSound != null)
        {
            PlaySound(damageSound);

            // Optionally also play from player (for 3D positioning)
            PlaySoundFromPlayer(target, damageSound);
        }

        // Apply knockback
        if (knockback)
        {
            ApplyKnockback(target, contactPoint);
        }

        // Visual effects
        CreateDamageEffect(contactPoint);

        // Flash player
        StartCoroutine(FlashPlayer(target));

        Debug.Log($"{target.name} took {damage} damage from {gameObject.name}");
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // Randomize pitch if enabled
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1.0f;
        }

        // Play the sound
        audioSource.PlayOneShot(clip, soundVolume);
    }

    void PlaySoundFromPlayer(GameObject player, AudioClip clip)
    {
        // This plays the sound from the player's position (good for 3D audio)
        if (clip == null) return;

        // Get or add AudioSource to player
        AudioSource playerAudio = player.GetComponent<AudioSource>();
        if (playerAudio == null)
        {
            playerAudio = player.AddComponent<AudioSource>();
            playerAudio.playOnAwake = false;
            playerAudio.spatialBlend = 0.8f;
        }

        // Play the sound from player's position
        playerAudio.PlayOneShot(clip, soundVolume * 0.7f); // Slightly quieter when playing from player
    }

    void ApplyKnockback(GameObject player, Vector2 contactPoint)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Reset vertical velocity for consistent knockback
        if (resetVerticalVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        // Calculate direction from contact point
        Vector2 direction = ((Vector2)player.transform.position - contactPoint).normalized;

        // Use custom direction if specified, otherwise use calculated direction
        Vector2 finalDirection = (knockbackDirection != Vector2.zero) ?
            knockbackDirection.normalized : direction;

        // Add the force
        rb.AddForce(finalDirection * knockbackForce, ForceMode2D.Impulse);
    }

    void CreateDamageEffect(Vector2 position)
    {
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, position, Quaternion.identity);
        }
    }

    IEnumerator FlashPlayer(GameObject player)
    {
        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            sprite.color = originalColor;
        }
    }

    // Public methods to control the hazard
    public void EnableDamage() => canDamage = true;
    public void DisableDamage() => canDamage = false;
    public void SetDamage(float newDamage) => damage = newDamage;

    // Audio control methods
    public void SetDamageSound(AudioClip newSound)
    {
        damageSound = newSound;
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
        }
    }

    public void PlayTestSound()
    {
        if (damageSound != null)
        {
            PlaySound(damageSound);
        }
    }
}