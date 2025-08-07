using UnityEngine;
using UnityEngine.SceneManagement;

public class InstantDialogue : MonoBehaviour
{
    public Dialogue Dialogue;

    private void Start()
    {
        DialogueSystem.OnDialogueEnded += Instance_OnDialogueEnded;
        DialogueSystem.Instance.PlayDialogue(Dialogue);
    }

    public void OnDestroy()
    {
        DialogueSystem.OnDialogueEnded -= Instance_OnDialogueEnded;
    }

    private void Instance_OnDialogueEnded(Dialogue obj)
    {
        SceneManager.LoadScene("MainScene");
    }
}
