using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image leftCharacterImage;  // Player side
    public Image rightCharacterImage; // NPC side

    [Header("Default Sprites")]
    public Sprite playerDefaultSprite;
    public Sprite npcDefaultSprite;

    private List<string> currentDialogue;
    private List<bool> currentSpeakers;
    private List<Sprite> currentEmotions;
    private int currentLine;
    private TutorialPlayerMovement playerMovement;
    private PlayerJump playerJump;
    private Animator playerAnimator;
    private string currentNPCName;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] dialogue, string npcName, bool[] speakers, Sprite[] emotions = null)
    {
        currentDialogue = new List<string>(dialogue);
        currentSpeakers = speakers != null ? new List<bool>(speakers) : new List<bool>();
        currentEmotions = emotions != null ? new List<Sprite>(emotions) : new List<Sprite>();
        currentNPCName = npcName;
        currentLine = 0;
        dialoguePanel.SetActive(true);

        playerMovement = FindObjectOfType<TutorialPlayerMovement>();
        playerJump = FindObjectOfType<PlayerJump>();
        playerAnimator = FindObjectOfType<Animator>();

        // Disable player movement and combat
        if (playerMovement != null)
        {
            playerMovement.SetMovement(false);
            // Force player to idle by setting velocity to zero
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (playerJump != null) playerJump.enabled = false;

        // Reset player animation to idle
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isRunning", false);
            // If you have other animation parameters, reset them here too
            // Example: playerAnimator.SetBool("isJumping", false);
        }

        ShowNextLine();
    }

    void ShowNextLine()
    {
        if (currentLine < currentDialogue.Count)
        {
            dialogueText.text = currentDialogue[currentLine];

            // Determine who is speaking and update UI accordingly
            bool isPlayerSpeaking = currentLine < currentSpeakers.Count ? currentSpeakers[currentLine] : false;

            // Reset both images first
            leftCharacterImage.gameObject.SetActive(false);
            rightCharacterImage.gameObject.SetActive(false);

            if (isPlayerSpeaking)
            {
                // Player is speaking (left side) - No name displayed for player
                nameText.text = ""; // Empty for player
                nameText.color = Color.blue;

                // Show player on left if we have a sprite
                Sprite playerSprite = playerDefaultSprite;
                if (currentLine < currentEmotions.Count && currentEmotions[currentLine] != null)
                {
                    playerSprite = currentEmotions[currentLine];
                }

                if (playerSprite != null)
                {
                    leftCharacterImage.sprite = playerSprite;
                    leftCharacterImage.color = Color.white;
                    leftCharacterImage.gameObject.SetActive(true);
                }

                // Show NPC on right (dimmed) if we have default NPC sprite
                if (npcDefaultSprite != null)
                {
                    rightCharacterImage.sprite = npcDefaultSprite;
                    rightCharacterImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                    rightCharacterImage.gameObject.SetActive(true);
                }
            }
            else
            {
                // NPC is speaking (right side) - Show NPC name
                nameText.text = currentNPCName;
                nameText.color = Color.red;

                // Show NPC on right if we have a sprite
                Sprite npcSprite = npcDefaultSprite;
                if (currentLine < currentEmotions.Count && currentEmotions[currentLine] != null)
                {
                    npcSprite = currentEmotions[currentLine];
                }

                if (npcSprite != null)
                {
                    rightCharacterImage.sprite = npcSprite;
                    rightCharacterImage.color = Color.white;
                    rightCharacterImage.gameObject.SetActive(true);
                }

                // Show player on left (dimmed) if we have default player sprite
                if (playerDefaultSprite != null)
                {
                    leftCharacterImage.sprite = playerDefaultSprite;
                    leftCharacterImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                    leftCharacterImage.gameObject.SetActive(true);
                }
            }

            currentLine++;
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (playerMovement != null) playerMovement.SetMovement(true);
        if (playerJump != null) playerJump.enabled = true;

        // Animation will automatically update when movement is re-enabled
    }

    void Update()
    {
        if (dialoguePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }
}