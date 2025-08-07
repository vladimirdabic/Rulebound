using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [Header("Configuration")]
    public Sprite MainPortrait;
    public DialogueLine[] Lines;
}

[Serializable]
public class DialogueLine
{
    [Header("Configuration")]
    [TextArea(1, 4)] public string Message;
    public Sprite Portrait;
    public bool IsBranching;
    public DialogueChoice[] Choices;

    [Header("Delays")]
    public float SecondsBefore;
    // TODO: Voice sound
}

[Serializable]
public class DialogueChoice
{
    public string Choice;
    public Dialogue NextDialogue;
}
