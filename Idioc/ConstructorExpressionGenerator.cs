using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Idioc
{
    class ConstructorExpressionGenerator : ExpressionGenerator
    {
        protected override Expression CreateExpression(Type type)
        {
            var constructor = SelectConstructor(type);
            if (constructor == null)
                throw new TypeNotConstructableException(type);
            var parameterExpressions = GenerateParameterExpressions(constructor);
            return Expression.New(constructor, parameterExpressions);
        }

        static ConstructorInfo SelectConstructor(Type type)
        {
            var constructors = from constructor in type.GetConstructors()
                               orderby constructor.GetParameters().Length descending
                               select constructor;

            return constructors.FirstOrDefault();
        }

        private IEnumerable<Expression> GenerateParameterExpressions(ConstructorInfo constructor)
        {
            var parameterEventArgs = from p in constructor.GetParameters()
                                     select new ExpressionGeneratingEventArgs(p.ParameterType);

            foreach (var eventArgs in parameterEventArgs)
            {
                OnExpressionGenerating(eventArgs);
                if (eventArgs.Expression == null)
                    throw new DependencyResolutionException(eventArgs.DependencyType);
                yield return eventArgs.Expression;
            }
        }
    }
}