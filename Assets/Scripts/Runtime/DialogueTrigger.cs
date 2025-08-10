using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    public TextAsset Dialogue;
    public string DialogueID = "main";

    [NonSerialized] public JSONDialogueFile DialogueFile;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        JSONDialogueSystem.Instance.PlayDialogue(DialogueFile, DialogueID);
        Destroy(this, 0.1f);
    }

    private void Awake()
    {
        DialogueFile = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);
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
