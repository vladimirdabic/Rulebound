using System;
using System.Collections.Generic;

namespace VD.Rulebound.CS
{
    public abstract class DialogueStmt
    {
        public interface IVisitor
        {
            bool VisitLine(Line line);
            bool VisitRaise(Raise raise);
            bool VisitFlagSet(FlagSet flagSet);
            bool VisitGiveItem(GiveItem giveItem);
            bool VisitTakeItem(TakeItem takeItem);
            bool VisitFlagCondition(FlagCondition flagCondition);
            bool VisitItemCondition(ItemCondition itemCondition);
            bool VisitPortrait(PortraitChange portraitChange);
        }

        public abstract bool Accept(IVisitor visitor);


        public class Line : DialogueStmt
        {
            public string Text;
            public float SecondsBefore;

            public Line(string text, float secondsBefore)
            {
                Text = text;
                SecondsBefore = secondsBefore;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitLine(this);
            }
        }

        public class Raise : DialogueStmt
        {
            public string CallbackName;

            public Raise(string callbackName)
            {
                CallbackName = callbackName;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitRaise(this);
            }
        }

        public class FlagSet : DialogueStmt
        {
            public string Flag;
            public bool Value;

            public FlagSet(string flag, bool value)
            {
                Flag = flag;
                Value = value;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitFlagSet(this);
            }
        }

        public class GiveItem : DialogueStmt
        {
            public string ItemID;
            public bool GiveOnce;

            public GiveItem(string itemID, bool giveOnce)
            {
                ItemID = itemID;
                GiveOnce = giveOnce;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitGiveItem(this);
            }
        }

        public class TakeItem : DialogueStmt
        {
            public string ItemID;

            public TakeItem(string itemID)
            {
                ItemID = itemID;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitTakeItem(this);
            }
        }

        public class FlagCondition : DialogueStmt
        {
            public string Flag;
            public bool Negated;
            public string NextDialogue;

            public FlagCondition(string flag, bool negated, string nextDialogue)
            {
                Flag = flag;
                Negated = negated;
                NextDialogue = nextDialogue;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitFlagCondition(this);
            }
        }

        public class ItemCondition : DialogueStmt
        {
            public string[] ItemIDs;
            public bool Negated;
            public string NextDialogue;

            public ItemCondition(string[] itemIDs, bool negated, string nextDialogue)
            {
                ItemIDs = itemIDs;
                Negated = negated;
                NextDialogue = nextDialogue;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitItemCondition(this);
            }
        }

        public class PortraitChange : DialogueStmt
        {
            public string Portrait;

            public PortraitChange(string portrait)
            {
                Portrait = portrait;
            }

            public override bool Accept(IVisitor visitor)
            {
                return visitor.VisitPortrait(this);
            }
        }
    }
}
