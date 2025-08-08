using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class JSONDialogueInteraction : MonoBehaviour, IInteractable
{
    public TextAsset Dialogue;
    public bool GivesItem;
    public Inventory InventoryRef;
    public ItemData Item;

    private JSONDialogueFile _dialogue;

    private static JSONDialogueFile _acquireDiag;
    private static JSONDialogueLine _acquiredLine;
    private static JSONDialogueLine _acquiredInfoLine;

    public void OnInteract()
    {
        JSONDialogueSystem.Instance.PlayDialogue(_dialogue);
    }

    private void Awake()
    {
        _dialogue = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);
        if (_acquireDiag != null) return;

        _acquiredLine = new JSONDialogueLine();
        _acquiredInfoLine = new JSONDialogueLine();

        _acquireDiag = new JSONDialogueFile
        {
            dialogue = new JSONDialogue[]
            {
                new JSONDialogue()
                {
                    id = "main",
                    lines = new JSONDialogueLine[]
                    {
                        _acquiredLine,
                        _acquiredInfoLine
                    }
                }
            }
        };
    }

    private void OnEnable()
    {
        JSONDialogueSystem.OnDialogueFileEnded += Instance_OnDialogueEnded;
    }

    private void OnDisable()
    {
        JSONDialogueSystem.OnDialogueFileEnded -= Instance_OnDialogueEnded;
    }

    private void Instance_OnDialogueEnded(JSONDialogueFile diag)
    {
        if (diag != _dialogue) return;
        if (!GivesItem) return;
        
        if(!InventoryRef.AddItem(Item)) return;

        _acquiredLine.text = $"* You have acquired:\n  {Item.Name}";
        _acquiredInfoLine.text = $"* {Item.Description}";

        JSONDialogueSystem.Instance.PlayDialogue(_acquireDiag);
    }
}
