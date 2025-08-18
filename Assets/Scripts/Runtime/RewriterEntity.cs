using System;
using TMPro;
using UnityEngine;
using VD.Rulebound.CS;

public class RewriterEntity : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public TMP_Text InsertedModuleText;

    [Header("Other")]
    public TextAsset CScript;
    public Inventory PlayerInventory;

    [NonSerialized] public ItemData InsertedItem;
    private CharacterScript _scriptInstance;
    private bool _used = false;

    private void Awake()
    {
        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

    private void OnEnable()
    {
        MenuManager.ButtonPressed += ButtonPressed;
    }

    private void OnDisable()
    {
        MenuManager.ButtonPressed -= ButtonPressed;
    }

    public void OnInteract()
    {
        UpdateUI();
        MenuManager.Instance.OpenMenu("rewriter");
    }

    public bool InsertItem(ItemData item)
    {
        if (InsertedItem != null) return false;
        if (!item.id.Contains("rule")) return false;

        InsertedItem = item;
        return true;
    }

    public ItemData EjectItem()
    {
        if (InsertedItem == null) return null;
        
        ItemData item = InsertedItem;
        InsertedItem = null;
        return item;
    }

    private void UpdateUI()
    {
        InsertedModuleText.text = InsertedItem == null ? "<Insert module>" : InsertedItem.name;
    }

    private void ButtonPressed(MenuButton button, Menu menu)
    {
        if (menu.ID != "rewriter") return;

        switch(button.ID)
        {
            case "rewrite":
                if (InsertedItem == null) return;
                if (InsertedItem.GetUserData<bool>("corrupted")) return;

                if (_used)
                {
                    MenuManager.Instance.CloseMenu();
                    DialogueSystem.Instance.PlayDialogue("rewriterused", _scriptInstance);
                    return;
                }

                InsertedItem.SetUserData("corrupted", true);
                InsertedItem.name = $"<i>{InsertedItem.name}</i>";
                _used = true;
                break;

            case "eject":
                ItemData item = EjectItem();
                if (item != null) PlayerInventory.AddItem(item, true);
                break;
        }

        UpdateUI();
    }
}
