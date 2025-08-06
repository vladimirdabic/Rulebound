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
}
