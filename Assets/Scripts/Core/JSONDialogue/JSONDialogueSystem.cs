using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JSONDialogueSystem : MonoBehaviour
{
    public static JSONDialogueSystem Instance;

    [Header("Dialog References")]
    public GameObject DialoguePanel;
    public TMP_Text MessageText;
    public Image PortraitImage;

    [Header("Choice References")]
    public GameObject ChoicePanel;
    public TMP_Text ChoiceText;
    public TMP_Text Choices;
    public Image Selector;
    [SerializeField] private float _selectorOffset;
    [SerializeField] private float _selectorItemJump;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Other")]
    public List<DialogueFlag> GlobalFlags;

    public static event Action<JSONDialogue> OnDialogueStarted;
    public static event Action<JSONDialogue> OnDialogueEnded;
    public static event Action<JSONDialogueFile> OnDialogueFileStarted;
    public static event Action<JSONDialogueFile> OnDialogueFileEnded;
    public static event Action<JSONDialogueLine> OnLineStarted;
    public static event Action<JSONDialogueLine> OnLineEnded;

    private IEnumerator _lineEnumerator;
    private JSONDialogueFile _currentDialogFile;
    private JSONDialogue _currentDialog;
    private JSONDialogueLine _currentLine;
    private int _choiceIdx;
    
    private InputAction _advanceAction;
    private InputAction _upAction;
    private InputAction _downAction;
    private InputAction _skipAction;
    private State _state;

    private enum State
    {
        WAITING, WRITING, CHOICE
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // Maybe DontDestroyOnLoad would be good here 

        InputActionMap map = _playerInput.actions.FindActionMap("Dialogue");
        _advanceAction = map.FindAction("Advance");
        _upAction = map.FindAction("Up");
        _downAction = map.FindAction("Down");
        _skipAction = map.FindAction("Skip");
    }

    private void OnEnable()
    {
        _advanceAction.performed += _advanceAction_performed;
        _upAction.performed += _choiceActionPeformed;
        _downAction.performed += _choiceActionPeformed;
        _skipAction.performed += _skipAction_performed;
    }

    private void OnDisable()
    {
        _advanceAction.performed -= _advanceAction_performed;
        _upAction.performed -= _choiceActionPeformed;
        _downAction.performed -= _choiceActionPeformed;
        _skipAction.performed -= _skipAction_performed;
    }

    public void PlayDialogue(JSONDialogueFile diag, string id = "main")
    {
        if (diag.dialogue == null || diag.dialogue.Length == 0) return;

        diag.Init();
        _currentDialogFile = diag;
        _currentDialog = diag.GetDialogue(id);
        
        OnDialogueFileStarted?.Invoke(_currentDialogFile);
        OnDialogueStarted?.Invoke(_currentDialog);

        _playerInput.SwitchCurrentActionMap("Dialogue");

        _lineEnumerator = _currentDialog.lines.GetEnumerator();
        PlayNextLine();
    }

    private void PlayDialogue(JSONDialogue diag)
    {
        _currentDialog = diag;
        OnDialogueStarted?.Invoke(_currentDialog);

        _playerInput.SwitchCurrentActionMap("Dialogue");

        _lineEnumerator = _currentDialog.lines.GetEnumerator();
        PlayNextLine();
    }

    private IEnumerator PlayLine(JSONDialogueLine line)
    {
        if (line.secondsBefore > 0)
        {
            DialoguePanel.SetActive(false);
            yield return new WaitForSeconds(line.secondsBefore);
        }

        _currentLine = line;
        
        // I don't really like how big this function is
        // because of these special instructions...
        if(line.condition != null)
        {
            DialogueFlag flag = GetFlag(GlobalFlags, line.condition) ?? GetFlag(_currentDialogFile.Flags, line.condition);

            if(flag.Value)
            {
                if (line.next == null)
                {
                    EndDialogue();
                    yield break;
                }

                JSONDialogue next = _currentDialogFile.GetDialogue(line.next);
                OnDialogueEnded?.Invoke(_currentDialog);
                PlayDialogue(next);
                yield break;
            }
            else
            {
                PlayNextLine();
                yield break;
            }
        }

        if(line.set != null)
        {
            DialogueFlag flag = GetFlag(GlobalFlags, line.set) ?? GetFlag(_currentDialogFile.Flags, line.set);
            flag.Value = true;
            PlayNextLine();
            yield break;
        }

        if (line.unset != null)
        {
            DialogueFlag flag = GetFlag(GlobalFlags, line.unset) ?? GetFlag(_currentDialogFile.Flags, line.unset);
            flag.Value = false;
            PlayNextLine();
            yield break;
        }


        DialoguePanel.SetActive(true);
        PortraitImage.sprite = line.Portrait != null ? line.Portrait : _currentDialog.MainPortrait;
        _state = State.WRITING;

        // Magic numbers, too lazy to turn them into constants...
        if (PortraitImage.sprite == null)
        {
            MessageText.rectTransform.anchoredPosition = new Vector2(19f, -12.1f);
            MessageText.rectTransform.sizeDelta = new Vector2(540.28f, 112.8449f);
            PortraitImage.gameObject.SetActive(false);
        }
        else
        {
            MessageText.rectTransform.anchoredPosition = new Vector2(131.4894f, -12.1f);
            MessageText.rectTransform.sizeDelta = new Vector2(427.7867f, 112.8449f);
            PortraitImage.gameObject.SetActive(true);
        }

        OnLineStarted?.Invoke(line);
        MessageText.text = string.Empty;

        for (int i = 0; i < _currentLine.text.Length; i++)
        {
            if (_state == State.WAITING) break;

            // quick & dirty richtext detection
            if (_currentLine.text[i] == '<')
            {
                int closingIndex = _currentLine.text.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    string tag = _currentLine.text.Substring(i, closingIndex - i + 1);
                    MessageText.text += tag;
                    i = closingIndex;
                    continue;
                }
            }

            MessageText.text += _currentLine.text[i];
            yield return new WaitForSeconds(0.03f);
        }

        MessageText.text = _currentLine.text;
        _state = State.WAITING;
    }

    private void PlayNextLine()
    {
        if(_lineEnumerator.MoveNext())
        {
            JSONDialogueLine line = (JSONDialogueLine)_lineEnumerator.Current;
            StartCoroutine(PlayLine(line));
        }
        else
        {
            if (_currentDialog.choices != null && _currentDialog.choices.Length != 0)
            {
                ChoiceText.text = _currentDialog.choiceText ?? string.Empty;
                Choices.text = string.Join('\n', _currentDialog.choices.Select(i => i.text));
                _choiceIdx = 0;
                ChoicePanel.SetActive(true);
                MoveSelectorToChoice();
                _state = State.CHOICE;
                return;
            }

            EndDialogue();
        }
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        switch(_state)
        {
            case State.WAITING:
                OnLineEnded?.Invoke(_currentLine);
                PlayNextLine();
                break;

            case State.CHOICE:
                JSONDialogueChoice choice = _currentDialog.choices[_choiceIdx];

                if(choice.next == null)
                {
                    EndDialogue();
                    return;
                }

                ChoicePanel.SetActive(false);
                OnDialogueEnded?.Invoke(_currentDialog);
                PlayDialogue(_currentDialogFile.GetDialogue(choice.next));
                break;

            default:
                return;
        }

    }

    private void _skipAction_performed(InputAction.CallbackContext obj)
    {
        if (_state != State.WRITING) return;

        _state = State.WAITING;
    }

    private void _choiceActionPeformed(InputAction.CallbackContext obj)
    {
        if (_state != State.CHOICE) return;

        switch (obj.action.name)
        {
            case "Up":
                _choiceIdx = Mathf.Max(--_choiceIdx, 0);
                break;

            case "Down":
                _choiceIdx = Mathf.Min(++_choiceIdx, _currentDialog.choices.Length - 1);
                break;
        }

        MoveSelectorToChoice();
    }

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        if(ChoicePanel != null) ChoicePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        OnDialogueEnded?.Invoke(_currentDialog);
        OnDialogueFileEnded?.Invoke(_currentDialogFile);
    }

    public DialogueFlag GetGlobalFlag(string name)
    {
        return GetFlag(GlobalFlags, name);
    }

    private DialogueFlag GetFlag(List<DialogueFlag> flags, string name)
    {
        foreach (DialogueFlag flag in flags)
            if (flag.Name == name) return flag;

        return null;
    }

    // Taken from InventorySystem
    private void MoveSelector(Vector3 position)
    {
        Selector.transform.position = position - new Vector3(_selectorOffset, 14.8f);
    }

    private void MoveSelectorToChoice()
    {
        MoveSelector(Choices.transform.position - new Vector3(0, _selectorItemJump * _choiceIdx));
    }
}
