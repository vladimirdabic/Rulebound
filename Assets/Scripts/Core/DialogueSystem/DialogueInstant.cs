using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class DialogueInstant : MonoBehaviour
{
    public TextAsset CScript;
    public string DialogueID = "main";
    public string NextScene;

    private CharacterScript _scriptInstance;

    private void Awake()
    {
        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

    private void Start()
    {
        CSInterpreter.DialogueChainEnded += OnDialogueChainEnded;
        DialogueSystem.Instance.PlayDialogue(DialogueID, _scriptInstance);
    }

    public void OnDestroy()
    {
        CSInterpreter.DialogueChainEnded -= OnDialogueChainEnded;
    }

    private void OnDialogueChainEnded(string lastDiagId)
    {
        if (NextScene == null || NextScene == string.Empty) return;
        SceneManager.LoadScene(NextScene);
    }
}
