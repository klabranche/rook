using System.Collections.Generic;
using System.Linq;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static Vector<T> ToVector<T>(this IEnumerable<T> items)
        {
            return new ArrayVector<T>(items.ToArray());
        }

        public static Vector<CompilerError> Errors<T>(this Vector<TypeChecked<T>> typeCheckedItems) where T : SyntaxTree
        {
            return typeCheckedItems.SelectMany(typeCheckedItem => typeCheckedItem.Errors).ToVector();
        }

        public static Vector<Class> Classes(this Vector<TypeChecked<Class>> typeCheckedClasses)
        {
            return typeCheckedClasses.Select(x => x.Syntax).ToVector();
        }

        public static Vector<Function> Functions(this Vector<TypeChecked<Function>> typeCheckedFunctions)
        {
            return typeCheckedFunctions.Select(x => x.Syntax).ToVector();
        }

        public static Vector<Expression> Expressions(this Vector<TypeChecked<Expression>> typeCheckedExpressions)
        {
            return typeCheckedExpressions.Select(x => x.Syntax).ToVector();
        }
    }
}