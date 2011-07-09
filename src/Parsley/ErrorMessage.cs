﻿using System;

namespace Parsley
{
    public abstract class ErrorMessage
    {
        public static ErrorMessage Unknown()
        {
            return new UnknownErrorMessage();
        }

        public static ErrorMessage Expected(string expectation)
        {
            return new ExpectedErrorMessage(expectation);
        }

        public static ErrorMessage Backtrack(Position position, ErrorMessageList errors)
        {
            return new BacktrackErrorMessage(position, errors);
        }
    }

    public class UnknownErrorMessage : ErrorMessage
    {
        internal UnknownErrorMessage() { }

        public override string ToString()
        {
            return "Parse error.";
        }
    }

    /// <summary>
    /// Parsers report this when a specific expectation was not met at the current position.
    /// </summary>
    public class ExpectedErrorMessage : ErrorMessage
    {
        internal ExpectedErrorMessage(string expectation)
        {
            Expectation = expectation;
        }

        public string Expectation { get; private set; }

        public override string ToString()
        {
            return Expectation + " expected";
        }
    }

    /// <summary>
    /// Parsers report this when they have backtracked after an error occurred.
    /// The Position property describes the position where the original error
    /// occurred.
    /// </summary>
    public class BacktrackErrorMessage : ErrorMessage
    {
        internal BacktrackErrorMessage(Position position, ErrorMessageList errors)
        {
            Position = position;
            Errors = errors;
        }

        public Position Position { get; set; }
        public ErrorMessageList Errors { get; set; }

        public override string ToString()
        {
            return String.Format("({0}, {1}): {2}", Position.Line, Position.Column, Errors);
        }
    }
}