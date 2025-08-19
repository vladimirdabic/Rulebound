using System;
using System.Collections.Generic;
using UnityEngine;
using VD.Rulebound.CS;

public class Character : MonoBehaviour
{
    public TextAsset Script;
    public List<Flag> Flags;
    
    [NonSerialized] public CharacterScript ScriptInstance;

    private void Awake()
    {
        ScriptInstance = CharacterScript.FromText(Script.text, Script.name);
        Flags = ScriptInstance.Flags;
    }

    private void OnEnable()
    {
        CSInterpreter.DialogueCallback += Callback;
    }

    private void OnDisable()
    {
        CSInterpreter.DialogueCallback -= Callback;
    }

    private void Callback(CharacterScript script, string callbackId)
    {
        if (script != ScriptInstance) return;

        switch(callbackId)
        {
            case "destroy":
                Destroy(gameObject);
                break;

            case "disable":
                gameObject.SetActive(false);
                break;
        }
    }
}
