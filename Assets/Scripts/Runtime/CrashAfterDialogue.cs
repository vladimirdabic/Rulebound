using UnityEngine;
using VD.Rulebound.CS;

public class CrashAfterDialogue : MonoBehaviour
{
    public string DialogueID;
    public bool SaveFlags;

    private void OnEnable()
    {
        CSInterpreter.DialogueEnded += DialogueEnded;
    }

    private void OnDisable()
    {
        CSInterpreter.DialogueEnded -= DialogueEnded;        
    }
    
    private void DialogueEnded(string dialogueId)
    {
        if (dialogueId != DialogueID) return;

        if (SaveFlags)
            StateManager.Save();

        Application.Quit();
    }
}
