using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SwitchSceneOnConfirm : MonoBehaviour
{
    public string SceneName;
    public PlayerInput Input;

    private InputAction _confirmAction;

    private void Awake()
    {
        InputActionMap map = Input.actions.FindActionMap("Menu");
        _confirmAction = map.FindAction("Confirm");
    }

    private void OnEnable()
    {
        _confirmAction.performed += _confirmAction_performed;
    }

    private void OnDisable()
    {
        _confirmAction.performed -= _confirmAction_performed;
    }

    private void _confirmAction_performed(InputAction.CallbackContext obj)
    {
        SceneManager.LoadScene(SceneName);
    }

    private void Start()
    {
        Input.SwitchCurrentActionMap("Menu");
    }
}
