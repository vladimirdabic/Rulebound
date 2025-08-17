using System.Collections.Generic;
using System;
using UnityEngine;


public class MenuButton : MonoBehaviour
{
    public int SelectionOrder;
    public string ID;
    //[NonSerialized] public string SelectionGroupID;
    
    private object _userData;

    public T GetUserData<T>()
    {
        return (T)_userData;
    }

    public void SetUserData<T>(T data)
    {
        _userData = data;
    }
}
