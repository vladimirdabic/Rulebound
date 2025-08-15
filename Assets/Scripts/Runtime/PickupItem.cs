using System.Collections.Generic;
using UnityEngine;
using VD.Rulebound.CS;

public class PickupItem : MonoBehaviour, IInteractable
{
    public Inventory Inventory;
    public TextAsset CScript;
    public string ItemID;

    private static CharacterScript _scriptInstance;

    private void Awake()
    {
        if (_scriptInstance != null) return;

        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

    public void OnInteract()
    {
        if (!enabled) return;

        ItemData item = ItemManager.Instance.GetItemByID(ItemID);
        if (!Inventory.AddItem(item, false)) return;

        List<DialogueStmt> lines = _scriptInstance.GetDialogue("acquireditem").Statements;
        var l1 = (DialogueStmt.Line)lines[0];
        var l2 = (DialogueStmt.Line)lines[1];

        l1.Text = $"* You have acquired:\n  {item.name}";
        l2.Text = $"* {item.description}";

        DialogueSystem.Instance.PlayDialogue("acquireditem", _scriptInstance);
        Destroy(gameObject);
    }
}
