using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONDialogueFile
{
    public JSONDialogueFlag[] flags;
    public JSONDialogue[] dialogue;

    [NonSerialized] public List<DialogueFlag> Flags;
    [NonSerialized] public bool Initialized;

    public void Init()
    {
        if (Initialized) return;

        Initialized = true;
        Flags = new List<DialogueFlag>();

        if(flags != null)
            foreach(JSONDialogueFlag flag in flags)
            {
                DialogueFlag diagFlag = new DialogueFlag(flag.name, flag.value);

                if (flag.global)
                {
                    if(JSONDialogueSystem.Instance.GetGlobalFlag(flag.name) == null)
                        JSONDialogueSystem.Instance.GlobalFlags.Add(diagFlag);
                }
                else
                {
                    Flags.Add(diagFlag);
                }
            }

        foreach(JSONDialogue diag in dialogue)
            diag.Init();
    }

    public JSONDialogue GetDialogue(string id)
    {
        foreach(JSONDialogue diag in dialogue)
            if(diag.id == id) return diag;

        return null;
    }

    public DialogueFlag GetFlag(string name)
    {
        return Flags.Find(flag => flag.Name == name);
    }
}

[Serializable]
public class JSONDialogue
{
    public string id;
    public string portrait;
    public JSONDialogueLine[] lines;
    public string choiceText;
    public JSONDialogueChoice[] choices;

    [NonSerialized] public Sprite MainPortrait;

    public void Init()
    {
        MainPortrait = Resources.Load<Sprite>(portrait);

        foreach (JSONDialogueLine line in lines)
            line.Init();
    }
}

[Serializable]
public class JSONDialogueFlag
{
    public string name;
    public bool global;
    public bool value;
}

[Serializable]
public class JSONDialogueLine
{
    // Normal line
    public string portrait;
    public string text;
    public float secondsBefore;

    // Condition
    public string condition;
    public string next;

    // Flags
    public string set;
    public string unset;

    [NonSerialized] public Sprite Portrait;

    public void Init()
    {
        Portrait = Resources.Load<Sprite>($"Sprites/{portrait}");
    }
}

[Serializable]
public class JSONDialogueChoice
{
    public string text;
    public string next;
}

[Serializable]
public class DialogueFlag
{
    public string Name;
    public bool Value;

    public DialogueFlag(string name, bool value)
    {
        this.Name = name;
        this.Value = value;
    }
}