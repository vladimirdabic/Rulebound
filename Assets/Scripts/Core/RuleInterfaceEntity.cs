using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleInterfaceEntity : MonoBehaviour, IInteractable
{
    public TextAsset Dialogue;

    private JSONDialogueFile _dialogueFile;

    private void Awake()
    {
        _dialogueFile = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);    
    }

    public void OnInteract()
    {
        if(RuleInterfaceSystem.Instance.AbandonEnding)
        {
            JSONDialogueSystem.Instance.PlayDialogue(_dialogueFile, "abandonending");
            return;
        }

        RuleInterfaceSystem.Instance.OpenInterface();
    }
}
