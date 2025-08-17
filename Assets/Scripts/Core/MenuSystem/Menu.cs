using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Menu : MonoBehaviour
{
    public string ID;
    public string DefaultSelectionGroup;
    public List<SelectionGroup> SelectionGroups;

    public SelectionGroup GetSelectionGroup(string id)
    {
        return SelectionGroups.FirstOrDefault(group => group.ID == id);
    }

    private void Start()
    {
        MenuManager.Instance.RegisterMenu(this);
    }
}

[Serializable]
public class SelectionGroup
{
    public string ID;
    public Direction Type;

    public enum Direction
    {
        VERTICAL, HORIZONTAL
    }
    
    public List<MenuButton> Buttons;

    public MenuButton GetButton(int selectionOrder)
    {
        return Buttons.FirstOrDefault(btn => btn.SelectionOrder == selectionOrder);
    }

    public int GetMaxSelectionIndex()
    {
        if(Buttons.Count == 0) return 0;
        return Buttons.Max(btn => btn.SelectionOrder);
    }
}
