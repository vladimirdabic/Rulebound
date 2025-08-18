using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class RuleInterfaceEntity : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public TMP_Text RuleListText;

    [Header("Dialogue References")]
    [SerializeField] private TextAsset CScript;

    public List<ItemData> InsertedRules;
    private CharacterScript _scriptInstance;

    [NonSerialized] public bool AbandonEnding = false;

    private static readonly string[] _allowedRules = new string[]
    {
        "compassionrule",
        "orderrule",
        "selfrule"
    };

    private void Awake()
    {
        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);
    }

    private void OnEnable()
    {
        CSInterpreter.DialogueEnded += OnDialogueEnded;
        InventoryController.ItemDropped += InventorySystem_ItemDropped;
        MenuManager.ButtonPressed += MenuButtonPressed;
    }

    private void OnDisable()
    {
        CSInterpreter.DialogueEnded -= OnDialogueEnded;
        InventoryController.ItemDropped -= InventorySystem_ItemDropped;
        MenuManager.ButtonPressed -= MenuButtonPressed;
    }

    public void OnInteract()
    {
        if (!enabled) return;

        if (AbandonEnding)
        {
            DialogueSystem.Instance.PlayDialogue("abandonending", _scriptInstance);
            return;
        }

        OpenInterface();
    }

    public bool AddRule(ItemData rule)
    {
        if (!_allowedRules.Contains(rule.id)) return false;
        if (InsertedRules.Any(item => item.id == rule.id)) return false;

        InsertedRules.Add(rule);
        return true;
    }

    public void OpenInterface()
    {
        RuleListText.text = string.Empty;
        for (int i = 0; i < InsertedRules.Count; ++i)
        {
            ItemData item = InsertedRules[i];
            RuleListText.text += $"({i + 1}) {item.name}\n";
        }

        MenuManager.Instance.OpenMenu("ruleinterface");
    }

    private void MenuButtonPressed(MenuButton button, Menu menu)
    {
        if (menu.ID != "ruleinterface" && button.ID != "confirm") return;
        MenuManager.Instance.CloseMenu();

        if (InsertedRules.Count < 3)
        {
            DialogueSystem.Instance.PlayDialogue("missing", _scriptInstance);
        }
        else
        {
            DialogueSystem.Instance.PlayDialogue("theendinterface", _scriptInstance);
        }
    }

    private void OnDialogueEnded(string dialogueId)
    {
        switch (dialogueId)
        {
            case "abandonending":
                SceneManager.LoadScene("MainMenuScene");
                break;

            case "theendinterface":
                ItemData corruptedRule = InsertedRules.FirstOrDefault(rule => rule.GetUserData<bool>("corrupted"));

                if(corruptedRule != null)
                {
                    Flag f = CSInterpreter.AddFlag($"c_{corruptedRule.id}");
                    f.Value = true;
                    f.Saved = true;
                }

                SceneManager.LoadScene("EndingScene");
                break;
        }
    }

    private void InventorySystem_ItemDropped(ItemData item, Inventory inv)
    {
        if (!item.id.Contains("rule")) return;

        AbandonEnding = true;
    }
}
