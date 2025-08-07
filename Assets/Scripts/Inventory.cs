using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemData> Items;

    private void Awake()
    {
        Items = new List<ItemData>();
    }

    public bool HasItem(string name)
    {
        return Items.Any(i => i.Name == name);
    }

    public bool AddItem(ItemData item)
    {
        if(HasItem(item.Name)) return false;

        Items.Add(item);
        return true;
    }
}
