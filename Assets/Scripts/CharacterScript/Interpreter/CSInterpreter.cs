using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VD.Rulebound.CS
{
    public delegate void DialogueHandler(string dialogueId);
    public delegate void DialogueLineHandler(string message, float secondsBefore);
    public delegate void ItemHandler(string itemId, bool once);
    public delegate bool ItemConditionDelegate(string itemId);
    public delegate void ChoiceHandler(string choiceText, Declaration.Choice[] choices);

    public class CSInterpreter : DialogueStmt.IVisitor
    {
        public static List<Flag> GlobalFlags = new List<Flag>();

        // Events
        public static event DialogueHandler DialogueStarted;
        public static event DialogueHandler DialogueEnded;
        public static event DialogueHandler DialogueChainStarted;
        public static event DialogueHandler DialogueChainEnded;
        public static event DialogueLineHandler DialogueLine;
        public static event Action<string> DialoguePortrait;
        public static event Action<CharacterScript, string> DialogueCallback;
        public static event ChoiceHandler ChoicesStarted;

        public static event ItemHandler GiveItem;
        public static event ItemHandler TakeItem;
        public static ItemConditionDelegate HasItemFunc;

        public CharacterScript CurrentScript { get; private set; }
        private IEnumerator _diagEnumerator;
        private Declaration.Dialogue _currentDialogue;
        private string _currentPortrait;

        private Stack<Context> _contextStack = new Stack<Context>();

        public void StartDialogue(string dialogueId, CharacterScript script)
        {
            Load(script);

            if (_currentDialogue == null)
                DialogueChainStarted?.Invoke(dialogueId);

            SetupDialogue(dialogueId);
            AdvanceDialogue();
        }

        private void Load(CharacterScript script)
        {
            if (CurrentScript != null)
            {
                _contextStack.Push(new Context()
                {
                    _currentDialogue = _currentDialogue,
                    _diagEnumerator = _diagEnumerator,
                    _script = CurrentScript,
                    _currentPortrait = _currentPortrait
                });
            }

            CurrentScript = script;
            _diagEnumerator = null;
            _currentDialogue = null;
        }

        private void SetupDialogue(string dialogueId)
        {
            var diag = CurrentScript.GetDialogue(dialogueId);
            _currentDialogue = diag ?? throw new KeyNotFoundException($"Dialogue '{dialogueId}' not found");

            DialogueStarted?.Invoke(dialogueId);
            _diagEnumerator = _currentDialogue.Statements.GetEnumerator();
            DialoguePortrait?.Invoke(null);
            _currentPortrait = null;
        }

        public void AdvanceDialogue()
        {
            while(_diagEnumerator.MoveNext())
            {
                DialogueStmt stmt = (DialogueStmt)_diagEnumerator.Current;
                if (stmt.Accept(this)) return;
            }

            if (_currentDialogue != null && _currentDialogue.Choices != null && _currentDialogue.Choices.Length > 0)
                ChoicesStarted?.Invoke(_currentDialogue.ChoiceText, _currentDialogue.Choices);
            else
                EndDialogue();
        }

        public void SelectChoice(int idx)
        {
            string next = _currentDialogue.Choices[idx].NextDialogueID;

            if(next == null)
            {
                EndDialogue();
                return;
            }

            DialogueEnded?.Invoke(_currentDialogue.Name);
            SetupDialogue(next);
            AdvanceDialogue();
        }

        public static Flag GetFlag(string name)
        {
            return GlobalFlags.Find(f => f.Name == name);
        }

        public static Flag AddFlag(string name)
        {
            Flag f = GetFlag(name);
            
            if (f == null)
            {
                f = new Flag(name, false);
                GlobalFlags.Add(f);
            }

            return f;
        }

        public void Reset()
        {
            _currentDialogue = null;
            CurrentScript = null;
            _contextStack.Clear();
        } 

        private void EndDialogue()
        {
            DialogueEnded?.Invoke(_currentDialogue.Name);

            if(_contextStack.Count > 0)
            {
                Context ctx = _contextStack.Pop();
                _currentDialogue = ctx._currentDialogue;
                _diagEnumerator = ctx._diagEnumerator;
                CurrentScript = ctx._script;
                _currentPortrait = ctx._currentPortrait;

                DialoguePortrait?.Invoke(_currentPortrait);
                AdvanceDialogue();
                return;
            }

            DialogueChainEnded?.Invoke(_currentDialogue.Name);
            CurrentScript = null;
            _currentDialogue = null;
        }


        public bool VisitLine(DialogueStmt.Line line)
        {
            DialogueLine?.Invoke(line.Text, line.SecondsBefore);
            return true;
        }

        public bool VisitFlagSet(DialogueStmt.FlagSet flagSet)
        {
            CurrentScript.GetFlag(flagSet.Flag).Value = flagSet.Value;
            return false;
        }

        public bool VisitGiveItem(DialogueStmt.GiveItem giveItem)
        {
            GiveItem?.Invoke(giveItem.ItemID, giveItem.GiveOnce);
            return true;
        }

        public bool VisitTakeItem(DialogueStmt.TakeItem takeItem)
        {
            TakeItem?.Invoke(takeItem.ItemID, false);
            return false;
        }

        public bool VisitFlagCondition(DialogueStmt.FlagCondition flagCondition)
        {
            Flag f = CurrentScript.GetFlag(flagCondition.Flag);
            bool check = flagCondition.Negated ? !f.Value : f.Value;

            if(check)
            {
                if(flagCondition.NextDialogue == "return")
                {
                    EndDialogue();
                    return true;
                }

                DialogueEnded?.Invoke(_currentDialogue.Name);
                SetupDialogue(flagCondition.NextDialogue);
            }

            return false;
        }

        public bool VisitItemCondition(DialogueStmt.ItemCondition itemCondition)
        {
            bool f = itemCondition.ItemIDs.All(id => HasItemFunc(id));
            bool check = itemCondition.Negated ? !f : f;

            if (check)
            {
                if (itemCondition.NextDialogue == "return")
                {
                    EndDialogue();
                    return true;
                }

                DialogueEnded?.Invoke(_currentDialogue.Name);
                SetupDialogue(itemCondition.NextDialogue);
            }

            return false;
        }

        public bool VisitPortrait(DialogueStmt.PortraitChange portraitChange)
        {
            _currentPortrait = portraitChange.Portrait;
            DialoguePortrait?.Invoke(portraitChange.Portrait);
            return false;
        }

        public bool VisitRaise(DialogueStmt.Raise raise)
        {
            DialogueCallback?.Invoke(CurrentScript, raise.CallbackName);
            return false;
        }

        internal class Context
        {
            internal CharacterScript _script;
            internal IEnumerator _diagEnumerator;
            internal Declaration.Dialogue _currentDialogue;
            internal string _currentPortrait;
        }
    }

    [Serializable]
    public class Flag
    {
        public string Name;
        public bool Value;

        [NonSerialized] public bool Saved;

        public Flag(string name, bool value)
        {
            Name = name;
            Value = value;
            Saved = false;
        }
    }
}
