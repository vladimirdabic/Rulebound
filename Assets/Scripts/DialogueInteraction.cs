using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueInteraction : MonoBehaviour, IInteractable
{
    public Dialogue Dialogue;
    public bool GivesItem;
    public Inventory InventoryRef;
    public ItemData Item;

    private Dialogue _acquireDiag;

    public void OnInteract()
    {
        DialogueSystem.Instance.PlayDialogue(Dialogue);
    }

    private void Awake()
    {
        _acquireDiag = ScriptableObject.CreateInstance<Dialogue>();
        _acquireDiag.Lines = new DialogueLine[] {
            new DialogueLine(),
            new DialogueLine(),
        };
    }

    private void OnEnable()
    {
        DialogueSystem.OnDialogueEnded += Instance_OnDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueSystem.OnDialogueEnded -= Instance_OnDialogueEnded;
    }

    private void Instance_OnDialogueEnded(Dialogue diag)
    {
        if (diag != Dialogue) return;
        if (!GivesItem) return;
        
        if(!InventoryRef.AddItem(Item)) return;

        _acquireDiag.Lines[0].Message = $"* You have acquired:\n  {Item.Name}";
        _acquireDiag.Lines[1].Message = $"* {Item.Description}";

        DialogueSystem.Instance.PlayDialogue(_acquireDiag);
    }
}
