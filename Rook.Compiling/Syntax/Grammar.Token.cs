﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        private delegate Parser<Token> TokenParser(Position position);

        private static readonly string[] keywords = new[]
        {
            "true", "false", "int", "bool", "void", "null", 
            "if", "return", "else", "fn"
        };

        private static readonly string[] operators = new[]
        {
            "(", ")", "*", "/", "+", "-", "<=", "<",
            ">=", ">", "==", "!=", "||", "&&", "!",
            "=", ",", "{", "}", "[]", "[", "]", "??", "?", ".", ":"
        };

        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(Token(position =>
                                     from line in Choice(String(System.Environment.NewLine, ";"), EndOfInput)
                                     from post in ZeroOrMore(WhiteSpace)
                                     select new Token(position, line)), "end of line");
            }
        }

        public static Parser<Token> Integer
        {
            get
            {
                return Token(position =>
                             from digits in OneOrMore(Digit)
                             select new Token(position, digits));
            }
        }

        public static Parser<Token> Boolean
        {
            get
            {
                return Keyword("true", "false");
            }
        }

        public static Parser<Token> AnyOperator
        {
            get
            {
                return Token(position =>
                             from symbol in String(operators)
                             select new Token(position, symbol));
            }
        }

        public static Parser<Token> Operator(params string[] expectedOperators)
        {
            return OnError(Expect(AnyOperator, IsOneOf(expectedOperators)), System.String.Join(", ", expectedOperators));
        }

        public static Parser<Token> AnyKeyword
        {
            get
            {
                return Token(position =>
                             from keyword in String(keywords)
                             from peekAhead in Not(OneOrMore(Letter))
                             select new Token(position, keyword));
            }
        }

        public static Parser<Token> Keyword(params string[] expectedKeywords)
        {
            return Expect(AnyKeyword, IsOneOf(expectedKeywords));
        }

        public static Parser<Token> Identifier
        {
            get
            {
                return Expect(Token(position =>
                                    from prefix in OneOrMore(Letter)
                                    from suffix in ZeroOrMore(Alphanumeric)
                                    select new Token(position, prefix + suffix)),
                              IsNotOneOf(keywords));
            }
        }

        private static Parser<Token> Token(TokenParser goal)
        {
            return from spaces in ZeroOrMore(ch => ch == ' ' || ch == '\t')
                   from position in Position
                   from g in goal(position)
                   select g;
        }

        private static Predicate<Token> IsOneOf(IEnumerable<string> values)
        {
            return x => values.Contains(x.ToString());
        }

        private static Predicate<Token> IsNotOneOf(IEnumerable<string> values)
        {
            return x => !values.Contains(x.ToString());
        }
    }
}