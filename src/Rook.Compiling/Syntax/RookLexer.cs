﻿using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexer : Lexer
    {
        private static readonly Pattern Whitespace = new Pattern("whitespace", @"[ \t\n]+", skippable: true);
        private static readonly Pattern Comment = new Pattern("comment", @"\/\/[^\n]*", skippable: true);

        public static readonly Keyword @int = new Keyword("int");
        public static readonly Keyword @bool = new Keyword("bool");
        public static readonly Keyword @string = new Keyword("string");
        public static readonly Keyword @void = new Keyword("void");
        public static readonly Keyword @null = new Keyword("null");
        public static readonly Keyword @if = new Keyword("if");
        public static readonly Keyword @else = new Keyword("else");
        public static readonly Keyword @fn = new Keyword("fn");
        public static readonly Keyword @true = new Keyword("true");
        public static readonly Keyword @false = new Keyword("false");
        public static readonly Keyword @class = new Keyword("class");
        public static readonly Keyword @new = new Keyword("new");

        public static readonly Pattern Integer = new Pattern("integer", @"
            0(?!\d) #Zero, not followed by other digits.

            |

            [1-9]\d* #Nonzero integer.
        ");
        public static readonly Pattern StringLiteral = new Pattern("string literal", @"
            # Open quote:
            ""

            # Zero or more content characters:
            (
                      [^""\\]              # Non-quote / non-slash character.
                |     \\ [""\\nrt]         # One of: slash-quote   \\   \n   \r   \t
                |     \\ u [0-9a-fA-F]{4}  # \u folowed by four hex digits
            )*

            # Close quote:
            ""
        ");
        public static readonly Pattern Identifier = new Pattern("identifier", @"[_a-zA-Z]+[_a-zA-Z0-9]*");
        public static readonly Operator Semicolon = new Operator(";");
        public static readonly Operator LeftParen = new Operator("(");
        public static readonly Operator RightParen = new Operator(")");
        public static readonly Operator Multiply = new Operator("*");
        public static readonly Operator Divide = new Operator("/");
        public static readonly Operator Add = new Operator("+");
        public static readonly Operator Subtract = new Operator("-");
        public static readonly Operator LessThanOrEqual = new Operator("<=");
        public static readonly Operator LessThan = new Operator("<");
        public static readonly Operator GreaterThanOrEqual = new Operator(">=");
        public static readonly Operator GreaterThan = new Operator(">");
        public static readonly Operator Equal = new Operator("==");
        public static readonly Operator NotEqual = new Operator("!=");
        public static readonly Operator Or = new Operator("||");
        public static readonly Operator And = new Operator("&&");
        public static readonly Operator Not = new Operator("!");
        public static readonly Operator Assignment = new Operator("=");
        public static readonly Operator Comma = new Operator(",");
        public static readonly Operator LeftBrace = new Operator("{");
        public static readonly Operator RightBrace = new Operator("}");
        public static readonly Operator Vector = new Operator("[]");
        public static readonly Operator LeftSquareBrace = new Operator("[");
        public static readonly Operator RightSquareBrace = new Operator("]");
        public static readonly Operator Colon = new Operator(":");
        public static readonly Operator NullCoalesce = new Operator("??");
        public static readonly Operator Question = new Operator("?");
        public static readonly Operator MemberAccess = new Operator(".");

        public RookLexer()
            :base(Whitespace, Comment,
                  @int, @bool, @string, @void, @null, @if, @else, @fn, @true, @false, @class, @new,
                  Integer, StringLiteral, Identifier,
                  LeftParen, RightParen,
                  Multiply, Divide,
                  Add, Subtract,
                  LessThanOrEqual, LessThan, GreaterThanOrEqual, GreaterThan,
                  Equal, NotEqual,
                  Or, And, Not,
                  Assignment, Comma,
                  LeftBrace, RightBrace,
                  Vector, LeftSquareBrace, RightSquareBrace, Colon,
                  NullCoalesce, Question, MemberAccess,
                  Semicolon) { }
    }
}