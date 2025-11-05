using UnityEngine;
using System.Collections;

public class ParallelWorldManager : MonoBehaviour
{
    [Header("World Objects")]
    public GameObject normalWorld;
    public GameObject parallelWorld;

    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Transition Settings")]
    public float transitionDelay = 1.5f;
    public float cooldownTime = 0.5f;

    [Header("Audio Settings")]
    public AudioClip toParallelSound;
    public AudioClip toNormalSound;
    public AudioClip parallelWorldMusic; // Add this for parallel world music
    public AudioClip normalWorldMusic;   // Add this for normal world music
    public float audioFadeTime = 0.5f;

    [Header("Light Settings")]
    public Light worldLight;
    public float normalLightIntensity = 1f;
    public float parallelLightIntensity = 0.3f;

    [Header("Debug")]
    public bool isParallelWorldActive = false;
    public bool isTransitioning = false;
    public bool isOnCooldown = false;
    public bool canSwitchToParallel = true; // Control whether player can switch to parallel world

    private AudioSource audioSource;
    private AudioSource backgroundAudioSource;

    void Start()
    {
        normalWorld.SetActive(true);
        parallelWorld.SetActive(false);
        isParallelWorldActive = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        backgroundAudioSource.loop = true;

        // Start with normal world music if available
        if (normalWorldMusic != null)
        {
            PlayBackgroundAudio(normalWorldMusic);
        }
    }

    void Update()
    {
        // PRESS F to toggle between worlds (only if not transitioning AND not on cooldown AND no dialogue active AND has mana)
        if (Input.GetKeyDown(toggleKey) && !isTransitioning && !isOnCooldown && !IsDialogueActive() && CanSwitchWorlds())
        {
            ToggleWorlds();
        }
    }

    void ToggleWorlds()
    {
        if (!isTransitioning && !isOnCooldown && !IsDialogueActive() && CanSwitchWorlds())
        {
            StartCoroutine(TransitionEffects());
        }
    }

    // Method to check if dialogue is active
    bool IsDialogueActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel.activeInHierarchy;
    }

    // Enhanced method to check if world switching is available
    public bool CanSwitchWorlds()
    {
        // Can always switch back to normal world
        if (isParallelWorldActive)
            return true;

        // Can only switch to parallel world if we have mana and switching is allowed
        if (!isParallelWorldActive)
            return canSwitchToParallel && HasEnoughMana();

        return !isTransitioning && !isOnCooldown && !IsDialogueActive();
    }

    // Check if player has enough mana to enter parallel world
    bool HasEnoughMana()
    {
        return PlayerStats.Instance != null && PlayerStats.Instance.currentMana > 0;
    }

    IEnumerator TransitionEffects()
    {
        isTransitioning = true;

        // 1. Play appropriate transition sound
        AudioClip transitionSound = isParallelWorldActive ? toNormalSound : toParallelSound;
        if (transitionSound != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // 2. Fade out current background audio
        yield return StartCoroutine(FadeAudio(backgroundAudioSource, backgroundAudioSource.volume, 0f, audioFadeTime / 2));

        // 3. Stop the background audio completely after fade out
        if (backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Stop();
        }

        // 4. Dim lights during transition
        if (worldLight != null)
        {
            float targetIntensity = isParallelWorldActive ? normalLightIntensity : parallelLightIntensity;
            StartCoroutine(DimLights(worldLight.intensity, targetIntensity, transitionDelay));
        }

        // 5. Wait a bit before switching worlds
        yield return new WaitForSeconds(transitionDelay / 3);

        // 6. Switch worlds
        if (isParallelWorldActive)
        {
            ExitParallelWorld();
        }
        else
        {
            EnterParallelWorld();
        }

        // 7. Wait a bit after switching
        yield return new WaitForSeconds(transitionDelay / 3);

        // 8. Start new background music for the current world
        AudioClip targetMusic = isParallelWorldActive ? parallelWorldMusic : normalWorldMusic;
        if (targetMusic != null)
        {
            PlayBackgroundAudio(targetMusic);
        }

        // 9. Start cooldown period
        isTransitioning = false;
        StartCoroutine(StartCooldown());
    }

    // Cooldown coroutine
    IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    IEnumerator FadeAudio(AudioSource audioSrc, float from, float to, float duration)
    {
        if (audioSrc == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (audioSrc != null)
                audioSrc.volume = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (audioSrc != null)
            audioSrc.volume = to;
    }

    IEnumerator DimLights(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (worldLight != null)
                worldLight.intensity = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (worldLight != null)
            worldLight.intensity = to;
    }

    void EnterParallelWorld()
    {
        normalWorld.SetActive(false);
        parallelWorld.SetActive(true);
        isParallelWorldActive = true;

        // Activate player stats and set parallel world state
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetParallelWorldState(true);
        }
    }

    void ExitParallelWorld()
    {
        parallelWorld.SetActive(false);
        normalWorld.SetActive(true);
        isParallelWorldActive = false;

        // Deactivate parallel world effects
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetParallelWorldState(false);
        }
    }

    // Public method to force switch to normal world (for mana depletion)
    public void ForceSwitchToNormalWorld()
    {
        if (isParallelWorldActive && !isTransitioning)
        {
            StartCoroutine(ForceSwitchCoroutine());
        }
    }

    IEnumerator ForceSwitchCoroutine()
    {
        isTransitioning = true;

        // Play transition sound
        if (toNormalSound != null)
        {
            audioSource.PlayOneShot(toNormalSound);
        }

        // Fade out audio
        yield return StartCoroutine(FadeAudio(backgroundAudioSource, backgroundAudioSource.volume, 0f, audioFadeTime / 2));

        if (backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Stop();
        }

        // Dim lights
        if (worldLight != null)
        {
            StartCoroutine(DimLights(worldLight.intensity, normalLightIntensity, transitionDelay));
        }

        yield return new WaitForSeconds(transitionDelay / 3);

        // Switch to normal world
        ExitParallelWorld();

        yield return new WaitForSeconds(transitionDelay / 3);

        // Start normal world music
        if (normalWorldMusic != null)
        {
            PlayBackgroundAudio(normalWorldMusic);
        }

        isTransitioning = false;

        // Prevent switching back to parallel world until mana is restored
        canSwitchToParallel = false;
        Debug.Log("Cannot switch to parallel world - mana depleted!");
    }

    // Method to re-enable parallel world switching (call this when mana is restored)
    public void EnableParallelWorldSwitching()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.currentMana > 0)
        {
            canSwitchToParallel = true;
            Debug.Log("Parallel world switching re-enabled");
        }
    }

    // Method to handle background music
    public void PlayBackgroundAudio(AudioClip musicClip)
    {
        if (musicClip != null && backgroundAudioSource != null)
        {
            backgroundAudioSource.clip = musicClip;
            backgroundAudioSource.volume = 0f; // Start at 0 volume
            backgroundAudioSource.Play();
            StartCoroutine(FadeAudio(backgroundAudioSource, 0f, 1f, audioFadeTime));
        }
    }

    // Method to stop background music immediately
    public void StopBackgroundAudioImmediate()
    {
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Stop();
        }
    }

    // Method to stop background music with fade out
    public void StopBackgroundAudio()
    {
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            StartCoroutine(FadeAudio(backgroundAudioSource, backgroundAudioSource.volume, 0f, audioFadeTime));
            StartCoroutine(StopAudioAfterFade(backgroundAudioSource, audioFadeTime));
        }
    }

    // Helper coroutine to stop audio after fade out
    private IEnumerator StopAudioAfterFade(AudioSource audioSrc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSrc != null)
        {
            audioSrc.Stop();
        }
    }
}