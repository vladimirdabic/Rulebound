using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text messageText;
    public Image portraitImage;
    public Transform choicesParent;
    public GameObject choiceButtonPrefab;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    public event UnityAction<Dialogue> OnDialogueStarted;
    public event UnityAction<Dialogue> OnDialogueEnded;
    public event UnityAction<DialogueLine> OnLineStarted;
    public event UnityAction<DialogueLine> OnLineEnded;

    private IEnumerator _lineEnumerator;
    private DialogueLine _currentLine;
    private Dialogue _currentDialog;
    
    
    private InputAction _advanceAction;
    private bool _finished;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        InputActionMap map = _playerInput.actions.FindActionMap("Dialogue");
        _advanceAction = map.FindAction("Advance");
    }

    private void OnEnable()
    {
        SubEvents();
    }

    private void OnDisable()
    {
        UnsubEvents();
    }

    private void SubEvents()
    {
        _advanceAction.performed += _advanceAction_performed;
    }

    private void UnsubEvents()
    {
        _advanceAction.performed -= _advanceAction_performed;
    }

    public void PlayDialogue(Dialogue diag)
    {
        if (diag.Lines == null || diag.Lines.Length == 0) return;

        _currentDialog = diag;
        OnDialogueStarted?.Invoke(diag);

        _lineEnumerator = diag.Lines.GetEnumerator();
        PlayNextLine();
    }

    private void PlayLine(DialogueLine line)
    {
        _playerInput.SwitchCurrentActionMap("Dialogue");
        _currentLine = line;

        dialoguePanel.SetActive(true);
        portraitImage.sprite = line.Portrait != null ? line.Portrait : _currentDialog.MainPortrait;
        _finished = false;

        //ClearChoices();
        //if (line.IsBranching) {}

        OnLineStarted?.Invoke(line);
        StartCoroutine(WriteText());
    }

    private void PlayNextLine()
    {
        if(_lineEnumerator.MoveNext())
        {
            DialogueLine line = (DialogueLine)_lineEnumerator.Current;
            PlayLine(line);
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator WriteText()
    {
        messageText.text = string.Empty;

        for(int i = 0; i < _currentLine.Message.Length; i++)
        {
            messageText.text += _currentLine.Message[i];
            yield return new WaitForSeconds(0.03f);
        }

        _finished = true;
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        if (!_finished) return;

        OnLineEnded?.Invoke(_currentLine);
        PlayNextLine();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        OnDialogueEnded?.Invoke(_currentDialog);
    }
}
