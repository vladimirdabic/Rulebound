using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("UI References")]
    public GameObject InventoryPanel;
    public TMP_Text ItemListText;
    public TMP_Text UseText;
    public TMP_Text DropText;
    public Image Selector;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Settings")]
    [SerializeField] private float _selectorOffset;
    [SerializeField] private float _selectorItemJump;

    private InputAction _selectAction;
    private InputAction _cancelAction;
    private readonly InputAction[] _moveActions = new InputAction[4];
    private InventoryState _state;
    private Inventory _inventory;
    private int _selectedItemIdx;
    private Action _selectedAction;

    private Dialogue _droppedItemDiag;

    public static event Action<ItemData, Inventory> ItemUsed;
    public static event Action<ItemData, Inventory> ItemDropped;

    private enum InventoryState
    {
        SELECTING, ACTION
    }

    private enum Action
    {
        USE, DROP, INFO
    }

    public void OpenInventory(Inventory inventory)
    {
        _state = InventoryState.SELECTING;
        _inventory = inventory;

        _playerInput.SwitchCurrentActionMap("Inventory");
        InventoryPanel.SetActive(true);

        _selectedItemIdx = 0;
        ItemListText.text = string.Empty;
        Selector.gameObject.SetActive(inventory.Items.Count > 0);
        MoveSelector(ItemListText.transform.position);
        RedrawItemList();
    }

    public void CloseInventory()
    {
        if (!InventoryPanel.activeInHierarchy) return;

        InventoryPanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        InputActionMap map = _playerInput.actions.FindActionMap("Inventory");
        _selectAction = map.FindAction("Select");
        _cancelAction = map.FindAction("Cancel");

        _moveActions[0] = map.FindAction("Up");
        _moveActions[1] = map.FindAction("Down");
        _moveActions[2] = map.FindAction("Left");
        _moveActions[3] = map.FindAction("Right");

        // Hacky, but works
        _droppedItemDiag = ScriptableObject.CreateInstance<Dialogue>();

        DialogueLine line = new DialogueLine();
        line.IsBranching = false;

        _droppedItemDiag.Lines = new DialogueLine[] { line };
    }

    private void Start()
    {
        DialogueSystem.OnDialogueEnded += DiagFinished;
    }

    private void OnEnable()
    {
        _selectAction.performed += _selectAction_performed;
        _cancelAction.performed += _cancelAction_performed;

        foreach (InputAction action in _moveActions)
            action.performed += _moveAction_performed;
    }

    private void OnDisable()
    {
        _selectAction.performed -= _selectAction_performed;
        _cancelAction.performed -= _cancelAction_performed;

        foreach (InputAction action in _moveActions)
            action.performed -= _moveAction_performed;
    }

    private void OnDestroy()
    {
        DialogueSystem.OnDialogueEnded -= DiagFinished;
    }

    private void _moveAction_performed(InputAction.CallbackContext obj)
    {
        switch(_state)
        {
            case InventoryState.SELECTING:
                HandleSelecting(obj.action.name);
                break;

            case InventoryState.ACTION:
                HandleActionSelecting(obj.action.name);
                break;
        }
    }

    private void HandleSelecting(string direction)
    {
        switch(direction)
        {
            case "Up":
                _selectedItemIdx = Mathf.Max(--_selectedItemIdx, 0);
                break;

            case "Down":
                _selectedItemIdx = Mathf.Min(++_selectedItemIdx, _inventory.Items.Count - 1);
                break;
        }

        MoveSelectorToItem();
    }

    private void HandleActionSelecting(string direction)
    {
        switch (direction)
        {
            case "Right":
                MoveSelector(DropText.transform.position);
                _selectedAction = Action.DROP;
                break;

            case "Left":
                MoveSelector(UseText.transform.position);
                _selectedAction = Action.USE;
                break;
        }
    }

    private void _selectAction_performed(InputAction.CallbackContext obj)
    {
        if (_inventory.Items.Count == 0) return;
        
        switch(_state)
        {
            case InventoryState.SELECTING:
                MoveSelector(UseText.transform.position);
                _state = InventoryState.ACTION;
                _selectedAction = Action.USE;
                break;

            case InventoryState.ACTION:
                switch (_selectedAction)
                {
                    case Action.USE:
                        ItemUsed?.Invoke(_inventory.Items[_selectedItemIdx], _inventory);
                        break;

                    case Action.DROP:
                        ItemData item = _inventory.Items[_selectedItemIdx];
                        _inventory.Items.RemoveAt(_selectedItemIdx);
                        ItemDropped?.Invoke(item, _inventory);

                        _droppedItemDiag.Lines[0].Message = $"* Dropped {item.Name}";
                        InventoryPanel.SetActive(false);
                        DialogueSystem.Instance.PlayDialogue(_droppedItemDiag);

                        break;
                }

                _state = InventoryState.SELECTING;
                break;
        }
    }

    private void _cancelAction_performed(InputAction.CallbackContext obj)
    {
        switch (_state)
        {
            case InventoryState.SELECTING:
                CloseInventory();
                break;

            case InventoryState.ACTION:
                _state = InventoryState.SELECTING;
                MoveSelectorToItem();
                break;
        }
    }

    private void DiagFinished(Dialogue diag)
    {
        if (diag != _droppedItemDiag) return;

        OpenInventory(_inventory);
    }

    private void MoveSelector(Vector3 position)
    {
        Selector.transform.position = position - new Vector3(_selectorOffset, 14.8f); // Probably not performant code but whatever...
    }

    private void MoveSelectorToItem()
    {
        MoveSelector(ItemListText.transform.position - new Vector3(0, _selectorItemJump * _selectedItemIdx));
    }

    private void RedrawItemList()
    {
        ItemListText.text = string.Join('\n', _inventory.Items.Select(i => i.Name));
    }
}
