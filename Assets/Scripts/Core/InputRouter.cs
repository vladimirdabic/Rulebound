using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }
    public PlayerInput Input { get; private set; }

    private InputAction _quitAction;
    private InputAction _fullscreenAction;
    private InputAction _resetDataAction;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        Input = GetComponent<PlayerInput>();

        InputActionMap map = Input.actions.FindActionMap("Global");

        _quitAction = map.FindAction("Quit");
        _fullscreenAction = map.FindAction("Fullscreen");
        _resetDataAction = map.FindAction("ResetData");

        map.Enable();
    }

    private void OnEnable()
    {
        _quitAction.performed += QuitPerformed;
        _fullscreenAction.performed += FullscreenPerformed;
        _resetDataAction.performed += ResetDataPerformed;
    }

    private void OnDisable()
    {
        _quitAction.performed -= QuitPerformed;
        _fullscreenAction.performed -= FullscreenPerformed;
        _resetDataAction.performed -= ResetDataPerformed;
    }

    private void FullscreenPerformed(InputAction.CallbackContext obj)
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = !Screen.fullScreen;
    }

    private void QuitPerformed(InputAction.CallbackContext obj)
    {
        StateManager.Quit();
    }

    private void ResetDataPerformed(InputAction.CallbackContext obj)
    {
        StateManager.Reset();
        StateManager.Save();
        SceneManager.LoadScene("LoaderScene");
    }
}