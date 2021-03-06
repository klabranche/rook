﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;

namespace Rook
{
    public class Serializer : Visitor<string>
    {
        public string Visit(CompilationUnit compilationUnit)
        {
            var classes = Translate(compilationUnit.Classes, " ");
            var functions = Translate(compilationUnit.Functions, " ");

            var list = new List<string>();

            if (classes != "")
                list.Add(classes);

            if (functions != "")
                list.Add(functions);

            return String.Join(" ", list);
        }

        public string Visit(Class @class)
        {
            return String.Format("class {0} {{{1}}}", Translate(@class.Name), Translate(@class.Methods, " "));
        }
 
        public string Visit(Function function)
        {
            return String.Format("{0} {1}({2}) {3}",
                                 Translate(function.ReturnTypeName),
                                 Translate(function.Name),
                                 Translate(function.Parameters, ", "),
                                 Translate(function.Body));
        }

        public string Visit(Name name)
        {
            return name.Identifier;
        }

        public string Visit(Parameter parameter)
        {
            if (parameter.IsImplicitlyTyped)
                return String.Format("{0}", parameter.Identifier);

            return String.Format("{0} {1}", Translate(parameter.DeclaredTypeName), parameter.Identifier);
        }

        public string Visit(Block block)
        {
            return String.Format("{{{0}{1}}}",
                                 Translate(block.VariableDeclarations, " ").Trim(),
                                 Translate(block.InnerExpressions, "; ").Trim());
        }

        public string Visit(Lambda lambda)
        {
            return String.Format("fn ({0}) {1}", Translate(lambda.Parameters, ", "), Translate(lambda.Body));
        }

        public string Visit(If conditional)
        {
            return String.Format("(if ({0}) ({1}) else ({2}))", Translate(conditional.Condition), Translate(conditional.BodyWhenTrue), Translate(conditional.BodyWhenFalse));
        }

        public string Visit(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration.IsImplicitlyTyped)
                return String.Format("{0} = {1};", variableDeclaration.Identifier, Translate(variableDeclaration.Value));

            return String.Format("{0} {1} = {2};", Translate(variableDeclaration.DeclaredTypeName), variableDeclaration.Identifier, Translate(variableDeclaration.Value));
        }

        public string Visit(Call call)
        {
            var callable = call.Callable;
            var arguments = call.Arguments;
            bool isOperator = call.IsOperator;

            if (isOperator)
            {
                if (arguments.Count() == 1)
                {
                    return String.Format("({0}({1}))",
                                         Translate(callable),
                                         Translate(arguments.First()));
                }

                return String.Format("(({0}) {1} ({2}))",
                                     Translate(arguments.ElementAt(0)),
                                     Translate(callable),
                                     Translate(arguments.ElementAt(1)));
            }

            return String.Format("({0}({1}))",
                                 Translate(callable),
                                 Translate(arguments, ", "));
        }

        public string Visit(MethodInvocation methodInvocation)
        {
            return String.Format("{0}.{1}({2})",
                                 Translate(methodInvocation.Instance),
                                 Translate(methodInvocation.MethodName),
                                 Translate(methodInvocation.Arguments, ", "));
        }

        public string Visit(New @new)
        {
            return String.Format("(new {0}())", Translate(@new.TypeName));
        }

        public string Visit(BooleanLiteral booleanLiteral)
        {
            return booleanLiteral.Value ? "true" : "false";
        }

        public string Visit(IntegerLiteral integerLiteral)
        {
            return integerLiteral.Digits;
        }

        public string Visit(StringLiteral stringLiteral)
        {
            return stringLiteral.QuotedLiteral;
        }

        public string Visit(Null nullLiteral)
        {
            return "null";
        }

        public string Visit(VectorLiteral vectorLiteral)
        {
            return String.Format("[{0}]", Translate(vectorLiteral.Items, ", "));
        }

        private string Translate<T>(IEnumerable<T> items, string separator) where T : SyntaxTree
        {
            return String.Join(separator, items.Select(Translate));
        }

        private static string Translate(TypeName typeName)
        {
            return typeName.ToString()
                .Replace("System.Boolean", "bool")
                .Replace("System.Int32", "int")
                .Replace("System.String", "string");
        }

        private string Translate<T>(T item) where T : SyntaxTree
        {
            return item.Visit(this);
        }
    }
}