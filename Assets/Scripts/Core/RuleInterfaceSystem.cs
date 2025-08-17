using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class RuleInterfaceSystem : MonoBehaviour
{
    public static RuleInterfaceSystem Instance;

    [Header("UI References")]
    public GameObject InterfacePanel;
    public TMP_Text RuleListText;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Dialogue References")]
    [SerializeField] private TextAsset CScript;

    public List<ItemData> InsertedRules;
    private InputAction _confirmAction;
    private InputAction _cancelAction;
    private CharacterScript _scriptInstance;

    [NonSerialized] public bool AbandonEnding = false;

    private static readonly string[] _allowedRules = new string[]
    {
        "Rule Of Compassion",
        "Rule Of Order",
        "Rule Of Self"
    };

    public bool AddRule(ItemData rule)
    {
        if (!_allowedRules.Contains(rule.name)) return false;
        if (InsertedRules.Any(item => item.name == rule.name)) return false;

        InsertedRules.Add(rule);
        return true;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _scriptInstance = CharacterScript.FromText(CScript.text, CScript.name);

        InputActionMap map = _playerInput.actions.FindActionMap("Interface");
        _confirmAction = map.FindAction("Confirm");
        _cancelAction = map.FindAction("Cancel");
    }

    private void OnEnable()
    {
        _confirmAction.performed += _advanceAction_performed;
        _cancelAction.performed += _skipAction_performed;

        CSInterpreter.DialogueEnded += OnDialogueEnded;
        InventoryController.ItemDropped += InventorySystem_ItemDropped;
    }

    private void OnDisable()
    {
        _confirmAction.performed -= _advanceAction_performed;
        _cancelAction.performed -= _skipAction_performed;

        CSInterpreter.DialogueEnded -= OnDialogueEnded;
        InventoryController.ItemDropped -= InventorySystem_ItemDropped;
    }

    public void OpenInterface()
    {
        _playerInput.SwitchCurrentActionMap("Interface");

        RuleListText.text = string.Empty;
        for(int i = 0; i < InsertedRules.Count; ++i)
        {
            ItemData item = InsertedRules[i];
            RuleListText.text += $"({i + 1}) {item.name}\n";
        }

        InterfacePanel.SetActive(true);
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        CloseInterface();

        if (InsertedRules.Count < 3)
        {
            DialogueSystem.Instance.PlayDialogue("missing", _scriptInstance);
        }
        else
        {
            DialogueSystem.Instance.PlayDialogue("theendinterface", _scriptInstance);
        }
    }

    private void _skipAction_performed(InputAction.CallbackContext obj)
    {
        CloseInterface();
    }

    private void OnDialogueEnded(string dialogueId)
    {
        switch(dialogueId)
        {
            case "abandonending":
                SceneManager.LoadScene("MainMenuScene");
                break;

            case "theendinterface":
                SceneManager.LoadScene("EndingScene");
                break;
        }
    }

    private void InventorySystem_ItemDropped(ItemData item, Inventory inv)
    {
        if (!item.name.Contains("Rule")) return;

        AbandonEnding = true;
    }

    public void CloseInterface()
    {
        InterfacePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
    }
}
