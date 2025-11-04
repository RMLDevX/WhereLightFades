using UnityEngine;

public class GreedNPC : MonoBehaviour
{
    public string npcName = "Villager";

    [Header("Dialogue Lines")]
    public string[] dialogueLines;

    [Header("Speaker for each line (True = Player, False = NPC)")]
    public bool[] speakerIsPlayer;

    [Header("Emotion Sprites (One per dialogue line)")]
    public Sprite[] emotionSprites;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && dialogueLines.Length > 0 && !hasTriggered)
        {
            hasTriggered = true;
            DialogueManager.Instance.StartDialogue(dialogueLines, npcName, speakerIsPlayer, emotionSprites);
        }
    }
}