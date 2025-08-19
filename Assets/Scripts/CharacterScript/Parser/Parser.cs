using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace VD.Rulebound.CS
{
    internal class Parser
    {
        private Token[] _tokens;

        private int _current;
        
        public Declaration[] Parse(Token[] tokens)
        {
            _tokens = tokens;
            List<Declaration> decls = new List<Declaration>();

            while(Available())
            {
                decls.Add(ParseDecl());
            }

            return decls.ToArray();
        }

        private Declaration ParseDecl()
        {
            if(Match(TokenType.FLAG))
            {
                Token name = Consume(TokenType.ID, "Expected flag name after 'flag'");

                bool global = Match(TokenType.GLOBAL);
                bool saved = !global && Match(TokenType.SAVED);
                if (saved) global = true;

                return new Declaration.Flag((string)name.Value, global, saved);
            }

            if(Match(TokenType.DIALOGUE))
            {
                return ParseDialogue();
            }

            throw Error("Expected top declaration");
        }

        private Declaration.Dialogue ParseDialogue()
        {
            Token id = Consume(TokenType.ID, "Expected dialogue id after 'dialogue'");

            Consume(TokenType.OPEN_BRACE, "Expected dialogue body");
            List<DialogueStmt> stmts = new List<DialogueStmt>();
            List<Declaration.Choice> choices = null;
            string choiceText = null;

            while(Available() && !Check(TokenType.CLOSE_BRACE))
            {
                if(Match(TokenType.CHOICES))
                {
                    choices ??= new List<Declaration.Choice>();
                    choiceText = Match(TokenType.STR) ? (string)Prev().Value : null;
                    choices.AddRange(ParseChoices());
                    continue;
                }

                //DialogueStmt stmt = ParseDiagStmt();
                stmts.Add(ParseDiagStmt());
            }

            Consume(TokenType.CLOSE_BRACE, "Expected '}' to close dialogue body");

            return new Declaration.Dialogue((string)id.Value, stmts.ToArray(), choices?.ToArray(), choiceText);
        }

        private List<Declaration.Choice> ParseChoices()
        {
            List<Declaration.Choice> choices = new List<Declaration.Choice>();

            Consume(TokenType.OPEN_BRACE, "Expected choices body");

            while (Available() && !Check(TokenType.CLOSE_BRACE))
            {
                string text = (string)Consume(TokenType.STR, "Expected choice text string").Value;
                string nextDiagId = null;

                if (Match(TokenType.RARROW))
                    nextDiagId = (string)Consume(TokenType.ID, "Expected next dialogue id after '->'").Value;

                //Consume(TokenType.RARROW, "Expected '->' after choice text");
                choices.Add(new Declaration.Choice(text, nextDiagId));
            }

            Consume(TokenType.CLOSE_BRACE, "Expected '}' to close choices body");

            return choices;
        }

        private DialogueStmt ParseDiagStmt()
        {
            if (Match(TokenType.SET))
            {
                Token flagName = Consume(TokenType.ID, "Expected flag name after 'set'");
                return new DialogueStmt.FlagSet((string)flagName.Value, true);
            }

            if (Match(TokenType.UNSET))
            {
                Token flagName = Consume(TokenType.ID, "Expected flag name after 'unset'");
                return new DialogueStmt.FlagSet((string)flagName.Value, false);
            }

            if (Match(TokenType.GIVE))
            {
                Token itemId = Consume(TokenType.STR, "Expected item id string after 'give'");
                bool once = MatchIdentifier("once");
                return new DialogueStmt.GiveItem((string)itemId.Value, once);
            }

            if (Match(TokenType.TAKE))
            {
                Token itemId = Consume(TokenType.STR, "Expected item id string after 'take'");
                return new DialogueStmt.TakeItem((string)itemId.Value);
            }

            if (Match(TokenType.IF))
            {
                return ParseDiagCond();
            }

            if (Match(TokenType.PORTRAIT))
            {
                if (MatchIdentifier("none"))
                    return new DialogueStmt.PortraitChange(null);
                else
                {
                    Token portrait = Consume(TokenType.STR, "Expected portrait sprite string after 'portrait'");
                    return new DialogueStmt.PortraitChange((string)portrait.Value);
                }
            }

            if (Match(TokenType.RAISE))
            {
                Token callback = Consume(TokenType.ID, "Expected callback after 'raise'");
                return new DialogueStmt.Raise((string)callback.Value);
            }

            Token diagLine = Consume(TokenType.STR, "Expected dialogue line or dialogue instruction");
            float secondsBefore = Match(TokenType.WAIT) ? (float)Consume(TokenType.NUM, "Expected wait time as a number after 'wait'").Value : 0;
            return new DialogueStmt.Line((string)diagLine.Value, secondsBefore);
        }

        private DialogueStmt ParseDiagCond()
        {
            bool negated = Match(TokenType.NOT);

            if(Match(TokenType.FLAG))
            {
                Token flagName = Consume(TokenType.ID, "Expected flag name after 'if flag'");
                Consume(TokenType.RARROW, "Expected '->' for next dialogue id");
                Token diagId = Consume(TokenType.ID, "Expected next dialogue id after '->'");

                return new DialogueStmt.FlagCondition((string)flagName.Value, negated, (string)diagId.Value);
            }

            if(Match(TokenType.ITEM))
            {
                List<string> itemIds = new List<string>();
                
                if(Match(TokenType.OPEN_SQUARE))
                {
                    do
                    {
                        Token itemId = Consume(TokenType.STR, "Expected item id string in 'if item' list");
                        itemIds.Add((string)itemId.Value);
                    }
                    while (Match(TokenType.COMMA));

                    Consume(TokenType.CLOSE_SQUARE, "Expected ']' to close item list");
                }
                else
                {
                    Token itemId = Consume(TokenType.STR, "Expected item id string after 'if item'");
                    itemIds.Add((string)itemId.Value);
                }

                Consume(TokenType.RARROW, "Expected '->' for next dialogue id");
                Token diagId = Consume(TokenType.ID, "Expected next dialogue id after '->'");

                return new DialogueStmt.ItemCondition(itemIds.ToArray(), negated, (string)diagId.Value);
            }

            throw Error("Expected 'item' or 'flag' after if");
        }

        private Token Consume(TokenType type, string message)
        {
            Token token = Advance();

            if (token.Type != type)
                throw new ParserException(token.SourceName, token.Line, message);

            return token;
        }

        private Token Advance()
        {
            return _tokens[_current++];
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Prev()
        {
            return _tokens[_current - 1];
        }

        private bool Available()
        {
            return Peek().Type != TokenType.EOF;
        }

        private bool Match(TokenType type)
        {
            if(Peek().Type == type)
            {
                Advance();
                return true;
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            return Peek().Type == type;
        }

        private bool MatchIdentifier(string value)
        {
            if ((Peek().Type == TokenType.ID) && ((string)Peek().Value) == value)
            {
                _current++;
                return true;
            }

            return false;
        }

        private ParserException Error(string message)
        {
            return new ParserException(Peek().SourceName, Peek().Line, message);
        }
    }

    public class ParserException : Exception
    {
        public int Line;
        public string Context;

        public ParserException(string source, int line, string message) : base(message)
        {
            Context = source;
            Line = line;
        }
    }
}
