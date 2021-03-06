﻿using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexerTests
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new RookLexer().Tokenize(input);
        }

        public void ShouldRecognizeIntegers()
        {
            Tokenize("0").Single().ShouldEqual(RookLexer.Integer, "0");
            Tokenize("1").Single().ShouldEqual(RookLexer.Integer, "1");
            Tokenize("01").Single().ShouldEqual(TokenKind.Unknown, "01");
            Tokenize("10").Single().ShouldEqual(RookLexer.Integer, "10");
            Tokenize("2147483647").Single().ShouldEqual(RookLexer.Integer, "2147483647");

            //NOTE: The parser does not limit integer literals to min and max values,
            //      because that is a type checking concern.
            Tokenize("2147483648").Single().ShouldEqual(RookLexer.Integer, "2147483648");
        }

        public void ShouldRecognizeStringLiterals()
        {
            Tokenize("\"\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"\"");
            Tokenize("\"a\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"a\"");
            Tokenize("\"abc\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc\"");
            Tokenize("\"abc \\\" def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\\" def\"");
            Tokenize("\"abc \\\\ def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\\\ def\"");
            Tokenize("\"abc \\n def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\n def\"");
            Tokenize("\"abc \\r def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\r def\"");
            Tokenize("\"abc \\t def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\t def\"");
            Tokenize("\"abc \\u005C def\"").Single().ShouldEqual(RookLexer.StringLiteral, "\"abc \\u005C def\"");

            Tokenize("\" a \" \" b \" \" c \"")
                .ShouldList(t => t.ShouldEqual(RookLexer.StringLiteral, "\" a \""),
                            t => t.ShouldEqual(RookLexer.StringLiteral, "\" b \""),
                            t => t.ShouldEqual(RookLexer.StringLiteral, "\" c \""));
        }

        public void ShouldRecognizeKeywords()
        {
            Tokenize("true").Single().ShouldEqual(RookLexer.@true, "true");
            Tokenize("false").Single().ShouldEqual(RookLexer.@false, "false");
            Tokenize("int").Single().ShouldEqual(RookLexer.@int, "int");
            Tokenize("bool").Single().ShouldEqual(RookLexer.@bool, "bool");
            Tokenize("string").Single().ShouldEqual(RookLexer.@string, "string");
            Tokenize("void").Single().ShouldEqual(RookLexer.@void, "void");
            Tokenize("null").Single().ShouldEqual(RookLexer.@null, "null");
            Tokenize("if").Single().ShouldEqual(RookLexer.@if, "if");
            Tokenize("else").Single().ShouldEqual(RookLexer.@else, "else");
            Tokenize("fn").Single().ShouldEqual(RookLexer.@fn, "fn");
            Tokenize("class").Single().ShouldEqual(RookLexer.@class, "class");
            Tokenize("new").Single().ShouldEqual(RookLexer.@new, "new");
        }

        public void ShouldRecognizeIdentifiers()
        {
            Tokenize("a").Single().ShouldEqual(RookLexer.Identifier, "a");
            Tokenize("ab").Single().ShouldEqual(RookLexer.Identifier, "ab");
            Tokenize("a0").Single().ShouldEqual(RookLexer.Identifier, "a0");
            Tokenize("_true_").Single().ShouldEqual(RookLexer.Identifier, "_true_");
        }

        public void ShouldRecognizeOperatorsGreedily()
        {
            Tokenize(";<=>=<>!====*/+-&&||!{}[][,]()???:.")
                .ShouldList(t => t.ShouldEqual(RookLexer.Semicolon, ";"),
                            t => t.ShouldEqual(RookLexer.LessThanOrEqual, "<="),
                            t => t.ShouldEqual(RookLexer.GreaterThanOrEqual, ">="),
                            t => t.ShouldEqual(RookLexer.LessThan, "<"),
                            t => t.ShouldEqual(RookLexer.GreaterThan, ">"),
                            t => t.ShouldEqual(RookLexer.NotEqual, "!="),
                            t => t.ShouldEqual(RookLexer.Equal, "=="),
                            t => t.ShouldEqual(RookLexer.Assignment, "="),
                            t => t.ShouldEqual(RookLexer.Multiply, "*"),
                            t => t.ShouldEqual(RookLexer.Divide, "/"),
                            t => t.ShouldEqual(RookLexer.Add, "+"),
                            t => t.ShouldEqual(RookLexer.Subtract, "-"),
                            t => t.ShouldEqual(RookLexer.And, "&&"),
                            t => t.ShouldEqual(RookLexer.Or, "||"),
                            t => t.ShouldEqual(RookLexer.Not, "!"),
                            t => t.ShouldEqual(RookLexer.LeftBrace, "{"),
                            t => t.ShouldEqual(RookLexer.RightBrace, "}"),
                            t => t.ShouldEqual(RookLexer.Vector, "[]"),
                            t => t.ShouldEqual(RookLexer.LeftSquareBrace, "["),
                            t => t.ShouldEqual(RookLexer.Comma, ","),
                            t => t.ShouldEqual(RookLexer.RightSquareBrace, "]"),
                            t => t.ShouldEqual(RookLexer.LeftParen, "("),
                            t => t.ShouldEqual(RookLexer.RightParen, ")"),
                            t => t.ShouldEqual(RookLexer.NullCoalesce, "??"),
                            t => t.ShouldEqual(RookLexer.Question, "?"),
                            t => t.ShouldEqual(RookLexer.Colon, ":"),
                            t => t.ShouldEqual(RookLexer.MemberAccess, "."));
        }

        public void ShouldRecognizeAndSkipOverWhitespace()
        {
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            Tokenize(" a if == \r\n 0 ")
                .ShouldList(t => t.ShouldEqual(RookLexer.Identifier, "a"),
                            t => t.ShouldEqual(RookLexer.@if, "if"),
                            t => t.ShouldEqual(RookLexer.Equal, "=="),
                            t => t.ShouldEqual(RookLexer.Integer, "0"));

            Tokenize("\ta\tif\t==\t\r\n\t0\t")
                .ShouldList(t => t.ShouldEqual(RookLexer.Identifier, "a"),
                            t => t.ShouldEqual(RookLexer.@if, "if"),
                            t => t.ShouldEqual(RookLexer.Equal, "=="),
                            t => t.ShouldEqual(RookLexer.Integer, "0"));

            Tokenize(" \t a \t if \t == \t \r\n \t 0 \t ")
                .ShouldList(t => t.ShouldEqual(RookLexer.Identifier, "a"),
                            t => t.ShouldEqual(RookLexer.@if, "if"),
                            t => t.ShouldEqual(RookLexer.Equal, "=="),
                            t => t.ShouldEqual(RookLexer.Integer, "0"));

            Tokenize("\t \ta\t \tif\t \t==\t \t\r\n\t \t0\t \t")
                .ShouldList(t => t.ShouldEqual(RookLexer.Identifier, "a"),
                            t => t.ShouldEqual(RookLexer.@if, "if"),
                            t => t.ShouldEqual(RookLexer.Equal, "=="),
                            t => t.ShouldEqual(RookLexer.Integer, "0"));
        }

        public void ShouldRecognizeAndSkipOverComments()
        {
            Tokenize("1 2//3\r4 5//6\n7 8//9\r\n10\n//\n11//")
                .ShouldList(t => t.ShouldEqual(RookLexer.Integer, "1"),
                            t => t.ShouldEqual(RookLexer.Integer, "2"),
                            t => t.ShouldEqual(RookLexer.Integer, "4"),
                            t => t.ShouldEqual(RookLexer.Integer, "5"),
                            t => t.ShouldEqual(RookLexer.Integer, "7"),
                            t => t.ShouldEqual(RookLexer.Integer, "8"),
                            t => t.ShouldEqual(RookLexer.Integer, "10"),
                            t => t.ShouldEqual(RookLexer.Integer, "11"));
        }
    }
}