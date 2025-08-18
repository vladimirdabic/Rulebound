using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    public TextAsset ItemDataJSON;

    private ItemData[] _itemData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _itemData = JsonUtility.FromJson<ItemDef>(ItemDataJSON.text).items;
    }

    public ItemData GetItemByID(string id)
    {
        return _itemData.FirstOrDefault(i => i.id == id);
    }

    public ItemData GetItemByName(string name)
    {
        return _itemData.FirstOrDefault(i => i.name == name);
    }
}

[Serializable]
public class ItemData
{
    public string id;
    public string name;
    public string description;
    public bool undroppable;

    [NonSerialized] private Dictionary<string, object> _userData;

    public T GetUserData<T>(string key)
    {
        Init();
        object obj = _userData.GetValueOrDefault(key);
        return obj == null ? default : (T)obj;
    }

    public void SetUserData<T>(string key, T data)
    {
        Init();
        _userData[key] = data;
    }

    private void Init()
    {
        _userData ??= new Dictionary<string, object>();
    }
}

[Serializable]
public class ItemDef
{
    public ItemData[] items;
}