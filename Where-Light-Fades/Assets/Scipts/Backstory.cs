using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
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

    [Header("Background")]
    public Image blackBackground;

    private int currentTextIndex = 0;
    private bool isTyping = false;
    private bool textComplete = false;

    void Start()
    {
        continuePrompt.SetActive(false);

        SetupBlackBackground();
        StartCoroutine(StartBackstorySequence());
    }

    void SetupBlackBackground()
    {
        if (blackBackground == null)
        {
            blackBackground = FindObjectOfType<Image>();
            if (blackBackground == null)
            {
                CreateBlackBackground();
            }
        }

        if (blackBackground != null)
        {
            blackBackground.color = Color.black;
            RectTransform rect = blackBackground.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }

    void CreateBlackBackground()
    {
        GameObject bgObject = new GameObject("BlackBackground");
        blackBackground = bgObject.AddComponent<Image>();
        blackBackground.color = Color.black;

        RectTransform rect = bgObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsFirstSibling();
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
            LoadSceneByBuildIndex(2); // Load scene number 2
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
                    LoadSceneByBuildIndex(2);
                }
            }
            else if (textComplete)
            {
                NextText();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            LoadSceneByBuildIndex(2); // Skip directly to scene 2
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
            LoadSceneByBuildIndex(2);
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