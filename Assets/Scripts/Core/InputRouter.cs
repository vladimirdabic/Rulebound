using UnityEngine;
using UnityEngine.InputSystem;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }
    public PlayerInput Input { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        Input = GetComponent<PlayerInput>();
    }

}