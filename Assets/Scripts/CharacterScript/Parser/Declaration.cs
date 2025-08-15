using System;
using System.Collections.Generic;

namespace VD.Rulebound.CS
{
    public abstract class Declaration
    {
        public interface IVisitor
        {
            void VisitFlag(Flag flag);
            void VisitDialogue(Dialogue dialogue);
        }

        public abstract void Accept(IVisitor visitor);


        public class Flag : Declaration
        {
            public string Name;
            public bool IsGlobal;

            public Flag(string name, bool isGlobal)
            {
                Name = name;
                IsGlobal = isGlobal;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitFlag(this);
            }
        }

        public class Dialogue : Declaration
        {
            public string Name;
            public List<DialogueStmt> Statements;
            public List<Choice> Choices;
            public string ChoiceText;

            public Dialogue(string name, List<DialogueStmt> statements, List<Choice> choices, string choiceText)
            {
                Name = name;
                Statements = statements;
                Choices = choices;
                ChoiceText = choiceText;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitDialogue(this);
            }
        }

        public class Choice
        {
            public string Text;
            public string NextDialogueID;

            public Choice(string text, string nextDialogueID)
            {
                Text = text;
                NextDialogueID = nextDialogueID;
            }
        }
    }
}
