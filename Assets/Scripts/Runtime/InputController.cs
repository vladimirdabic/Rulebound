using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VD.Rulebound.CS;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Inventory))]
public class InputController : MonoBehaviour
{
    [Header("Player")]
    public float Speed;

    [Header("CS References")]
    public TextAsset CScript;

    public IInteractable Target { get; private set; }

    private InputAction _moveAction;
    private InputAction _interactAction;
    private InputAction _openInvAction;

    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private Animator _animator;

    private Inventory _inventory;
    private CharacterScript _cScript;

    private Vector3 _inputVector;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = _rb.GetComponent<PlayerInput>();
        _animator = _rb.GetComponent<Animator>();
        _inventory = GetComponent<Inventory>();

        _cScript = CharacterScript.FromText(CScript.text, CScript.name);

        InputActionMap map = _playerInput.actions.FindActionMap("Player");
        _moveAction = map.FindAction("Move");
        _interactAction = map.FindAction("Interact");
        _openInvAction = map.FindAction("OpenInv");
    }

    private void OnEnable()
    {
        _interactAction.performed += _interactAction_performed;
        _moveAction.canceled += _moveAction_canceled;
        _openInvAction.performed += _openInvAction_performed;

        InventoryController.ItemUsed += InventorySystem_ItemUsed;

        CSInterpreter.GiveItem += CSInterpreter_GiveItem;
        CSInterpreter.TakeItem += CSInterpreter_TakeItem;
        CSInterpreter.HasItemFunc = CSInterpreter_HasItem;
    }

    private void OnDisable()
    {
        _interactAction.performed -= _interactAction_performed;
        _moveAction.canceled -= _moveAction_canceled;
        _openInvAction.performed -= _openInvAction_performed;

        InventoryController.ItemUsed -= InventorySystem_ItemUsed;

        CSInterpreter.GiveItem -= CSInterpreter_GiveItem;
        CSInterpreter.TakeItem -= CSInterpreter_TakeItem;
        CSInterpreter.HasItemFunc = null;
    }

    private void _moveAction_canceled(InputAction.CallbackContext obj)
    {
        _animator.SetFloat("LX", _inputVector.x);
        _animator.SetFloat("LY", _inputVector.y);
    }

    private void _interactAction_performed(InputAction.CallbackContext obj)
    {
        Target?.OnInteract();
    }

    private void _openInvAction_performed(InputAction.CallbackContext obj)
    {
        //InventorySystem.Instance.OpenInventory(_inventory);
        GetComponent<InventoryController>().OpenInventory(_inventory);
    }

    private void FixedUpdate()
    {
        _inputVector = _moveAction.ReadValue<Vector2>();

        // Multiplying by fixed dt is for consistency with units, and that speed remains the same if fdt is changed.
        _rb.MovePosition(transform.position + Speed * Time.fixedDeltaTime * _inputVector);

        _animator.SetFloat("InX", _inputVector.x);
        _animator.SetFloat("InY", _inputVector.y);
        _animator.SetBool("IsWalking", _moveAction.inProgress);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable[] interactables = other.GetComponents<IInteractable>();
        IInteractable found = interactables.FirstOrDefault(i => i is MonoBehaviour behaviour && behaviour.enabled);
        Target = found;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var _))
            Target = null;
    }

    private void InventorySystem_ItemUsed(ItemData item, Inventory inv)
    {
        if (inv != _inventory) return;

        if (Target == null && item.id.Contains("rule"))
        {
            DialogueSystem.Instance.PlayDialogue("onlyatinterface", _cScript);
            return;
        }

        if(Target is RuleInterfaceEntity ruleInterface)
        {
            if (ruleInterface.AddRule(item)) _inventory.Items.Remove(item);
        }
        else if(Target is RewriterEntity rewriter)
        {
            if (rewriter.InsertItem(item)) _inventory.Items.Remove(item);
        }
    }

    private void CSInterpreter_GiveItem(string itemId, bool once)
    {
        ItemData item = ItemManager.Instance.GetItemByID(itemId);
        if (!_inventory.AddItem(item, once)) return;

        DialogueStmt[] lines = _cScript.GetDialogue("acquireditem").Statements;
        var l1 = (DialogueStmt.Line)lines[0];
        var l2 = (DialogueStmt.Line)lines[1];

        l1.Text = $"* You have acquired:\n  {item.name}";
        l2.Text = $"* {item.description}";

        DialogueSystem.Instance.PlayDialogue("acquireditem", _cScript);
    }

    private void CSInterpreter_TakeItem(string itemId, bool once)
    {
        _inventory.RemoveItem(itemId);
    }

    private bool CSInterpreter_HasItem(string itemId)
    {
        return _inventory.HasItem(itemId);
    }
}
