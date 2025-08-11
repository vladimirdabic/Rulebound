using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public Inventory Inventory;
    public ItemData Item;

    private static JSONDialogueFile _acquireDiag;
    private static JSONDialogueLine _acquiredLine;
    private static JSONDialogueLine _acquiredInfoLine;

    // Copied from Dialogue System (too lazy to make it reusable)
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

    public void OnInteract()
    {
        if (!enabled) return;
        if (!Inventory.AddItem(Item)) return;

        _acquiredLine.text = $"* You have acquired:\n  {Item.Name}";
        _acquiredInfoLine.text = $"* {Item.Description}";
        Destroy(gameObject, 0.1f);

        JSONDialogueSystem.Instance.PlayDialogue(_acquireDiag);
    }
}
