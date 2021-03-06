﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Core;
using Rook.Core.Collections;

namespace Rook.Compiling.Types
{
    public partial class TypeVariable : DataType
    {
        private static readonly Vector<DataType> sharedEmptyGenericArguments = new ArrayVector<DataType>();
        private readonly ulong name;
        private readonly bool isGeneric;

        public TypeVariable(ulong name)
            : this(name, true)
        {
        }

        public TypeVariable(ulong name, bool isGeneric)
        {
            this.name = name;
            this.isGeneric = isGeneric;
        }

        public override string Name
        {
            get { return name.ToString(); }
        }

        public override bool IsGeneric
        {
            get { return isGeneric; }
        }

        public override bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public override Vector<DataType> GenericArguments
        {
            get { return sharedEmptyGenericArguments; }
        }

        public override bool Contains(TypeVariable typeVariable)
        {
            return typeVariable == this;
        }

        public override IEnumerable<TypeVariable> FindTypeVariables()
        {
            yield return this;
        }

        public override DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions)
        {
            if (substitutions.ContainsKey(this))
                return substitutions[this];

            return this;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class TypeVariable
    {
        private static ulong next;
        private static Factory createNext;

        public delegate TypeVariable Factory(bool isGeneric);

        static TypeVariable()
        {
            next = 0;
            createNext = isGeneric => new TypeVariable(next++, isGeneric);
        }

        public static TypeVariable CreateGeneric()
        {
            return createNext(true);
        }

        public static TypeVariable CreateNonGeneric()
        {
            return createNext(false);
        }

        public static IDisposable TestFactory()
        {
            var previousFactory = createNext;

            ulong nextForTest = 0;
            createNext = isGeneric => new TypeVariable(nextForTest++, isGeneric);

            return new DisposableAction(() => createNext = previousFactory);
        }
    }
}