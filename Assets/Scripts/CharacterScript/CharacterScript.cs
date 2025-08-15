using System;
using System.Collections.Generic;

namespace VD.Rulebound.CS
{
    public class CharacterScript
    {
        public readonly List<Flag> Flags;
        public readonly Dictionary<string, Declaration.Dialogue> Dialogues;

        public CharacterScript()
        {
            Flags = new List<Flag>();
            Dialogues = new Dictionary<string, Declaration.Dialogue>();
        }

        public Flag GetFlag(string name)
        {
            return CSInterpreter.GetFlag(name) ?? GetLocalFlag(name);
        }

        public Flag GetLocalFlag(string name)
        {
            return Flags.Find(f => f.Name == name);
        }

        public Declaration.Dialogue GetDialogue(string id)
        {
            return Dialogues.ContainsKey(id) ? Dialogues[id] : null;
        }

        public static CharacterScript FromText(string source, string ctx = "internal")
        {
            Lexer lexer = new Lexer();
            Parser parser = new Parser();
            CharacterScript cs = new CharacterScript();

            List<Token> tokens = lexer.Scan(source, ctx);
            List<Declaration> decls = parser.Parse(tokens);

            new CSLoader(cs, decls);

            return cs;
        }
    }

    internal class CSLoader : Declaration.IVisitor
    {
        private readonly CharacterScript _script;

        internal CSLoader(CharacterScript cs, List<Declaration> decls)
        {
            _script = cs;

            foreach(Declaration decl in decls)
                decl.Accept(this);
        }

        public void VisitDialogue(Declaration.Dialogue dialogue)
        {
            _script.Dialogues[dialogue.Name] = dialogue;
        }

        public void VisitFlag(Declaration.Flag flag)
        {
            Flag f = new Flag(flag.Name, false);

            if (flag.IsGlobal)
            {
                if (CSInterpreter.GetFlag(flag.Name) == null)
                    CSInterpreter.GlobalFlags.Add(f);

                return;
            }

            if (_script.GetLocalFlag(flag.Name) == null)
                _script.Flags.Add(f);
        }
    }
}
