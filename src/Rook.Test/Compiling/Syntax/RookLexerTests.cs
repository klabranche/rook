﻿using System.Collections.Generic;
using System.Linq;
using Parsley;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class RookLexerTests : Grammar
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new RookLexer().Tokenize(new Text(input));
        }

        [Fact]
        public void ShouldRecognizeIntegers()
        {
            Tokenize("0").Single().ShouldBe(RookLexer.Integer, "0");
            Tokenize("1").Single().ShouldBe(RookLexer.Integer, "1");
            Tokenize("01").Single().ShouldBe(TokenKind.Unknown, "01");
            Tokenize("10").Single().ShouldBe(RookLexer.Integer, "10");
            Tokenize("2147483647").Single().ShouldBe(RookLexer.Integer, "2147483647");

            //NOTE: The parser does not limit integer literals to min and max values,
            //      because that is a type checking concern.
            Tokenize("2147483648").Single().ShouldBe(RookLexer.Integer, "2147483648");
        }

        [Fact]
        public void ShouldRecognizeStringLiterals()
        {
            Tokenize("\"\"").Single().ShouldBe(RookLexer.StringLiteral, "\"\"");
            Tokenize("\"a\"").Single().ShouldBe(RookLexer.StringLiteral, "\"a\"");
            Tokenize("\"abc\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc\"");
            Tokenize("\"abc \\\" def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\\" def\"");
            Tokenize("\"abc \\\\ def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\\\ def\"");
            Tokenize("\"abc \\n def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\n def\"");
            Tokenize("\"abc \\r def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\r def\"");
            Tokenize("\"abc \\t def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\t def\"");
            Tokenize("\"abc \\u005C def\"").Single().ShouldBe(RookLexer.StringLiteral, "\"abc \\u005C def\"");

            Tokenize("\" a \" \" b \" \" c \"")
                .ShouldList(t => t.ShouldBe(RookLexer.StringLiteral, "\" a \""),
                            t => t.ShouldBe(RookLexer.StringLiteral, "\" b \""),
                            t => t.ShouldBe(RookLexer.StringLiteral, "\" c \""));
        }

        [Fact]
        public void ShouldRecognizeKeywords()
        {
            Tokenize("true").Single().ShouldBe(RookLexer.@true, "true");
            Tokenize("false").Single().ShouldBe(RookLexer.@false, "false");
            Tokenize("int").Single().ShouldBe(RookLexer.@int, "int");
            Tokenize("bool").Single().ShouldBe(RookLexer.@bool, "bool");
            Tokenize("string").Single().ShouldBe(RookLexer.@string, "string");
            Tokenize("void").Single().ShouldBe(RookLexer.@void, "void");
            Tokenize("null").Single().ShouldBe(RookLexer.@null, "null");
            Tokenize("if").Single().ShouldBe(RookLexer.@if, "if");
            Tokenize("else").Single().ShouldBe(RookLexer.@else, "else");
            Tokenize("fn").Single().ShouldBe(RookLexer.@fn, "fn");
        }

        [Fact]
        public void ShouldRecognizeIdentifiers()
        {
            Tokenize("a").Single().ShouldBe(RookLexer.Identifier, "a");
            Tokenize("ab").Single().ShouldBe(RookLexer.Identifier, "ab");
            Tokenize("a0").Single().ShouldBe(RookLexer.Identifier, "a0");
        }

        [Fact]
        public void ShouldRecognizeOperatorsGreedily()
        {
            Tokenize("<=>=<>!====*/+-&&||!{}[][,]()???:")
                .ShouldList(t => t.ShouldBe(RookLexer.LessThanOrEqual, "<="),
                            t => t.ShouldBe(RookLexer.GreaterThanOrEqual, ">="),
                            t => t.ShouldBe(RookLexer.LessThan, "<"),
                            t => t.ShouldBe(RookLexer.GreaterThan, ">"),
                            t => t.ShouldBe(RookLexer.NotEqual, "!="),
                            t => t.ShouldBe(RookLexer.Equal, "=="),
                            t => t.ShouldBe(RookLexer.Assignment, "="),
                            t => t.ShouldBe(RookLexer.Multiply, "*"),
                            t => t.ShouldBe(RookLexer.Divide, "/"),
                            t => t.ShouldBe(RookLexer.Add, "+"),
                            t => t.ShouldBe(RookLexer.Subtract, "-"),
                            t => t.ShouldBe(RookLexer.And, "&&"),
                            t => t.ShouldBe(RookLexer.Or, "||"),
                            t => t.ShouldBe(RookLexer.Not, "!"),
                            t => t.ShouldBe(RookLexer.LeftBrace, "{"),
                            t => t.ShouldBe(RookLexer.RightBrace, "}"),
                            t => t.ShouldBe(RookLexer.Vector, "[]"),
                            t => t.ShouldBe(RookLexer.LeftSquareBrace, "["),
                            t => t.ShouldBe(RookLexer.Comma, ","),
                            t => t.ShouldBe(RookLexer.RightSquareBrace, "]"),
                            t => t.ShouldBe(RookLexer.LeftParen, "("),
                            t => t.ShouldBe(RookLexer.RightParen, ")"),
                            t => t.ShouldBe(RookLexer.NullCoalesce, "??"),
                            t => t.ShouldBe(RookLexer.Question, "?"),
                            t => t.ShouldBe(RookLexer.Colon, ":"));
        }

        [Fact]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            Tokenize("\r\n").Single().ShouldBe(RookLexer.EndOfLine, "\n");
            Tokenize("\r\n \r\n \t ").Single().ShouldBe(RookLexer.EndOfLine, "\n \n \t ");

            Tokenize(";").Single().ShouldBe(RookLexer.EndOfLine, ";");
            Tokenize("; \r\n \t ").Single().ShouldBe(RookLexer.EndOfLine, "; \n \t ");
        }

        [Fact]
        public void ShouldRecognizeAndSkipOverIntralineWhitespace()
        {
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            Tokenize(" a if == \r\n 0 ")
                .ShouldList(t => t.ShouldBe(RookLexer.Identifier, "a"),
                            t => t.ShouldBe(RookLexer.@if, "if"),
                            t => t.ShouldBe(RookLexer.Equal, "=="),
                            t => t.ShouldBe(RookLexer.EndOfLine, "\n "),
                            t => t.ShouldBe(RookLexer.Integer, "0"));

            Tokenize("\ta\tif\t==\t\r\n\t0\t")
                .ShouldList(t => t.ShouldBe(RookLexer.Identifier, "a"),
                            t => t.ShouldBe(RookLexer.@if, "if"),
                            t => t.ShouldBe(RookLexer.Equal, "=="),
                            t => t.ShouldBe(RookLexer.EndOfLine, "\n\t"),
                            t => t.ShouldBe(RookLexer.Integer, "0"));

            Tokenize(" \t a \t if \t == \t \r\n \t 0 \t ")
                .ShouldList(t => t.ShouldBe(RookLexer.Identifier, "a"),
                            t => t.ShouldBe(RookLexer.@if, "if"),
                            t => t.ShouldBe(RookLexer.Equal, "=="),
                            t => t.ShouldBe(RookLexer.EndOfLine, "\n \t "),
                            t => t.ShouldBe(RookLexer.Integer, "0"));

            Tokenize("\t \ta\t \tif\t \t==\t \t\r\n\t \t0\t \t")
                .ShouldList(t => t.ShouldBe(RookLexer.Identifier, "a"),
                            t => t.ShouldBe(RookLexer.@if, "if"),
                            t => t.ShouldBe(RookLexer.Equal, "=="),
                            t => t.ShouldBe(RookLexer.EndOfLine, "\n\t \t"),
                            t => t.ShouldBe(RookLexer.Integer, "0"));
        }
    }
}