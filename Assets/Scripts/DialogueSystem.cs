using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [Header("UI References")]
    public GameObject DialoguePanel;
    public TMP_Text MessageText;
    public Image PortraitImage;
    public Transform ChoicesParent;
    public GameObject ChoiceButtonPrefab;

    [Header("Input References")]
    [SerializeField] private PlayerInput _playerInput;

    public static event Action<Dialogue> OnDialogueStarted;
    public static event Action<Dialogue> OnDialogueEnded;
    public static event Action<DialogueLine> OnLineStarted;
    public static event Action<DialogueLine> OnLineEnded;

    private IEnumerator _lineEnumerator;
    private DialogueLine _currentLine;
    private Dialogue _currentDialog;
    
    
    private InputAction _advanceAction;
    private bool _finished;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // Maybe DontDestroyOnLoad would be good here 

        InputActionMap map = _playerInput.actions.FindActionMap("Dialogue");
        _advanceAction = map.FindAction("Advance");
    }

    private void OnEnable()
    {
        _advanceAction.performed += _advanceAction_performed;
    }

    private void OnDisable()
    {
        _advanceAction.performed -= _advanceAction_performed;
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

        DialoguePanel.SetActive(true);
        _currentLine = line;
        PortraitImage.sprite = line.Portrait != null ? line.Portrait : _currentDialog.MainPortrait;
        _finished = false;

        // Magic numbers, too lazy to turn them into constants...
        if(PortraitImage.sprite == null)
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

        // TODO: Branching paths
        //if (line.IsBranching) {}

        OnLineStarted?.Invoke(line);
        MessageText.text = string.Empty;

        for (int i = 0; i < _currentLine.Message.Length; i++)
        {
            MessageText.text += _currentLine.Message[i];
            yield return new WaitForSeconds(0.03f);
        }

        _finished = true;
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
        if (!_finished) return;

        OnLineEnded?.Invoke(_currentLine);
        PlayNextLine();
    }

    public void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        OnDialogueEnded?.Invoke(_currentDialog);
    }
}
