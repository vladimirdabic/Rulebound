using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuleInterfaceSystem : MonoBehaviour
{
    public static RuleInterfaceSystem Instance;

    [Header("UI References")]
    public GameObject InterfacePanel;
    public TMP_Text RuleListText;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Dialogue References")]
    [SerializeField] private TextAsset Dialogue;

    public List<ItemData> InsertedRules;
    private InputAction _confirmAction;
    private InputAction _cancelAction;
    private JSONDialogueFile _dialogueFile;

    private static readonly string[] _allowedRules = new string[]
    {
        "Rule Of Compassion",
        "Rule Of Order",
        "Rule Of Self"
    };

    public bool AddRule(ItemData rule)
    {
        if (!_allowedRules.Contains(rule.Name)) return false;
        if (InsertedRules.Any(item => item.Name == rule.name)) return false;

        InsertedRules.Add(rule);
        return true;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _dialogueFile = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);

        InputActionMap map = _playerInput.actions.FindActionMap("Interface");
        _confirmAction = map.FindAction("Confirm");
        _cancelAction = map.FindAction("Cancel");
    }

    private void OnEnable()
    {
        _confirmAction.performed += _advanceAction_performed;
        _cancelAction.performed += _skipAction_performed;
    }

    private void OnDisable()
    {
        _confirmAction.performed -= _advanceAction_performed;
        _cancelAction.performed -= _skipAction_performed;
    }

    public void OpenInterface()
    {
        _playerInput.SwitchCurrentActionMap("Interface");

        RuleListText.text = string.Empty;
        for(int i = 0; i < InsertedRules.Count; ++i)
        {
            ItemData item = InsertedRules[i];
            RuleListText.text += $"({i + 1}) {item.Name}\n";
        }

        InterfacePanel.SetActive(true);
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        if(InsertedRules.Count < 3)
        {
            CloseInterface();
            JSONDialogueSystem.Instance.PlayDialogue(_dialogueFile, "missing");
        }
        else
        {
            // TODO: Goto endgame screen
        }
    }

    private void _skipAction_performed(InputAction.CallbackContext obj)
    {
        CloseInterface();
    }

    public void CloseInterface()
    {
        InterfacePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
    }
}
