using System;
using UnityEngine;
using VD.Rulebound.CS;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Character Script")]
    public TextAsset CScript;
    public string DialogueID = "main";

    [Header("State")]
    public bool DestroyAfter = true;
    public bool Enabled = true;
    public string EnableAfterDialogue;
    public string DisableAfterDialogue;

    [NonSerialized] public CharacterScript ScriptInstance;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!Enabled) return;
        if (!other.CompareTag("Player")) return;
        DialogueSystem.Instance.PlayDialogue(DialogueID, ScriptInstance);
        if(DestroyAfter) Destroy(this, 0.1f);
    }

    private void Awake()
    {
        ScriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

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
        if (dialogueId == EnableAfterDialogue)
            Enabled = true;
        else if(dialogueId == DisableAfterDialogue)
            Enabled = false;
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)collider.offset,
            collider.size
        );
    }
}
