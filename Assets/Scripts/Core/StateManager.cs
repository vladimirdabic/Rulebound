using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VD.Rulebound.CS;

public class StateManager
{
    public static readonly string SavePath = Path.Combine(Application.persistentDataPath, "data.json");
    public static DataFile Data = null;

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            Reset();
            return;
        }

        string txt = File.ReadAllText(SavePath);
        Data = JsonUtility.FromJson<DataFile>(txt);
        
        foreach(Flag flag in Data.Flags)
            flag.Saved = true;

        CSInterpreter.GlobalFlags.AddRange(Data.Flags);
    }

    public static void Save()
    {
        Data.Flags = CSInterpreter.GlobalFlags.Where(flag => flag.Saved).ToArray();
        string txt = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, txt);
    }

    public static void Reset()
    {
        // Default data
        Data = new DataFile();
        CSInterpreter.GlobalFlags?.Clear();
        DialogueSystem.Instance.ForceEndDialogue();
    }

    public static void Quit()
    {
        Save();

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    [Serializable]
    public class DataFile
    {
        public Flag[] Flags;
    }
}
