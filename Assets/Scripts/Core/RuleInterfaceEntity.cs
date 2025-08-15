using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class RuleInterfaceEntity : MonoBehaviour, IInteractable
{
    public TextAsset CScript;

    private CharacterScript _scriptInstance;

    private void Awake()
    {
        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

    public void OnInteract()
    {
        if (!enabled) return;

        if(RuleInterfaceSystem.Instance.AbandonEnding)
        {
            DialogueSystem.Instance.PlayDialogue("abandonending", _scriptInstance);
            return;
        }

        RuleInterfaceSystem.Instance.OpenInterface();
    }
}
