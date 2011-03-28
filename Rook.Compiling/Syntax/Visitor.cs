﻿namespace Rook.Compiling.Syntax
{
    public interface Visitor<out TResult>
    {
        TResult Visit(Program program);
        TResult Visit(Function function);
        TResult Visit(Name name);
        TResult Visit(Parameter parameter);
        TResult Visit(Block block);
        TResult Visit(Lambda lambda);
        TResult Visit(If conditional);
        TResult Visit(VariableDeclaration variableDeclaration);
        TResult Visit(Call call);
        TResult Visit(BooleanLiteral booleanLiteral);
        TResult Visit(IntegerLiteral integerLiteral);
        TResult Visit(Null nullLiteral);
        TResult Visit(VectorLiteral vectorLiteral);
    }
}