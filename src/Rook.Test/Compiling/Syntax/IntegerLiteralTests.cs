using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class IntegerLiteralTests : ExpressionTests
    {
        [Test]
        public void IsZeroOrPositiveSequenceOfDigits()
        {
            Parses("0").IntoTree("0");
            Parses("2147483647").IntoTree("2147483647");

            //Overflow values parse, but will be caught by the type checker.
            Parses("2147483648").IntoTree("2147483648");
        }

        [Test]
        public void HasIntegerType()
        {
            AssertType(Integer, "0");
            AssertType(Integer, "2147483647");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var integer = (IntegerLiteral)Parse("12345");
            integer.Type.ShouldBeNull();

            var typedInteger = (IntegerLiteral)integer.WithTypes(Environment()).Syntax;
            typedInteger.Type.ShouldEqual(Integer);
        }

        [Test]
        public void FailsTypeCheckingWhenOutOfRange()
        {
            AssertTypeCheckError(1, 1, "Invalid constant: 2147483648", "2147483648");
        }
    }
}