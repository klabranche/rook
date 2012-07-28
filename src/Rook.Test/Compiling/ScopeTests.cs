﻿using System;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class ScopeTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly Scope global, ab, cd;

        public ScopeTests()
        {
            var typeChecker = new TypeChecker();
            global = new GlobalScope(typeChecker);

            ab = global.CreateLocalScope();
            ab.Bind("a", Integer);
            ab.Bind("b", Integer);

            cd = ab.CreateLocalScope();
            cd.Bind("c", Boolean);
            cd.Bind("d", Boolean);
        }

        [Fact]
        public void StoresLocals()
        {
            AssertType(Integer, ab, "a");
            AssertType(Integer, ab, "b");

            AssertType(Boolean, cd, "c");
            AssertType(Boolean, cd, "d");
        }

        [Fact]
        public void DefersToSurroundingScopeAfterSearchingLocals()
        {
            AssertType(Integer, cd, "a");
            AssertType(Integer, cd, "b");
        }

        [Fact]
        public void ProvidesContainmentPredicate()
        {
            ab.Contains("a").ShouldBeTrue();
            ab.Contains("b").ShouldBeTrue();
            ab.Contains("c").ShouldBeFalse();
            ab.Contains("d").ShouldBeFalse();
            ab.Contains("z").ShouldBeFalse();

            cd.Contains("a").ShouldBeTrue();
            cd.Contains("b").ShouldBeTrue();
            cd.Contains("c").ShouldBeTrue();
            cd.Contains("d").ShouldBeTrue();
            cd.Contains("z").ShouldBeFalse();
        }

        [Fact]
        public void ProvidesBuiltinSignaturesInTheGlobalScope()
        {
            AssertType("System.Func<int, int, bool>", global, "<");
            AssertType("System.Func<int, int, bool>", global, "<=");
            AssertType("System.Func<int, int, bool>", global, ">");
            AssertType("System.Func<int, int, bool>", global, ">=");
            AssertType("System.Func<int, int, bool>", global, "==");
            AssertType("System.Func<int, int, bool>", global, "!=");

            AssertType("System.Func<int, int, int>", global, "+");
            AssertType("System.Func<int, int, int>", global, "-");
            AssertType("System.Func<int, int, int>", global, "*");
            AssertType("System.Func<int, int, int>", global, "/");

            AssertType("System.Func<bool, bool, bool>", global, "||");
            AssertType("System.Func<bool, bool, bool>", global, "&&");
            AssertType("System.Func<bool, bool>", global, "!");

            AssertType("System.Func<Rook.Core.Nullable<2>, 2, 2>", global, "??");
            AssertType("System.Func<3, Rook.Core.Void>", global, "Print");
            AssertType("System.Func<4, Rook.Core.Nullable<4>>", global, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<5>, 5>", global, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<6>, int, System.Collections.Generic.IEnumerable<6>>", global, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<7>, int, System.Collections.Generic.IEnumerable<7>>", global, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<8>, bool>", global, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<9>, int>", global, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<10>, System.Func<10, 11>, System.Collections.Generic.IEnumerable<11>>", global, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<12>, System.Func<12, bool>, System.Collections.Generic.IEnumerable<12>>", global, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<13>, System.Collections.Generic.IEnumerable<13>>", global, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<14>, int, 14>", global, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<15>, int, int, Rook.Core.Collections.Vector<15>>", global, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<16>, 16, Rook.Core.Collections.Vector<16>>", global, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<17>, int, 17, Rook.Core.Collections.Vector<17>>", global, "With");
        }

        [Fact]
        public void CanBePopulatedWithAUniqueBinding()
        {
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            global.TryIncludeUniqueBinding(new Parameter(null, Boolean, "b")).ShouldBeTrue();
            AssertType(Integer, global, "a");
            AssertType(Boolean, global, "b");
        }

        [Fact]
        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeFalse();
            global.TryIncludeUniqueBinding(new Parameter(null, Boolean, "a")).ShouldBeFalse();
            AssertType(Integer, global, "a");
        }

        [Fact]
        public void CanDetermineWhetherAGivenTypeVariableIsGenericWhenPreparedWithAKnownListOfNonGenericTypeVariables()
        {
            var var0 = new TypeVariable(0);
            var var1 = new TypeVariable(1);
            var var2 = new TypeVariable(2);
            var var3 = new TypeVariable(3);

            var outerLambda = cd.CreateLambdaScope();
            var local = outerLambda.CreateLocalScope();
            var middleLambda = local.CreateLambdaScope();
            var local2 = middleLambda.CreateLocalScope();
            var innerLambda = local2.CreateLambdaScope();

            outerLambda.TreatAsNonGeneric(new[] { var0 });
            middleLambda.TreatAsNonGeneric(new[] { var1, var2 });
            innerLambda.TreatAsNonGeneric(new[] { var3 });

            outerLambda.IsGeneric(var0).ShouldBeFalse();
            outerLambda.IsGeneric(var1).ShouldBeTrue();
            outerLambda.IsGeneric(var2).ShouldBeTrue();
            outerLambda.IsGeneric(var3).ShouldBeTrue();

            middleLambda.IsGeneric(var0).ShouldBeFalse();
            middleLambda.IsGeneric(var1).ShouldBeFalse();
            middleLambda.IsGeneric(var2).ShouldBeFalse();
            middleLambda.IsGeneric(var3).ShouldBeTrue();

            innerLambda.IsGeneric(var0).ShouldBeFalse();
            innerLambda.IsGeneric(var1).ShouldBeFalse();
            innerLambda.IsGeneric(var2).ShouldBeFalse();
            innerLambda.IsGeneric(var3).ShouldBeFalse();
        }

        [Fact]
        public void FreshensTypeVariablesOnEachLookup()
        {
            var scope = new GlobalScope(new TypeChecker());

            scope.Bind("concreteType", Integer);
            AssertType(Integer, scope, "concreteType");

            scope.Bind("typeVariable", new TypeVariable(0));
            AssertType(new TypeVariable(2), scope, "typeVariable");
            AssertType(new TypeVariable(3), scope, "typeVariable");

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(4), new TypeVariable(5), new NamedType("B", new TypeVariable(4), new TypeVariable(5)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));
            scope.Bind("genericTypeIncludingTypeVariables", definedType);
            AssertType(expectedTypeAfterLookup, scope, "genericTypeIncludingTypeVariables");
        }

        [Fact]
        public void FreshensOnlyGenericTypeVariablesOnEachLookup()
        {
            //Prevents type '1' from being freshened on type lookup by marking it as non-generic in the scope:

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(1), new NamedType("B", new TypeVariable(2), new TypeVariable(1)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));

            var globalScope = new GlobalScope(new TypeChecker());
            var lambdaScope = globalScope.CreateLambdaScope();
            lambdaScope.TreatAsNonGeneric(new[] { new TypeVariable(1) });
            lambdaScope.Bind("genericTypeIncludingGenericAndNonGenericTypeVariables", definedType);

            AssertType(expectedTypeAfterLookup, lambdaScope, "genericTypeIncludingGenericAndNonGenericTypeVariables");
        }

        private static void AssertType(DataType expectedType, Scope scope, string key)
        {
            DataType value;

            if (scope.TryGet(key, out value))
                value.ShouldEqual(expectedType);
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
        }

        private static void AssertType(string expectedType, Scope scope, string key)
        {
            DataType value;

            if (scope.TryGet(key, out value))
                expectedType.ShouldEqual(value.ToString());
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
        }
    }
}