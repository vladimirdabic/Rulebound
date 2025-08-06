using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class InputController : MonoBehaviour
{
    [Header("Player")]
    public float Speed;

    private InputAction _moveAction;
    private InputAction _interactAction;

    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private Animator _animator;

    private Vector3 _inputVector;
    private IInteractable _target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = _rb.GetComponent<PlayerInput>();
        _animator = _rb.GetComponent<Animator>();

        InputActionMap map = _playerInput.actions.FindActionMap("Player");
        _moveAction = map.FindAction("Move");
        _interactAction = map.FindAction("Interact");
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
    }

    private void UnsubEvents()
    {
        _interactAction.performed -= _interactAction_performed;
        _moveAction.canceled -= _moveAction_canceled;
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
        if (other.TryGetComponent<IInteractable>(out var interactable))
            _target = interactable;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var _))
            _target = null;
    }
}
