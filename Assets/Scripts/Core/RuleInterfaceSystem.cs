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

    public List<ItemData> InsertedRules;
    private InputAction _advanceAction;
    private InputAction _skipAction;

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

        InputActionMap map = _playerInput.actions.FindActionMap("Dialogue");
        _advanceAction = map.FindAction("Advance");
        _skipAction = map.FindAction("Skip");
    }

    private void OnEnable()
    {
        _advanceAction.performed += _advanceAction_performed;
        _skipAction.performed += _skipAction_performed;
    }

    private void OnDisable()
    {
        _advanceAction.performed -= _advanceAction_performed;
        _skipAction.performed -= _skipAction_performed;
    }

    public void OpenInterface()
    {
        _playerInput.SwitchCurrentActionMap("Dialogue");

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
        // TODO
        throw new System.NotImplementedException();
    }

    private void _skipAction_performed(InputAction.CallbackContext obj)
    {
        InterfacePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
    }
}
