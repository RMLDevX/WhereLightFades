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
    public Image characterImage;

    private List<string> currentDialogue;
    private List<Sprite> currentEmotions;
    private int currentLine;
    private TutorialPlayerMovement playerMovement;
    private PlayerJump playerJump;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] dialogue, string npcName, Sprite[] emotions = null)
    {
        currentDialogue = new List<string>(dialogue);
        currentEmotions = emotions != null ? new List<Sprite>(emotions) : new List<Sprite>();
        currentLine = 0;
        nameText.text = npcName;
        dialoguePanel.SetActive(true);

        playerMovement = FindObjectOfType<TutorialPlayerMovement>();
        playerJump = FindObjectOfType<PlayerJump>();

        if (playerMovement != null) playerMovement.SetMovement(false);
        if (playerJump != null) playerJump.enabled = false;

        ShowNextLine();
    }

    void ShowNextLine()
    {
        if (currentLine < currentDialogue.Count)
        {
            dialogueText.text = currentDialogue[currentLine];

            // Change image if available for this line
            if (currentLine < currentEmotions.Count && currentEmotions[currentLine] != null)
            {
                characterImage.sprite = currentEmotions[currentLine];
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
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
    }

    void Update()
    {
        if (dialoguePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }
}