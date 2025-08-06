using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [Header("Configuration")]
    public Sprite MainPortrait;
    public DialogueLine[] Lines;

    [Header("Events")]
    public UnityAction Finished;
    public UnityAction Started;
}

[Serializable]
public class DialogueLine
{
    [Header("Configuration")]
    [TextArea(1, 4)] public string Message;
    public Sprite Portrait;
    public bool IsBranching;
    public DialogueChoice[] Choices;
    // TODO: Voice sound

    [Header("Events")]
    public UnityAction Finished;
    public UnityAction Started;
}

[Serializable]
public class DialogueChoice
{
    public string Choice;
    public Dialogue NextDialogue;
}
