﻿using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorSpec
    {
        private Lexer x, endOfInput;

        [SetUp]
        public void SetUp()
        {
            x = new Lexer(new Text("x"));
            endOfInput = new Lexer(new Text(""));
        }

        [Test]
        public void CanIndicateErrorsAtTheCurrentPosition()
        {
            new Error<object>(endOfInput, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldEqual("Parse error.");
            new Error<object>(endOfInput, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldEqual("statement expected");
        }

        [Test]
        public void CanIndicateMultipleErrorsAtTheCurrentPosition()
        {
            var errors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Error<object>(endOfInput, errors).ErrorMessages.ToString().ShouldEqual("A or B expected");
        }

        [Test]
        [ExpectedException(typeof(MemberAccessException), ExpectedMessage = "(1, 1): Parse error.")]
        public void ThrowsWhenAttemptingToGetParsedValue()
        {
            var value = new Error<object>(x, ErrorMessage.Unknown()).Value;
        }

        [Test]
        public void ProvidesErrorMessageWithPositionAsToString()
        {
            new Error<object>(x, ErrorMessage.Unknown()).ToString().ShouldEqual("(1, 1): Parse error.");
            new Error<object>(x, ErrorMessage.Expected("y")).ToString().ShouldEqual("(1, 1): y expected");
        }

        [Test]
        public void HasRemainingUnparsedTokens()
        {
            new Error<object>(x, ErrorMessage.Unknown()).UnparsedTokens.ShouldEqual(x);
            new Error<object>(endOfInput, ErrorMessage.Unknown()).UnparsedTokens.ShouldEqual(endOfInput);
        }

        [Test]
        public void ReportsErrorState()
        {
            new Error<object>(x, ErrorMessage.Unknown()).Success.ShouldBeFalse();
        }

        [Test]
        public void PropogatesItselfWithoutConsumingInputWhenAskedToParseRemainingInput()
        {
            Parser<string> shouldNotBeCalled = tokens => { throw new Exception(); };

            Reply<string> reply = new Error<object>(x, ErrorMessage.Expected("expectation")).ParseRest(o => shouldNotBeCalled);
            reply.Success.ShouldBeFalse();
            reply.UnparsedTokens.ShouldEqual(x);
            reply.ErrorMessages.ToString().ShouldEqual("expectation expected");
        }
    }
}