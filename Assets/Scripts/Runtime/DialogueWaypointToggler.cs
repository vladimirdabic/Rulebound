using UnityEngine;

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
        JSONDialogueSystem.OnDialogueEnded += JSONDialogueSystem_OnDialogueEnded;   
    }

    private void OnDisable()
    {
        JSONDialogueSystem.OnDialogueEnded -= JSONDialogueSystem_OnDialogueEnded;
    }

    private void JSONDialogueSystem_OnDialogueEnded(JSONDialogue diag)
    {
        if(diag.id == EnableDialogueID)
        {
            _collider.isTrigger = true;
            _wpHandler.enabled = true;
        }
        else if(diag.id == DisableDialogueID)
        {
            _collider.isTrigger = false;
            _wpHandler.enabled = false;
        }
    }
}
