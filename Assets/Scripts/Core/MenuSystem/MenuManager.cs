using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public delegate void ButtonPressedDelegate(MenuButton button, Menu menu);

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    //[Header("Input References")]
    //[SerializeField] private PlayerInput _playerInput;

    [Header("UI References")]
    public Image Selector;
    [SerializeField] private float _selectorOffset;

    public static event Action<Menu> MenuOpened;
    public static event Action<Menu> MenuClosed;
    public static event ButtonPressedDelegate ButtonPressed;

    private Dictionary<string, Menu> _menus;
    private PlayerInput _playerInput;
    private InputAction _confirmAction;
    private InputAction _cancelAction;
    private readonly InputAction[] _moveActions = new InputAction[4];

    // State stuff
    private int _currentButtonIdx;
    private MenuButton _currentButton;
    private Menu _currentMenu;
    private SelectionGroup _currentGroup;
    private Stack<SelectionContext> _selectionStack; 

    private class SelectionContext
    {
        public MenuButton CurrentButton;
        public int CurrentButtonIdx;
        public SelectionGroup CurrentGroup;
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _menus = new Dictionary<string, Menu>();
        _selectionStack = new Stack<SelectionContext>();
        _playerInput = InputRouter.Instance.Input;

        InputActionMap map = _playerInput.actions.FindActionMap("Menu");
        _confirmAction = map.FindAction("Confirm");
        _cancelAction = map.FindAction("Cancel");
        _moveActions[0] = map.FindAction("Up");
        _moveActions[1] = map.FindAction("Down");
        _moveActions[2] = map.FindAction("Left");
        _moveActions[3] = map.FindAction("Right");
    }

    private void OnEnable()
    {
        _confirmAction.performed += confirmPerformed;
        _cancelAction.performed += cancelPerformed;

        foreach (InputAction action in _moveActions)
            action.performed += movePerformed;
    }

    private void OnDisable()
    {
        _confirmAction.performed -= confirmPerformed;
        _cancelAction.performed -= cancelPerformed;

        foreach (InputAction action in _moveActions)
            action.performed -= movePerformed;
    }

    private void confirmPerformed(InputAction.CallbackContext ctx)
    {
        if (_currentButton == null) return;
        ButtonPressed?.Invoke(_currentButton, _currentMenu);
    }

    private void cancelPerformed(InputAction.CallbackContext ctx)
    {
        if (_selectionStack.Count == 0)
        {
            CloseMenu();
            return;
        }

        PreviousControlGroup();
    }

    private void movePerformed(InputAction.CallbackContext ctx)
    {
        switch(ctx.action.name)
        {
            case "Up":
                if (_currentGroup.Type != SelectionGroup.Direction.VERTICAL) return;
                PrevButton();
                break;

            case "Down":
                if (_currentGroup.Type != SelectionGroup.Direction.VERTICAL) return;
                NextButton();
                break;

            case "Left":
                if (_currentGroup.Type != SelectionGroup.Direction.HORIZONTAL) return;
                PrevButton();
                break;

            case "Right":
                if (_currentGroup.Type != SelectionGroup.Direction.HORIZONTAL) return;
                NextButton();
                break;
        }
    }

    private void NextButton()
    {
        if (_currentButtonIdx >= _currentGroup.GetMaxSelectionIndex()) return;

        _currentButtonIdx++;
        _currentButton = _currentGroup.GetButton(_currentButtonIdx);

        MoveSelectorButton(_currentButton);
    }

    private void PrevButton()
    {
        if (_currentButtonIdx <= 0) return;

        _currentButtonIdx--;
        _currentButton = _currentGroup.GetButton(_currentButtonIdx);

        MoveSelectorButton(_currentButton);
    }

    public void RegisterMenu(Menu menu)
    {
        _menus[menu.ID] = menu;
    }

    public void UnregisterMenu(Menu menu)
    {
        if (!_menus.ContainsKey(menu.ID)) return;
        _menus.Remove(menu.ID);
    }

    public void OpenMenu(string menuId)
    {
        if (!_menus.ContainsKey(menuId)) return;

        _currentMenu = _menus[menuId];
        foreach(Transform child in _currentMenu.gameObject.transform)
            child.gameObject.SetActive(true);

        MenuOpened?.Invoke(_currentMenu);
        _playerInput.SwitchCurrentActionMap("Menu");

        _currentGroup = _currentMenu.GetSelectionGroup(_currentMenu.DefaultSelectionGroup);
        if (_currentGroup.Buttons.Count == 0)
        {
            _currentButton = null;
            return;
        }

        Selector.gameObject.SetActive(true);
        _currentButtonIdx = 0;
        _currentButton = _currentGroup.GetButton(0);
        MoveSelectorButton(_currentButton);
    }

    public void CloseMenu()
    {
        foreach (Transform child in _currentMenu.gameObject.transform)
            child.gameObject.SetActive(false);

        Selector.gameObject.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        MenuClosed?.Invoke(_currentMenu);
        _currentMenu = null;
        _selectionStack.Clear();
    }


    public void SwitchSelectionGroup(string groupId)
    {
        var ctx = new SelectionContext()
        {
            CurrentGroup = _currentGroup,
            CurrentButton = _currentButton,
            CurrentButtonIdx = _currentButtonIdx
        };

        _selectionStack.Push(ctx);

        _currentGroup = _currentMenu.GetSelectionGroup(groupId);
        _currentButtonIdx = 0;
        _currentButton = _currentGroup.GetButton(0);
        MoveSelectorButton(_currentButton);
    }

    public void PreviousControlGroup()
    {
        if (_selectionStack.Count == 0) return;

        var prev = _selectionStack.Pop();

        _currentGroup = prev.CurrentGroup;
        if (_currentGroup.Buttons.Count == 0)
        {
            _currentButton = null;
            Selector.gameObject.SetActive(false);
            return;
        }

        _currentButton = prev.CurrentButton;
        _currentButtonIdx = prev.CurrentButtonIdx;

        MoveSelectorButton(_currentButton);
    }

    public Menu GetMenu(string menuId)
    {
        return _menus.ContainsKey(menuId) ? _menus[menuId] : null;
    }

    private void MoveSelectorButton(MenuButton btn)
    {
        MoveSelector(btn.transform.position);
    }

    private void MoveSelector(Vector3 position)
    {
        Selector.transform.position = position - new Vector3(_selectorOffset, 14.8f);
    }
}
