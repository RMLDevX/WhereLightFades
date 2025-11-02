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
    public float cooldownTime = 0.5f; // NEW: Cooldown after transition

    [Header("Audio Settings")]
    public AudioClip toParallelSound;
    public AudioClip toNormalSound;
    public float audioFadeTime = 0.5f;

    [Header("Light Settings")]
    public Light worldLight;
    public float normalLightIntensity = 1f;
    public float parallelLightIntensity = 0.3f;

    [Header("Debug")]
    public bool isParallelWorldActive = false;
    public bool isTransitioning = false;
    public bool isOnCooldown = false; // NEW: Cooldown state

    private AudioSource audioSource;
    private AudioSource backgroundAudioSource;

    void Start()
    {
        normalWorld.SetActive(true);
        parallelWorld.SetActive(false);
        isParallelWorldActive = false;

        // Setup audio sources
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Create separate audio source for background music (if needed)
        backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        backgroundAudioSource.loop = true;
    }

    void Update()
    {
        // PRESS F to toggle between worlds (only if not transitioning AND not on cooldown)
        if (Input.GetKeyDown(toggleKey) && !isTransitioning && !isOnCooldown)
        {
            ToggleWorlds();
        }
    }

    void ToggleWorlds()
    {
        if (!isTransitioning && !isOnCooldown)
        {
            StartCoroutine(TransitionEffects());
        }
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

        // 2. Fade out current background audio (if any)
        yield return StartCoroutine(FadeAudio(backgroundAudioSource, backgroundAudioSource.volume, 0f, audioFadeTime / 2));

        // 3. Dim lights during transition
        if (worldLight != null)
        {
            float targetIntensity = isParallelWorldActive ? normalLightIntensity : parallelLightIntensity;
            StartCoroutine(DimLights(worldLight.intensity, targetIntensity, transitionDelay));
        }

        // 4. Wait a bit before switching worlds
        yield return new WaitForSeconds(transitionDelay / 3);

        // 5. Switch worlds
        if (isParallelWorldActive)
        {
            ExitParallelWorld();
        }
        else
        {
            EnterParallelWorld();
        }

        // 6. Wait a bit after switching
        yield return new WaitForSeconds(transitionDelay / 3);

        // 7. Fade in new background audio (if any)
        yield return StartCoroutine(FadeAudio(backgroundAudioSource, 0f, 1f, audioFadeTime / 2));

        // 8. Start cooldown period
        isTransitioning = false;
        StartCoroutine(StartCooldown());
    }

    // NEW: Cooldown coroutine
    IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    IEnumerator FadeAudio(AudioSource audioSrc, float from, float to, float duration)
    {
        if (audioSrc == null || !audioSrc.isPlaying) yield break;

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

        // You can play parallel world background music here
        // PlayBackgroundAudio(parallelWorldMusic);
    }

    void ExitParallelWorld()
    {
        parallelWorld.SetActive(false);
        normalWorld.SetActive(true);
        isParallelWorldActive = false;

        // You can play normal world background music here
        // PlayBackgroundAudio(normalWorldMusic);
    }

    // Optional: Method to handle background music
    public void PlayBackgroundAudio(AudioClip musicClip)
    {
        if (musicClip != null && backgroundAudioSource != null)
        {
            backgroundAudioSource.clip = musicClip;
            backgroundAudioSource.volume = 0f;
            backgroundAudioSource.Play();
            StartCoroutine(FadeAudio(backgroundAudioSource, 0f, 1f, audioFadeTime));
        }
    }

    // Optional: Method to stop background music
    public void StopBackgroundAudio()
    {
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            StartCoroutine(FadeAudio(backgroundAudioSource, backgroundAudioSource.volume, 0f, audioFadeTime));
        }
    }

    // NEW: Public method to check if world switching is available
    public bool CanSwitchWorlds()
    {
        return !isTransitioning && !isOnCooldown;
    }
}