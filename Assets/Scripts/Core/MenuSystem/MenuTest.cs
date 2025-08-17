using UnityEngine;

public class MenuTest : MonoBehaviour
{
    private bool _opened = false;

    private void OnEnable()
    {
        MenuManager.ButtonPressed += ButtonPressed;
    }

    private void OnDisable()
    {
        MenuManager.ButtonPressed -= ButtonPressed;    
    }

    private void ButtonPressed(MenuButton btn, Menu menu)
    {
        if (menu.ID != "test") return;

        if (btn.ID == "testbtn")
            MenuManager.Instance.SwitchSelectionGroup("group2");
    }

    private void Update()
    {
        if (_opened) return;
        MenuManager.Instance.OpenMenu("test");
        _opened = true;
    }
}
