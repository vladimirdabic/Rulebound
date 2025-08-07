using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

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

    public static event Action<Dialogue> OnDialogueStarted;
    public static event Action<Dialogue> OnDialogueEnded;
    public static event Action<DialogueLine> OnLineStarted;
    public static event Action<DialogueLine> OnLineEnded;

    private IEnumerator _lineEnumerator;
    private DialogueLine _currentLine;
    private Dialogue _currentDialog;
    private int _choiceIdx;
    
    private InputAction _advanceAction;
    private InputAction _upAction;
    private InputAction _downAction;
    //private bool _finished;
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
    }

    private void OnEnable()
    {
        _advanceAction.performed += _advanceAction_performed;
        _upAction.performed += _choiceActionPeformed;
        _downAction.performed += _choiceActionPeformed;
    }

    private void OnDisable()
    {
        _advanceAction.performed -= _advanceAction_performed;
        _upAction.performed -= _choiceActionPeformed;
        _downAction.performed -= _choiceActionPeformed;
    }

    public void PlayDialogue(Dialogue diag)
    {
        if (diag.Lines == null || diag.Lines.Length == 0) return;

        _currentDialog = diag;
        OnDialogueStarted?.Invoke(diag);

        _playerInput.SwitchCurrentActionMap("Dialogue");
        //DialoguePanel.SetActive(true);

        _lineEnumerator = diag.Lines.GetEnumerator();
        PlayNextLine();
    }

    private IEnumerator PlayLine(DialogueLine line)
    {
        if (line.SecondsBefore > 0)
        {
            DialoguePanel.SetActive(false);
            yield return new WaitForSeconds(line.SecondsBefore);
        }

        _currentLine = line;

        if (line.IsBranching)
        {
            ChoiceText.text = line.Message;
            Choices.text = string.Join('\n', line.Choices.Select(i => i.Choice));
            _choiceIdx = 0;
            ChoicePanel.SetActive(true);
            MoveSelectorToChoice();
            _state = State.CHOICE;
            yield break;
        }

        DialoguePanel.SetActive(true);
        PortraitImage.sprite = line.Portrait != null ? line.Portrait : _currentDialog.MainPortrait;
        //_finished = false;
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

        for (int i = 0; i < _currentLine.Message.Length; i++)
        {
            MessageText.text += _currentLine.Message[i];
            yield return new WaitForSeconds(0.03f);
        }

        //_finished = true;
        _state = State.WAITING;
    }

    private void PlayNextLine()
    {
        if(_lineEnumerator.MoveNext())
        {
            DialogueLine line = (DialogueLine)_lineEnumerator.Current;
            StartCoroutine(PlayLine(line));
        }
        else
        {
            EndDialogue();
        }
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        //if (!_finished) return;

        switch(_state)
        {
            case State.WAITING:
                OnLineEnded?.Invoke(_currentLine);
                PlayNextLine();
                break;

            case State.CHOICE:
                DialogueChoice choice = _currentLine.Choices[_choiceIdx];

                if(choice.NextDialogue == null)
                {
                    EndDialogue();
                    return;
                }

                PlayDialogue(choice.NextDialogue);
                ChoicePanel.SetActive(false);
                break;

            default:
                return;
        }

    }

    private void _choiceActionPeformed(InputAction.CallbackContext obj)
    {
        switch(obj.action.name)
        {
            case "Up":
                _choiceIdx = Mathf.Max(--_choiceIdx, 0);
                break;

            case "Down":
                _choiceIdx = Mathf.Min(++_choiceIdx, _currentLine.Choices.Length - 1);
                break;
        }

        MoveSelectorToChoice();
    }

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        ChoicePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        OnDialogueEnded?.Invoke(_currentDialog);
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
