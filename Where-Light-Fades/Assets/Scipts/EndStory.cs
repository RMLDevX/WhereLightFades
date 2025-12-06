using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class TypewriterEffectEnd : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f;
    public float delayBeforeStart = 1f;
    public float delayAfterComplete = 2f;
    public float fadeDuration = 1f;

    [Header("Text Content")]
    [TextArea(3, 10)]
    public string[] backstoryTexts;

    [Header("UI References")]
    public TMP_Text dialogueText;
    public GameObject continuePrompt;
    public Image backgroundImage; // Reference to your image

    private int currentTextIndex = 0;
    private bool isTyping = false;
    private bool textComplete = false;

    void Start()
    {
        continuePrompt.SetActive(false);

        // Make sure the background image is set up properly
        SetupBackground();
        StartCoroutine(StartBackstorySequence());
    }

    void SetupBackground()
    {
        // If you haven't assigned the background image in the inspector,
        // try to find it or create a placeholder
        if (backgroundImage == null)
        {
            // Try to find an existing Image component that's not the black background
            Image[] images = FindObjectsOfType<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject.name != "BlackBackground")
                {
                    backgroundImage = img;
                    break;
                }
            }
        }

        // If we still don't have a background image, you might want to assign one
        if (backgroundImage == null)
        {
            Debug.LogWarning("No background image found. Please assign one in the inspector.");
        }

        // Remove or hide the black background if it exists
        RemoveBlackBackground();
    }

    void RemoveBlackBackground()
    {
        // Find and destroy the black background if it was created
        GameObject blackBg = GameObject.Find("BlackBackground");
        if (blackBg != null)
        {
            Destroy(blackBg);
        }

        // Also check if there's a black background in the scene
        Image[] allImages = FindObjectsOfType<Image>();
        foreach (Image img in allImages)
        {
            if (img.color == Color.black && img.gameObject.name != "BackgroundImage")
            {
                // This might be our black background - disable or destroy it
                img.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator StartBackstorySequence()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        StartCoroutine(TypeText(backstoryTexts[currentTextIndex]));
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        textComplete = false;
        dialogueText.text = "";

        foreach (char letter in textToType)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        textComplete = true;

        // Check if this is the last text
        if (currentTextIndex == backstoryTexts.Length - 1)
        {
            // Last text - show continue prompt but auto-proceed after delay
            continuePrompt.SetActive(true);
            yield return new WaitForSeconds(delayAfterComplete);
            LoadSceneByBuildIndex(0); // Load scene number 2
        }
        else
        {
            // Not last text - wait for player input
            continuePrompt.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = backstoryTexts[currentTextIndex];
                isTyping = false;
                textComplete = true;
                continuePrompt.SetActive(true);

                // If this was the last text and player skipped, proceed to scene 2
                if (currentTextIndex == backstoryTexts.Length - 1)
                {
                    LoadSceneByBuildIndex(0);
                }
            }
            else if (textComplete)
            {
                NextText();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            LoadSceneByBuildIndex(0); // Skip directly to scene 2
    }

    void NextText()
    {
        currentTextIndex++;
        continuePrompt.SetActive(false);

        if (currentTextIndex < backstoryTexts.Length)
        {
            StartCoroutine(TypeText(backstoryTexts[currentTextIndex]));
        }
        else
        {
            // All texts completed - proceed to scene 2
            LoadSceneByBuildIndex(0);
        }
    }

    void LoadSceneByBuildIndex(int buildIndex)
    {
        StartCoroutine(LoadSceneWithFade(buildIndex));
    }

    IEnumerator LoadSceneWithFade(int buildIndex)
    {
        // Fade out screen
        yield return StartCoroutine(FadeOut());

        // Load scene by build index
        SceneManager.LoadScene(buildIndex);
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}