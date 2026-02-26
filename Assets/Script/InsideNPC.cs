using UnityEngine;

public class InsideNPC : MonoBehaviour, IInteractable
{
    [TextArea]
    public string[] lines;

    int index = 0;

    public void Interact()
    {
        if (DialogueManager.Instance.IsTalking) return;

        if (index < lines.Length)
        {
            DialogueManager.Instance.StartDialogue(lines[index], null);
            index++;
        }
    }
}
