﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class TypeUnifier
    {
        private readonly IDictionary<TypeVariable, DataType> substitutions;
        private static readonly IEnumerable<string> success = Enumerable.Empty<string>();

        public TypeUnifier()
        {
            substitutions = new Dictionary<TypeVariable, DataType>();
        }

        public DataType Normalize(DataType type)
        {
            if (type is TypeVariable)
                return NormalizeVariable((TypeVariable)type);

            return NormalizeNamedType((NamedType)type);
        }

        public IEnumerable<string> Unify(DataType a, DataType b)
        {
            a = Normalize(a);
            b = Normalize(b);

            if (a is TypeVariable)
            {
                var variableA = (TypeVariable)a;

                if (b.Contains(variableA))
                {
                    if (a != b)
                        return Error(a, b);

                    return success;
                }

                substitutions[variableA] = b;
                return success;
            }

            if (b is TypeVariable)
                return Unify(b, a);

            if (a.Name != b.Name)
                return Error(a, b);

            if (a.GenericArguments.Count() != b.GenericArguments.Count())
                return Error(a, b);

            return PairwiseUnify(a.GenericArguments, b.GenericArguments);
        }

        private IEnumerable<string> PairwiseUnify(IEnumerable<DataType> first, IEnumerable<DataType> second)
        {
            var errors = new List<string>();

            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
                while (e1.MoveNext() && e2.MoveNext())
                    if (e1.Current != e2.Current)
                        errors.AddRange(Unify(e1.Current, e2.Current));

            return errors;
        }

        private DataType NormalizeNamedType(NamedType named)
        {
            if (!named.IsGeneric)
                return named;

            var normalizedGenericArguments = new List<DataType>();

            bool alreadyNormalized = true;
            foreach (var genericArgument in named.GenericArguments)
            {
                var normalizedGenericArgument = Normalize(genericArgument);

                if (genericArgument != normalizedGenericArgument)
                    alreadyNormalized = false;

                normalizedGenericArguments.Add(normalizedGenericArgument);
            }

            if (alreadyNormalized)
                return named;

            return new NamedType(named.Name, normalizedGenericArguments.ToArray());
        }

        private DataType NormalizeVariable(TypeVariable variable)
        {
            if (substitutions.ContainsKey(variable))
            {
                DataType substitution = substitutions[variable];

                //We cannot simply return substitution, because it may be a complex
                //type containing type variables that have themselves been unified.
                //Instead, we must return the normalized version of that substitution.
                DataType normalizedSubstitution = Normalize(substitution);

                //Optimization: We could just return normalizedSubstitution, but if
                //If normalizing the substitution provided a better answer than the substitution
                //itself, we might as well store that better answer as the substitution going 
                //forward.  This helps by shortening long chains so that they don't have to be 
                //traversed again.
                if (normalizedSubstitution != substitution)
                    substitutions[variable] = normalizedSubstitution;

                return normalizedSubstitution;
            }
            return variable;
        }

        private static IEnumerable<string> Error(DataType expected, DataType actual)
        {
            return new[] {String.Format("Type mismatch: expected {0}, found {1}.", expected, actual)};
        }
    }
}