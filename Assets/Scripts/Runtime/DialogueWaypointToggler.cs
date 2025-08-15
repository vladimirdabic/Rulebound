using UnityEngine;
using VD.Rulebound.CS;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(WaypointHandler))]
public class WaypointTogglerDialogue : MonoBehaviour
{
    public string EnableDialogueID;
    public string DisableDialogueID;

    private BoxCollider2D _collider;
    private WaypointHandler _wpHandler;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _wpHandler = GetComponent<WaypointHandler>();
    }

    private void OnEnable()
    {
        CSInterpreter.DialogueEnded += OnDialogueEnded;   
    }

    private void OnDisable()
    {
        CSInterpreter.DialogueEnded -= OnDialogueEnded;
    }

    private void OnDialogueEnded(string dialogueId)
    {
        if(dialogueId == EnableDialogueID)
        {
            _collider.isTrigger = true;
            _wpHandler.enabled = true;
        }
        else if(dialogueId == DisableDialogueID)
        {
            _collider.isTrigger = false;
            _wpHandler.enabled = false;
        }
    }
}
