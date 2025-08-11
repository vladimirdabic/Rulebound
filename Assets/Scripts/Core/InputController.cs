using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Inventory))]
public class InputController : MonoBehaviour
{
    [Header("Player")]
    public float Speed;

    [Header("Dialogue References")]
    public TextAsset Dialogue;

    private InputAction _moveAction;
    private InputAction _interactAction;
    private InputAction _openInvAction;

    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private Animator _animator;

    private Inventory _inventory;
    private JSONDialogueFile _dialogueFile;

    private Vector3 _inputVector;
    private IInteractable _target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = _rb.GetComponent<PlayerInput>();
        _animator = _rb.GetComponent<Animator>();
        _inventory = GetComponent<Inventory>();

        _dialogueFile = JsonUtility.FromJson<JSONDialogueFile>(Dialogue.text);

        InputActionMap map = _playerInput.actions.FindActionMap("Player");
        _moveAction = map.FindAction("Move");
        _interactAction = map.FindAction("Interact");
        _openInvAction = map.FindAction("OpenInv");
    }

    private void OnEnable()
    {
        SubEvents();
    }

    private void OnDisable()
    {
        UnsubEvents();
    }

    private void SubEvents()
    {
        _interactAction.performed += _interactAction_performed;
        _moveAction.canceled += _moveAction_canceled;
        _openInvAction.performed += _openInvAction_performed;

        InventorySystem.ItemUsed += InventorySystem_ItemUsed;
    }

    private void UnsubEvents()
    {
        _interactAction.performed -= _interactAction_performed;
        _moveAction.canceled -= _moveAction_canceled;
        _openInvAction.performed -= _openInvAction_performed;

        InventorySystem.ItemUsed -= InventorySystem_ItemUsed;
    }

    private void _moveAction_canceled(InputAction.CallbackContext obj)
    {
        _animator.SetFloat("LX", _inputVector.x);
        _animator.SetFloat("LY", _inputVector.y);
    }

    private void _interactAction_performed(InputAction.CallbackContext obj)
    {
        _target?.OnInteract();
    }

    private void _openInvAction_performed(InputAction.CallbackContext obj)
    {
        InventorySystem.Instance.OpenInventory(_inventory);
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
        _target = found;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var _))
            _target = null;
    }

    private void InventorySystem_ItemUsed(ItemData item, Inventory inv)
    {
        if (inv != _inventory) return;
        if (_target == null || _target.GetType() != typeof(RuleInterfaceEntity))
        {
            if (item.Name.Contains("Rule"))
                JSONDialogueSystem.Instance.PlayDialogue(_dialogueFile, "onlyatinterface");
            return;
        }

        if(RuleInterfaceSystem.Instance.AddRule(item))
        {
            _inventory.Items.Remove(item);
        }
    }
}
