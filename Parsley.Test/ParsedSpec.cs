﻿using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ParsedSpec
    {
        private Lexer unparsed;

        [SetUp]
        public void SetUp()
        {
            unparsed = new CharLexer("0");
        }

        [Test]
        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", unparsed).Value.ShouldEqual("parsed");
        }

        [Test]
        public void HasNoErrorMessage()
        {
            new Parsed<string>("x", unparsed).ErrorMessage.ShouldBeNull();
        }

        [Test]
        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldEqual(unparsed);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }

        [Test]
        public void CanContinueParsingTheRemainingInputWhenGivenAParserGenerator()
        {
            Parser<string> next = tokens => new Parsed<string>(tokens.CurrentToken.Literal, tokens.Advance());

            Reply<string> reply = new Parsed<string>("x", unparsed).ParseRest(s => next);
            reply.Success.ShouldBeTrue();
            reply.Value.ShouldEqual("0");
            reply.UnparsedTokens.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}