using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class TypeChecker
    {
        private readonly TypeUnifier unifier;
        private readonly TypeRegistry typeRegistry;
        private readonly List<CompilerError> errorLog;

        public TypeChecker()
        {
            unifier = new TypeUnifier();
            typeRegistry = new TypeRegistry();
            errorLog = new List<CompilerError>();
        }

        public Vector<CompilerError> Errors { get { return errorLog.ToVector(); } }
        public bool HasErrors { get { return errorLog.Any(); } }

        //TODO: This property is deprecated.  Once TypeRegistry can discover .NET types via reflection,
        //rephrase unit test usages of this so they don't have to manually prepare the TypeRegistry.
        public TypeRegistry TypeRegistry { get { return typeRegistry; } }

        public CompilationUnit TypeCheck(CompilationUnit compilationUnit)
        {
            var position = compilationUnit.Position;
            var classes = compilationUnit.Classes;
            var functions = compilationUnit.Functions;

            foreach (var @class in classes)//TODO: Test coverage.
                typeRegistry.Register(@class);

            var scope = CreateGlobalScope(classes, functions);

            return new CompilationUnit(position, TypeCheck(classes, scope), TypeCheck(functions, scope));
        }

        public Class TypeCheck(Class @class, Scope scope)
        {
            var position = @class.Position;
            var name = @class.Name;
            var methods = @class.Methods;

            var localScope = CreateLocalScope(scope, methods);

            return new Class(position, name, TypeCheck(methods, localScope));
        }

        public Function TypeCheck(Function function, Scope scope)
        {
            var position = function.Position;
            var returnType = function.ReturnType;
            var name = function.Name;
            var parameters = function.Parameters;
            var body = function.Body;
            var declaredType = function.DeclaredType;

            var localScope = CreateLocalScope(scope, parameters);

            var typedBody = TypeCheck(body, localScope);

            Unify(typedBody.Position, returnType, typedBody.Type);

            return new Function(position, returnType, name, parameters, typedBody, declaredType);
        }

        public Expression TypeCheck(Expression expression, Scope scope)
        {
            return expression.WithTypes(this, scope);
        }

        public Expression TypeCheck(Name name, Scope scope)
        {
            var position = name.Position;
            var identifier = name.Identifier;

            DataType type;

            if (scope.TryGet(identifier, out type))
                return new Name(position, identifier, type.FreshenGenericTypeVariables());

            LogError(CompilerError.UndefinedIdentifier(name));
            return name;
        }

        public Expression TypeCheck(Block block, Scope scope)
        {
            var position = block.Position;
            var variableDeclarations = block.VariableDeclarations;
            var innerExpressions = block.InnerExpressions;

            var localScope = new LocalScope(scope);

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in variableDeclarations)
            {
                var typedValue = TypeCheck(variable.Value, localScope);

                var binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typedValue.Type, variable.Identifier, variable.Value);

                if (!localScope.TryIncludeUniqueBinding(binding))
                    LogError(CompilerError.DuplicateIdentifier(binding.Position, binding));

                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position, binding.Type, variable.Identifier, typedValue));

                Unify(typedValue.Position, binding.Type, typedValue.Type);
            }

            var typedInnerExpressions = TypeCheck(innerExpressions, localScope);

            var blockType = typedInnerExpressions.Last().Type;

            return new Block(position, typedVariableDeclarations.ToVector(), typedInnerExpressions, blockType);
        }

        public Expression TypeCheck(Lambda lambda, Scope scope)
        {
            var position = lambda.Position;
            var parameters = lambda.Parameters;
            var body = lambda.Body;

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(parameters).ToVector();

            var localScope = CreateLocalScope(scope, typedParameters);

            var typedBody = TypeCheck(body, localScope);

            var normalizedParameters = NormalizeTypes(typedParameters);

            var parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return new Lambda(position, normalizedParameters, typedBody, NamedType.Function(parameterTypes, typedBody.Type));
        }

        private static IEnumerable<Parameter> ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                if (parameter.IsImplicitlyTyped())
                    yield return new Parameter(parameter.Position, TypeVariable.CreateNonGeneric(), parameter.Identifier);
                else
                    yield return parameter;
        }

        private Vector<Parameter> NormalizeTypes(IEnumerable<Parameter> typedParameters)
        {
            return typedParameters.Select(p => new Parameter(p.Position, unifier.Normalize(p.Type), p.Identifier)).ToVector();
        }

        public Expression TypeCheck(If conditional, Scope scope)
        {
            var position = conditional.Position;
            var condition = conditional.Condition;
            var bodyWhenTrue = conditional.BodyWhenTrue;
            var bodyWhenFalse = conditional.BodyWhenFalse;

            var typedCondition = TypeCheck(condition, scope);
            var typedWhenTrue = TypeCheck(bodyWhenTrue, scope);
            var typedWhenFalse = TypeCheck(bodyWhenFalse, scope);

            Unify(typedCondition.Position, NamedType.Boolean, typedCondition.Type);
            Unify(typedWhenFalse.Position, typedWhenTrue.Type, typedWhenFalse.Type);

            return new If(position, typedCondition, typedWhenTrue, typedWhenFalse, typedWhenTrue.Type);
        }

        public Expression TypeCheck(Call call, Scope scope)
        {
            var position = call.Position;
            var callable = call.Callable;
            var arguments = call.Arguments;
            var isOperator = call.IsOperator;

            var typedCallable = TypeCheck(callable, scope);
            var typedArguments = TypeCheck(arguments, scope);

            var calleeType = typedCallable.Type;
            var calleeFunctionType = calleeType as NamedType;

            if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
            {
                LogError(CompilerError.ObjectNotCallable(position));
                return call;
            }

            var returnType = calleeType.InnerTypes.Last();
            var argumentTypes = typedArguments.Select(x => x.Type).ToVector();

            Unify(position, calleeType, NamedType.Function(argumentTypes, returnType));

            var callType = unifier.Normalize(returnType);

            return new Call(position, typedCallable, typedArguments, isOperator, callType);
        }

        public Expression TypeCheck(MethodInvocation methodInvocation, Scope scope)
        {
            var position = methodInvocation.Position;
            var instance = methodInvocation.Instance;
            var methodName = methodInvocation.MethodName;
            var arguments = methodInvocation.Arguments;

            var typedInstance = TypeCheck(instance, scope);

            var instanceType = typedInstance.Type;
            var instanceNamedType = instanceType as NamedType;

            if (instanceNamedType == null)
            {
                LogError(CompilerError.AmbiguousMethodInvocation(position));
                return methodInvocation;
            }

            Vector<Binding> typeMembers;
            if (typeRegistry.TryGetMembers(instanceNamedType, out typeMembers))
            {
                Scope typeMemberScope = new TypeMemberScope(typeMembers);

                //This block is SUSPICIOUSLY like all of TypeCheck(Call, Scope), but Callable/MethodName is evaluated in a special scope and the successful return is different.

                var Callable = methodName;

                //EXPERIMENTAL - TRY EXTENSION METHOD WHEN WE FAILED TO FIND THE METHOD IN THE TYPE MEMBER SCOPE
                if (!typeMemberScope.Contains(Callable.Identifier))
                {
                    var extensionMethodCall = TypeCheck(new Call(position, methodName, new[] { instance }.Concat(arguments)), scope);

                    if (extensionMethodCall != null)
                        return extensionMethodCall;
                }

                var typedCallable = TypeCheck(Callable, typeMemberScope);
                
                //END EXPERIMENT

                var typedArguments = TypeCheck(arguments, scope);

                var calleeType = typedCallable.Type;
                var calleeFunctionType = calleeType as NamedType;

                if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                {
                    LogError(CompilerError.ObjectNotCallable(position));
                    return methodInvocation;
                }

                var returnType = calleeType.InnerTypes.Last();
                var argumentTypes = typedArguments.Select(x => x.Type).ToVector();

                Unify(position, calleeType, NamedType.Function(argumentTypes, returnType));

                var callType = unifier.Normalize(returnType);
                //End of suspiciously duplicated code.

                var typedMethodName = (Name)typedCallable;
                return new MethodInvocation(position, typedInstance, typedMethodName, typedArguments, callType);
            }
            else
            {
                //HACK: Because TypeRegistry cannot yet look up members for concretions of generic types like int*,
                //  we have to double-check whether this is an extension method call for a regular built-in generic function.
                //  Once TypeRegistry lets you look up the members for a type like int*, this block should be removed.
                if (scope.Contains(methodName.Identifier))
                {
                    var typedCallable = TypeCheck(methodName, scope);
                    if (typedCallable != null)
                    {
                        var extensionMethodCall = TypeCheck(new Call(position, methodName, new[] { instance }.Concat(arguments)), scope);

                        if (extensionMethodCall != null)
                            return extensionMethodCall;
                    }
                }
                //END HACK

                LogError(CompilerError.UndefinedType(instance.Position, instanceNamedType));
                return methodInvocation;
            }
        }

        public Expression TypeCheck(New @new, Scope scope)
        {
            var position = @new.Position;
            var typeName = @new.TypeName;

            var typedTypeName = (Name)TypeCheck(typeName, scope);

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
            {
                LogError(CompilerError.TypeNameExpectedForConstruction(typeName.Position, typeName));
                return @new;
            }

            var constructedType = constructorType.InnerTypes.Last();

            return new New(position, typedTypeName, constructedType);
        }

        public Expression TypeCheck(BooleanLiteral booleanLiteral, Scope scope)
        {
            return booleanLiteral;
        }

        public Expression TypeCheck(IntegerLiteral integerLiteral, Scope scope)
        {
            var position = integerLiteral.Position;
            var digits = integerLiteral.Digits;

            int value;

            if (int.TryParse(digits, out value))
                return new IntegerLiteral(position, digits, NamedType.Integer);

            LogError(CompilerError.InvalidConstant(position, digits));
            return integerLiteral;
        }

        public Expression TypeCheck(StringLiteral stringLiteral, Scope scope)
        {
            return stringLiteral;
        }

        public Expression TypeCheck(Null nullLiteral, Scope scope)
        {
            var Position = nullLiteral.Position;

            return new Null(Position, NamedType.Nullable(TypeVariable.CreateGeneric()));
        }

        public Expression TypeCheck(VectorLiteral vectorLiteral, Scope scope)
        {
            var position = vectorLiteral.Position;
            var items = vectorLiteral.Items;

            var typedItems = TypeCheck(items, scope);

            var firstItemType = typedItems.First().Type;

            foreach (var typedItem in typedItems)
                Unify(typedItem.Position, firstItemType, typedItem.Type);

            return new VectorLiteral(position, typedItems, NamedType.Vector(firstItemType));
        }

        private Vector<Expression> TypeCheck(Vector<Expression> expressions, Scope scope)
        {
            return expressions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<Function> TypeCheck(Vector<Function> functions, Scope scope)
        {
            return functions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<Class> TypeCheck(Vector<Class> classes, Scope scope)
        {
            return classes.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private void Unify(Position position, DataType typeA, DataType typeB)
        {
            //Attempts to unify a type with UnknownType.Instance would produce
            //a redundant and unhelpful error message.  Errors that would lead
            //to unifying with UnknownType.Instance should already report useful
            //error messages prior to unification.

            if (typeA == UnknownType.Instance || typeB == UnknownType.Instance)
                return;

            var errors = unifier.Unify(typeA, typeB).Select(error => new CompilerError(position, error)).ToVector();

            var succeeded = !errors.Any();

            if (!succeeded)
                LogErrors(errors);
        }

        private void LogErrors(IEnumerable<CompilerError> errors)
        {
            errorLog.AddRange(errors);
        }

        private void LogError(CompilerError error)
        {
            errorLog.Add(error);
        }

        private GlobalScope CreateGlobalScope(Vector<Class> classes, Vector<Function> functions)
        {
            var globals = new GlobalScope();

            foreach (var @class in classes)
                if (!globals.TryIncludeUniqueBinding(@class))
                    LogError(CompilerError.DuplicateIdentifier(@class.Position, @class));

            foreach (var function in functions)
                if (!globals.TryIncludeUniqueBinding(function))
                    LogError(CompilerError.DuplicateIdentifier(function.Position, function));

            return globals;
        }

        private LocalScope CreateLocalScope(Scope parent, Vector<Function> methods)
        {
            var locals = new LocalScope(parent);

            foreach (var method in methods)
                if (!locals.TryIncludeUniqueBinding(method))
                    LogError(CompilerError.DuplicateIdentifier(method.Position, method));

            return locals;
        }

        private LocalScope CreateLocalScope(Scope parent, Vector<Parameter> parameters)
        {
            var locals = new LocalScope(parent);

            foreach (var parameter in parameters)
                if (!locals.TryIncludeUniqueBinding(parameter))
                    LogError(CompilerError.DuplicateIdentifier(parameter.Position, parameter));

            return locals;
        }
    }
}