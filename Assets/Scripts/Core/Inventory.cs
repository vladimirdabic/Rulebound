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

    public bool HasItem(string id)
    {
        return Items.Any(i => i.id == id);
    }

    public bool AddItem(ItemData item, bool alreadyHasCheck)
    {
        if(alreadyHasCheck && HasItem(item.id)) return false;

        Items.Add(item);
        return true;
    }

    public bool RemoveItem(string id)
    {
        if (!HasItem(id)) return false;

        Items.RemoveAll(i => i.id == id);
        return true;
    }
}
