using System;
using UnityEngine;
using VD.Rulebound.CS;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    public TextAsset CScript;
    public string DialogueID = "main";

    [NonSerialized] public CharacterScript ScriptInstance;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DialogueSystem.Instance.PlayDialogue(DialogueID, ScriptInstance);
        Destroy(this, 0.1f);
    }

    private void Awake()
    {
        ScriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
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
