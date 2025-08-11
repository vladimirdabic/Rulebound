using UnityEngine;
using UnityEngine.SceneManagement;

public class JSONInstantDialogue : MonoBehaviour
{
    public TextAsset Dialogue;
    public string DialogueID = "main";
    public string NextScene;


    private JSONDialogueFile _dialogue;

    private void Awake()
    {
        _dialogue = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);
    }

    private void Start()
    {
        JSONDialogueSystem.OnDialogueFileEnded += Instance_OnDialogueEnded;
        JSONDialogueSystem.Instance.PlayDialogue(_dialogue, DialogueID);
    }

    public void OnDestroy()
    {
        JSONDialogueSystem.OnDialogueFileEnded -= Instance_OnDialogueEnded;
    }

    private void Instance_OnDialogueEnded(JSONDialogueFile diag)
    {
        if (NextScene == null || NextScene == string.Empty) return;
        SceneManager.LoadScene(NextScene);
    }
}
