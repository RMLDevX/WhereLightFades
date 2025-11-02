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
        // PRESS F to toggle between worlds (only if not transitioning AND not on cooldown AND no dialogue active)
        if (Input.GetKeyDown(toggleKey) && !isTransitioning && !isOnCooldown && !IsDialogueActive())
        {
            ToggleWorlds();
        }
    }

    void ToggleWorlds()
    {
        if (!isTransitioning && !isOnCooldown && !IsDialogueActive())
        {
            StartCoroutine(TransitionEffects());
        }
    }

    // Method to check if dialogue is active
    bool IsDialogueActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel.activeInHierarchy;
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
    }

    void ExitParallelWorld()
    {
        parallelWorld.SetActive(false);
        normalWorld.SetActive(true);
        isParallelWorldActive = false;
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

    // Public method to check if world switching is available
    public bool CanSwitchWorlds()
    {
        return !isTransitioning && !isOnCooldown && !IsDialogueActive();
    }
}