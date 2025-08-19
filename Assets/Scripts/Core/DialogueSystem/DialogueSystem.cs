using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VD.Rulebound.CS;


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

    [Header("Player References")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Other")]
    public List<Flag> GlobalFlags;
    
    private CSInterpreter _interpreter;
    private Declaration.Choice[] _choices;
    private int _choiceIdx;
    private string _currentMsg;

    private InputAction _advanceAction;
    private InputAction _upAction;
    private InputAction _downAction;
    private InputAction _skipAction;
    private State _state;

    private Coroutine _writingCoroutine;

    private enum State
    {
        WAITING, WRITING, CHOICE, BLOCK_INPUT
    }


    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _interpreter = new CSInterpreter();
        GlobalFlags = CSInterpreter.GlobalFlags;

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

        CSInterpreter.DialogueLine += Interpreter_DialogueLine;
        CSInterpreter.DialogueChainEnded += Interpreter_DialogueChainEnded;
        CSInterpreter.ChoicesStarted += Interpreter_ChoicesStarted;
        CSInterpreter.DialogueChainStarted += Interpreter_DialogueChainStarted;
        CSInterpreter.DialoguePortrait += Interpreter_DialoguePortrait;
        CSInterpreter.DialogueCallback += CSInterpreter_DialogueCallback;
    }

    private void OnDisable()
    {
        _advanceAction.performed -= _advanceAction_performed;
        _upAction.performed -= _choiceActionPeformed;
        _downAction.performed -= _choiceActionPeformed;
        _skipAction.performed -= _skipAction_performed;

        CSInterpreter.DialogueLine -= Interpreter_DialogueLine;
        CSInterpreter.DialogueChainEnded -= Interpreter_DialogueChainEnded;
        CSInterpreter.ChoicesStarted -= Interpreter_ChoicesStarted;
        CSInterpreter.DialogueChainStarted -= Interpreter_DialogueChainStarted;
        CSInterpreter.DialoguePortrait -= Interpreter_DialoguePortrait;
        CSInterpreter.DialogueCallback -= CSInterpreter_DialogueCallback;
    }

    public void PlayDialogue(string id, CharacterScript cs)
    {
        _interpreter.StartDialogue(id, cs);
    }

    private void Interpreter_DialoguePortrait(string portraitSprite)
    {
        PortraitImage.sprite = portraitSprite != null ? Resources.Load<Sprite>(portraitSprite) : null;

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
    }

    private void Interpreter_DialogueLine(string message, float secondsBefore)
    {
        _state = State.BLOCK_INPUT;
        _writingCoroutine = StartCoroutine(PlayLine(message, secondsBefore));
    }

    private void Interpreter_ChoicesStarted(string choiceText, Declaration.Choice[] choices)
    {
        _choices = choices;
        ChoiceText.text = choiceText ?? string.Empty;

        Choices.text = string.Join('\n', choices.Select(i => i.Text));
        _choiceIdx = 0;

        ChoicePanel.SetActive(true);
        MoveSelectorToChoice();
        
        _state = State.CHOICE;
    }

    private void Interpreter_DialogueChainStarted(string dialogueId)
    {
        _state = State.BLOCK_INPUT;
        _playerInput.SwitchCurrentActionMap("Dialogue");
    }

    private void Interpreter_DialogueChainEnded(string dialogueId)
    {
        DialoguePanel.SetActive(false);
        if (ChoicePanel != null) ChoicePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
    }

    private void CSInterpreter_DialogueCallback(string callbackId)
    {
        switch(callbackId)
        {
            case "quit":
                StateManager.Quit();
                break;
        }
    }

    private void _advanceAction_performed(InputAction.CallbackContext obj)
    {
        switch (_state)
        {
            case State.WAITING:
                _state = State.BLOCK_INPUT;
                _interpreter.AdvanceDialogue();
                break;

            case State.CHOICE:
                _state = State.BLOCK_INPUT;
                ChoicePanel.SetActive(false);
                _interpreter.SelectChoice(_choiceIdx);
                break;

            default:
                return;
        }

    }

    private void _skipAction_performed(InputAction.CallbackContext obj)
    {
        if (_state != State.WRITING) return;
        if (_writingCoroutine == null) return;

        StopCoroutine(_writingCoroutine);

        _state = State.WAITING;
        MessageText.text = _currentMsg;
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
                _choiceIdx = Mathf.Min(++_choiceIdx, _choices.Length - 1);
                break;
        }

        MoveSelectorToChoice();
    }

    private IEnumerator PlayLine(string message, float secondsBefore)
    {
        //_state = State.BLOCK_INPUT;

        if (secondsBefore > 0)
        {
            DialoguePanel.SetActive(false);
            yield return new WaitForSeconds(secondsBefore);
        }

        DialoguePanel.SetActive(true);
        _state = State.WRITING;
        _currentMsg = message;
        MessageText.text = string.Empty;

        for (int i = 0; i < message.Length; i++)
        {
            // quick & dirty richtext detection
            if (message[i] == '<')
            {
                int closingIndex = message.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    string tag = message.Substring(i, closingIndex - i + 1);
                    MessageText.text += tag;
                    i = closingIndex;
                    continue;
                }
            }

            MessageText.text += message[i];
            yield return new WaitForSeconds(0.03f);
        }

        MessageText.text = message;
        _state = State.WAITING;
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

    public void ForceEndDialogue()
    {
        if(_writingCoroutine != null) StopCoroutine(_writingCoroutine);
        DialoguePanel.SetActive(false);
        if (ChoicePanel != null) ChoicePanel.SetActive(false);
        _playerInput.SwitchCurrentActionMap("Player");
        _interpreter.Reset();
    }
}
