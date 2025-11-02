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
    public AudioClip transitionSound;

    [Header("Light Settings")]
    public Light worldLight;
    public float normalLightIntensity = 1f;
    public float parallelLightIntensity = 0.3f;

    [Header("Screen Effect")]
    public CanvasGroup fadeOverlay;

    [Header("Debug")]
    public bool isParallelWorldActive = false;
    public bool isTransitioning = false;

    private AudioSource audioSource;

    void Start()
    {
        normalWorld.SetActive(true);
        parallelWorld.SetActive(false);
        isParallelWorldActive = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // PRESS F to toggle between worlds
        if (Input.GetKeyDown(toggleKey) && !isTransitioning)
        {
            ToggleWorlds();
        }
    }

    void ToggleWorlds()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionEffects());
        }
    }

    IEnumerator TransitionEffects()
    {
        isTransitioning = true;

        // 1. Play transition sound
        if (transitionSound != null)
            audioSource.PlayOneShot(transitionSound);

        // 2. Start fade out
        if (fadeOverlay != null)
            yield return StartCoroutine(FadeScreen(0f, 1f, transitionDelay / 2));

        // 3. Dim lights during transition
        if (worldLight != null)
        {
            float targetIntensity = isParallelWorldActive ? normalLightIntensity : parallelLightIntensity;
            StartCoroutine(DimLights(worldLight.intensity, targetIntensity, transitionDelay));
        }

        // 4. Wait a bit in the middle
        yield return new WaitForSeconds(transitionDelay / 4);

        // 5. Switch worlds (happens in middle of transition)
        if (isParallelWorldActive)
        {
            ExitParallelWorld();
        }
        else
        {
            EnterParallelWorld();
        }

        // 6. Wait a bit more
        yield return new WaitForSeconds(transitionDelay / 4);

        // 7. Fade back in
        if (fadeOverlay != null)
            yield return StartCoroutine(FadeScreen(1f, 0f, transitionDelay / 2));

        isTransitioning = false;
    }

    IEnumerator FadeScreen(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeOverlay.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.alpha = to;
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
}