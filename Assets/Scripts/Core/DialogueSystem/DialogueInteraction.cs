using System;
using System.Collections.Generic;
using UnityEngine;
using VD.Rulebound.CS;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Character))]
public class DialogueInteraction : MonoBehaviour, IInteractable
{
    public string DialogueID = "main";

    private Character _character;
    
    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    public void OnInteract()
    {
        if (!enabled) return;
        DialogueSystem.Instance.PlayDialogue(DialogueID, _character.ScriptInstance);
    }

}
