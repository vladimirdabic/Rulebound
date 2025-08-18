using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
            CharacterScript cs = new CharacterScript();
            Declaration[] decls = CSLoader.GetHashedTree(source);

            if(decls == null)
            {
                Lexer lexer = new Lexer();
                Parser parser = new Parser();

                Token[] tokens = lexer.Scan(source, ctx);
                decls = parser.Parse(tokens);
            }

            new CSLoader(cs, source, decls);
            return cs;
        }
    }

    internal class CSLoader : Declaration.IVisitor
    {
        private readonly CharacterScript _script;
        internal static readonly Dictionary<string, Declaration[]> _cachedASTs = new Dictionary<string, Declaration[]>();

        internal CSLoader(CharacterScript cs, string source, Declaration[] decls)
        {
            string hash = GetStableHash(source);

            _script = cs;
            _cachedASTs[hash] = decls;

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
            f.Saved = flag.IsSaved;

            if (flag.IsGlobal)
            {
                if (CSInterpreter.GetFlag(flag.Name) == null)
                    CSInterpreter.GlobalFlags.Add(f);

                return;
            }

            if (_script.GetLocalFlag(flag.Name) == null)
                _script.Flags.Add(f);
        }

        internal static Declaration[] GetHashedTree(string source)
        {
            string hash = GetStableHash(source);

            if(_cachedASTs.ContainsKey(hash)) 
                return _cachedASTs[hash];
            
            return null;
        }

        private static string GetStableHash(string input)
        {
            using var md5 = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(bytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
