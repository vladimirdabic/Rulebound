using UnityEngine;

public class DestroyAfterDialogue : MonoBehaviour
{
    public string DialogueID;

    private void OnEnable()
    {
        JSONDialogueSystem.OnDialogueEnded += JSONDialogueSystem_OnDialogueEnded;
    }

    private void OnDisable()
    {
        JSONDialogueSystem.OnDialogueEnded -= JSONDialogueSystem_OnDialogueEnded;
    }

    private void JSONDialogueSystem_OnDialogueEnded(JSONDialogue diag)
    {
        if (diag.id != DialogueID) return;

        Destroy(gameObject);
    }

}
