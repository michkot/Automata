﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Automata.Grammars
{
    internal enum TokenType { NT, T, ARR, OR, ERR, IG, EOS }
    
    internal class Token {
        public TokenType t;
        public string content;
        public int length;
        override public string ToString() {
            string type = "";

            switch (t) {
                case TokenType.NT:
                    type = "NonExprinal(";
                    break;
                case TokenType.T:
                    type = "Exprinal(";
                    break;
                case TokenType.ARR:
                    type = "ARROW(";
                    break;
                case TokenType.OR:
                    type = "OR(";
                    break;
                case TokenType.ERR:
                    type = "ERR(";
                    break;
                case TokenType.IG:
                    type = "IG(";
                    break;
                case TokenType.EOS:
                    type = "END(";
                    break;
            }

            return type + content + ")";
        }
    }

    internal class Lexer
    {
        private string lexbuf;
        private Dictionary<TokenType, Regex> tokendescs = new Dictionary<TokenType, Regex>();

        public Lexer(string buf)
        {
            lexbuf = buf;
            tokendescs[TokenType.NT]  = new Regex(@"^([#A-Z]\S*)");           // Nonterminal
            tokendescs[TokenType.T]   = new Regex(@"^([^#A-Z\-\|\s]\S*)");    // Terminal
            tokendescs[TokenType.ARR] = new Regex(@"^->");                    // Arrow
            tokendescs[TokenType.OR]  = new Regex(@"^\|");                    // Or
            tokendescs[TokenType.IG]  = new Regex(@"^\s+");                   // Ignorables
        }

        private Token DoMatch() {
            Token next = new Token();
            next.t = TokenType.ERR;
            next.length = 1;
            next.content = "";
                
            foreach (KeyValuePair<TokenType, Regex> pair in tokendescs) 
            {
                Match m = pair.Value.Match(lexbuf);
                if (m.Success) 
                {
                    next.length = m.Groups[0].Length;
                    next.t = pair.Key;
                        
                    if (m.Groups.Count > 0) 
                    {
                        next.content = m.Groups[1].Value;
                    }

                    break;
                }
             } // end loop over token types
             return next;
        }

        public Token Next() {
            foreach (Token t in GetTokens()) {
                return t;
            }

            return null;
        }

        public IEnumerable<Token> GetTokens() 
        {
            while (lexbuf.Length > 0) 
            {
                Token next = DoMatch();
                lexbuf = lexbuf.Substring(next.length);

                if (next.t == TokenType.ERR)
                {
                    yield return next;
                    yield break;
                }

                if (next.t != TokenType.IG)
                {
                    yield return next;
                }
            }
            Token end = new Token();
            end.t = TokenType.EOS;
            end.content = "";
            end.length = 0;

            yield return (end);
            yield break;
        }
    }

    class ParseException : System.ApplicationException { }

    public class GrammarParser<T>
    {
        private Lexer lexer;
        private Func<string, Automaton<T>> parseRegex;
        private Grammars.Nonterminal startvar;
        private List<Grammars.Production> productions;

        private Dictionary<string, GrammarSymbol> terminalMap;
        private int __regexId = 0;

        private Dictionary<Nonterminal, Automaton<T>> parsedRegexes;

        private GrammarParser(Lexer lex, Func<string, Automaton<T>> parseRegex)
        {
            lexer = lex;
            this.parseRegex = parseRegex;
            startvar = null;
            productions = new List<Grammars.Production>();
            terminalMap = new Dictionary<string, GrammarSymbol>();
            this.parsedRegexes = new Dictionary<Nonterminal, Automaton<T>>();
        }

        GrammarSymbol MkGrammarSymbolForTerminalOrRegex(string regexOrTerminal)
        {
            GrammarSymbol symb;
            if (!terminalMap.TryGetValue(regexOrTerminal, out symb))
            {
                var aut = parseRegex(regexOrTerminal);
                if (aut.IsEpsilon)
                {
                    symb = null;
                }
                else if (aut.InitialStateIsSource && aut.HasSingleFinalSink && aut.StateCount == 2)
                {
                    //this is indeed a terminal
                    T cond = aut.GetCondition(aut.InitialState, aut.FinalState);
                    symb = new Terminal<T>(cond, regexOrTerminal);
                }
                else
                {
                    int id = __regexId++;
                    var nt = Nonterminal.MkNonterminalForRegex(id);
                    parsedRegexes[nt] = aut;
                    symb = nt;
                }
                terminalMap[regexOrTerminal] = symb;
            }
            return symb;
        }

        public static Grammars.ContextFreeGrammar Parse(Func<string, Automaton<T>> mkTerm, string buf)
        {
            Lexer lex = new Lexer(buf);
            var gp = new GrammarParser<T>(lex, mkTerm);
            gp.Parse();
            Grammars.ContextFreeGrammar G = gp.GetGrammar();
            return G;
        }
        

        private Grammars.Nonterminal ExpectNT()
        {
            Token next = lexer.Next();
            if (next.t != TokenType.NT)
            {
                throw new ParseException();
            }

            return Grammars.Nonterminal.CreateByParser(next.content);
        }

        private void ExpectArrow()
        {
            Token next = lexer.Next();
            if (next.t != TokenType.ARR)
            {
                throw new ParseException();
            }
        }

        private void Parse()
        {
            bool done = false;
            Token cur = null;
            Token last = null;

            Grammars.Nonterminal curlhs = ExpectNT();
            startvar = curlhs;

            ExpectArrow();
            List<Grammars.GrammarSymbol> currhs = new List<Grammars.GrammarSymbol>();
            
            while (!done)
            {
                last = cur;
                cur = lexer.Next();

                switch (cur.t)
                {
                    case TokenType.NT:
                        currhs.Add(Grammars.Nonterminal.CreateByParser(cur.content));
                        break;
                    case TokenType.T:
                        {
                            var symb = MkGrammarSymbolForTerminalOrRegex(cur.content);
                            //symb == null means that the terminal denotes epsilon, e.g. terminal ()
                            if (symb != null)
                                currhs.Add(symb);
                            break;
                        }
                    case TokenType.OR:
                        productions.Add(new Grammars.Production(curlhs, currhs.ToArray()));
                        currhs.Clear();
                        break;
                    case TokenType.ARR:
                        if (currhs.Count < 1) {
                            throw new ParseException();
                        }
                        if (last.t != TokenType.NT) {
                            throw new ParseException();
                        }

                        // downcast :(
                        Grammars.Nonterminal newlhs = (Grammars.Nonterminal)currhs[currhs.Count -1];
                        currhs.RemoveAt(currhs.Count -1);
                        productions.Add(new Grammars.Production(curlhs, currhs.ToArray()));
                        currhs.Clear();
                        curlhs = newlhs;
                        break;
                    case TokenType.EOS:
                        productions.Add(new Grammars.Production(curlhs, currhs.ToArray()));
                        currhs.Clear();
                        done = true;
                        break;
                    default:
                        throw new ParseException();            
                }
            }
        }

        private Grammars.ContextFreeGrammar GetGrammar()
        {
            //add new productions for all automata transitions
            foreach (var entry in this.parsedRegexes)
            {
                var q0 = entry.Key;
                var aut = entry.Value;
                var stateLookup = new Dictionary<int, Nonterminal>();
                var predLookup = new Dictionary<T, Terminal<T>>();
                stateLookup[aut.InitialState] = q0;

                Func<int, Nonterminal> GetState = (q) =>
                {
                    Nonterminal nt;
                    if (!stateLookup.TryGetValue(q, out nt))
                    {
                        nt = Nonterminal.MkNonterminalForAutomatonState(q0.Name, q);
                        stateLookup[q] = nt;
                    }
                    return nt;
                };

                Func<T, Terminal<T>> GetPred = (x) =>
                {
                    Terminal<T> t;
                    if (!predLookup.TryGetValue(x, out t))
                    {
                        t = new Terminal<T>(x, x.ToString());
                        predLookup[x] = t;
                    }
                    return t;
                };


                foreach (var move in aut.GetMoves())
                {
                    if (move.IsEpsilon)
                        productions.Add(new Production(GetState(move.SourceState), GetState(move.TargetState)));
                    else
                        productions.Add(new Production(GetState(move.SourceState), GetPred(move.Label), GetState(move.TargetState)));
                }

                foreach (var qf in aut.GetFinalStates())
                {
                    productions.Add(new Production(GetState(qf)));
                }
            }

            return new Grammars.ContextFreeGrammar(startvar, productions);
        }
    }
}
