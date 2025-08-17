using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VD.Rulebound.CS;

public class InventoryController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private TextAsset _inventoryCScript;
    [SerializeField] private GameObject _invPanel;
    
    private CharacterScript _cScript;
    private Inventory _inventory;
    private ItemData _selectedItem;

    public static event Action<ItemData, Inventory> ItemUsed;
    public static event Action<ItemData, Inventory> ItemDropped;

    public void OpenInventory(Inventory inventory)
    {
        _inventory = inventory;
        RedrawItemList();
        MenuManager.Instance.OpenMenu("inventory");
    }

    private void Awake()
    {
        _cScript = CharacterScript.FromText(_inventoryCScript.text, _inventoryCScript.name);
        MenuManager.ButtonPressed += MenuButtonPressed;
    }

    private void Start()
    {
        CSInterpreter.DialogueEnded += DiagFinished;
    }

    private void OnDestroy()
    {
        CSInterpreter.DialogueEnded -= DiagFinished;
    }

    private void MenuButtonPressed(MenuButton button, Menu menu)
    {
        if (menu.ID != "inventory") return;

        switch(button.ID)
        {
            case "item":
                _selectedItem = button.GetUserData<ItemData>();
                MenuManager.Instance.SwitchSelectionGroup("action");
                break;

            case "useitem":
                MenuManager.Instance.CloseMenu();
                ItemUsed?.Invoke(_selectedItem, _inventory);
                break;

            case "dropitem":
                DropItem();
                break;
        }
    }

    private void DiagFinished(string dialogueId)
    {
        if (dialogueId != "droppeditem") return;

        StartCoroutine(DelayedReopenInv());
    }

    private IEnumerator DelayedReopenInv()
    {
        yield return null;
        OpenInventory(_inventory);
    }

    private void RedrawItemList()
    {
        SelectionGroup itemsGroup = MenuManager.Instance.GetMenu("inventory").GetSelectionGroup("items");

        // Probably not the best way to do this
        foreach(MenuButton btn in itemsGroup.Buttons)
            Destroy(btn.gameObject);

        itemsGroup.Buttons.Clear();

        for (int i = 0; i < _inventory.Items.Count; ++i)
        {
            ItemData item = _inventory.Items[i];

            GameObject obj = new GameObject();
            obj.AddComponent<CanvasRenderer>();
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            MenuButton btn = obj.AddComponent<MenuButton>();

            obj.transform.SetParent(_invPanel.transform, false);
            txt.text = item.name;
            txt.font = _font;
            txt.fontSize = 28;

            RectTransform rect = txt.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(295, 30);
            rect.anchoredPosition = new Vector2(80.60199f, -66.3f - (30 * i));

            btn.SetUserData(item);
            btn.ID = "item";
            btn.SelectionOrder = i;
            itemsGroup.Buttons.Add(btn);
        }
    }

    private void DropItem()
    {
        DialogueStmt stmt = _cScript.GetDialogue("droppeditem").Statements[0];

        if (_selectedItem.undroppable)
        {
            ((DialogueStmt.Line)stmt).Text = $"* You tried dropping:\n  {_selectedItem.name}\n  ...but you couldn't";
        }
        else
        {
            _inventory.Items.Remove(_selectedItem);
            ItemDropped?.Invoke(_selectedItem, _inventory);

            // Still hacky... Could implement something like templated strings in Character Scripts
            ((DialogueStmt.Line)stmt).Text = $"* Dropped {_selectedItem.name}";
        }

        MenuManager.Instance.CloseMenu();
        DialogueSystem.Instance.PlayDialogue("droppeditem", _cScript);
    }
}
