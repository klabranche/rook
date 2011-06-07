﻿using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class TokenKindSpec
    {
        private TokenKind lower;
        private TokenKind upper;
        private Text abcDEF;

        [SetUp]
        public void SetUp()
        {
            lower = new TokenKind(@"[a-z]+");
            upper = new TokenKind(@"[A-Z]+");
            abcDEF = new Text("abcDEF");
        }

        [Test]
        public void ProducesNullTokenUponFailedPatternMatch()
        {
            Token token;

            upper.TryMatch(abcDEF, out token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        [Test]
        public void ProducesTokenUponSuccessfulPatternMatch()
        {
            Token token;

            lower.TryMatch(abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);
        }
    }
}