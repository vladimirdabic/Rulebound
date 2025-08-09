using UnityEngine;

public class JSONDialogueItemGiver : MonoBehaviour
{
    public string DialogueID;
    public Inventory InventoryRef;
    public ItemData Item;

    private static JSONDialogueFile _acquireDiag;
    private static JSONDialogueLine _acquiredLine;
    private static JSONDialogueLine _acquiredInfoLine;

    private void Awake()
    {
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
        JSONDialogueSystem.OnDialogueEnded += JSONDialogueSystem_OnDialogueEnded;
    }

    private void OnDisable()
    {
        JSONDialogueSystem.OnDialogueEnded -= JSONDialogueSystem_OnDialogueEnded;
    }

    private void JSONDialogueSystem_OnDialogueEnded(JSONDialogue diag)
    {
        if (diag.id != DialogueID) return;
        AddItem(Item);
    }

    public void AddItem(ItemData item)
    {
        if (!InventoryRef.AddItem(item)) return;

        _acquiredLine.text = $"* You have acquired:\n  {item.Name}";
        _acquiredInfoLine.text = $"* {item.Description}";

        JSONDialogueSystem.Instance.PlayDialogue(_acquireDiag);
    }
}
