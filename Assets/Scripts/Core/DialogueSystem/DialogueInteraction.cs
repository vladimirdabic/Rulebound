using System;
using System.Collections.Generic;
using UnityEngine;
using VD.Rulebound.CS;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueInteraction : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public TextAsset CScript;
    public string DialogueID = "main";

    [Header("Flags")]
    public List<Flag> Flags;

    [NonSerialized] public CharacterScript ScriptInstance;

    public void OnInteract()
    {
        if (!enabled) return;
        DialogueSystem.Instance.PlayDialogue(DialogueID, ScriptInstance);
    }

    private void Awake()
    {
        ScriptInstance = CharacterScript.FromText(CScript.text, DialogueID);
        Flags = ScriptInstance.Flags;
    }
}
