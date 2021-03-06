﻿using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class SyntaxTreeTests<TSyntax> where TSyntax : SyntaxTree
    {
        protected static RookGrammar RookGrammar { get { return new RookGrammar(); } }
        protected static DataType Integer { get { return NamedType.Integer; } }
        protected static DataType Boolean { get { return NamedType.Boolean; } }
        protected static DataType Unknown { get { return UnknownType.Instance; } }

        protected TSyntax Parse(string source)
        {
            return Parses(source).Value;
        }

        protected Reply<TSyntax> Parses(string source)
        {
            return Parser.Parses(source);
        }

        protected Reply<TSyntax> FailsToParse(string source)
        {
            return Parser.FailsToParse(source);
        }

        protected abstract Parser<TSyntax> Parser { get; }

        protected delegate DataType TypeMapping(string name);

        protected static Scope Scope(params TypeMapping[] locals)
        {
            var globalScope = new GlobalScope();
            var localScope = new LocalScope(globalScope);

            foreach (var local in locals)
            {
                var item = local(null);
                var name = local.Method.GetParameters()[0].Name;
                localScope.Bind(name, item);
            }

            return localScope;
        }
    }
}