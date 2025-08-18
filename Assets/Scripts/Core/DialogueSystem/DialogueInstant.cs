using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class DialogueInstant : MonoBehaviour
{
    public TextAsset CScript;
    public string DialogueID = "main";
    public SceneJump[] AfterDialogueJumps;
    
    private CharacterScript _scriptInstance;

    [Serializable]
    public class SceneJump
    {
        public string DialogueID;
        public string SceneName;
    }

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
        SceneJump next = AfterDialogueJumps?.FirstOrDefault(sj => sj.DialogueID == lastDiagId);

        if(next != null) SceneManager.LoadScene(next.SceneName);
    }
}
