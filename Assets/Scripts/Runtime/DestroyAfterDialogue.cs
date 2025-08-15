using UnityEngine;
using VD.Rulebound.CS;

public class DestroyAfterDialogue : MonoBehaviour
{
    public string DialogueID;

    private void OnEnable()
    {
        CSInterpreter.DialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        CSInterpreter.DialogueEnded -= OnDialogueEnded;
    }

    private void OnDialogueEnded(string dialogueId)
    {
        if (dialogueId != DialogueID) return;

        Destroy(gameObject);
    }

}
