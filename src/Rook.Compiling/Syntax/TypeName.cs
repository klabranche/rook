﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;
using Rook.Core;

namespace Rook.Compiling.Syntax
{
    public class TypeName : Value<TypeName>
    {
        public static readonly TypeName Empty = new TypeName();

        private readonly string name;
        private readonly TypeName[] genericArguments;
        private readonly string fullName;

        private TypeName()
        {
            name = "";
            genericArguments = new TypeName[] {};
            fullName = "";
        }

        public TypeName(string name, params TypeName[] genericArguments)
        {
            this.name = name;
            this.genericArguments = genericArguments;

            fullName = genericArguments.Any()
                           ? String.Format("{0}<{1}>", name, String.Join(", ", (IEnumerable<TypeName>) genericArguments))
                           : name;
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<TypeName> GenericArguments
        {
            get { return genericArguments; }
        }

        protected override object[] ImmutableFields()
        {
            return new object[] { fullName };
        }

        public override string ToString()
        {
            return fullName;
        }

        [Obsolete]
        public DataType ToDataType()
        {
            if (this == Empty)
                return UnknownType.Instance;

            return new NamedType(name, genericArguments.Select(x => x.ToDataType()).ToArray());
        }
    }
}