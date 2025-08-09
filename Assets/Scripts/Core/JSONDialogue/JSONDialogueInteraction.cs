using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class JSONDialogueInteraction : MonoBehaviour, IInteractable
{
    public TextAsset Dialogue;
    public string DialogueID = "main";

    [NonSerialized] public JSONDialogueFile DialogueFile;

    public void OnInteract()
    {
        JSONDialogueSystem.Instance.PlayDialogue(DialogueFile, DialogueID);
    }

    private void Awake()
    {
        DialogueFile = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);
    }
}
