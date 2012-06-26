using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Idioc
{
    #region EventArgs

    class ExpressionGeneratingEventArgs : EventArgs
    {
        public ExpressionGeneratingEventArgs(Type dependencyType)
        {
            DependencyType = dependencyType;
        }

        public Type DependencyType { get; private set; }
        public Expression Expression { get; set; }
    }

    #endregion

    #region Interfaces

    interface IExpressionGenerator
    {
        Expression GenerateExpression(Type type);
        event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;
    }

    interface IConstructorSelector
    {
        ConstructorInfo SelectConstructor(Type type);
    }

    #endregion

    #region Implementations

    class MostSpecificConstructorSelector : IConstructorSelector
    {
        public ConstructorInfo SelectConstructor(Type type)
        {
            var constructors = from constructor in type.GetConstructors()
                               orderby constructor.GetParameters().Length descending
                               select constructor;

            return constructors.FirstOrDefault();
        }
    }

    abstract class ExpressionGenerator : IExpressionGenerator
    {
        protected ExpressionGenerator()
        {
            DependencyExpressionGenerating = (sender, args) => { };
        }

        public abstract Expression GenerateExpression(Type type);

        public event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;

        protected virtual void OnExpressionGenerating(ExpressionGeneratingEventArgs args)
        {
            var handler = DependencyExpressionGenerating;
            handler(this, args);
        }
    }

    class ConstructorExpressionGenerator : ExpressionGenerator
    {
        private readonly IConstructorSelector selector;

        public ConstructorExpressionGenerator(IConstructorSelector selector)
        {
            this.selector = selector;
        }

        public override Expression GenerateExpression(Type type)
        {
            var constructor = selector.SelectConstructor(type);

            var parameterExpressions = GenerateParameterExpressions(constructor);

            return Expression.New(constructor, parameterExpressions);
        }

        private IEnumerable<Expression> GenerateParameterExpressions(ConstructorInfo constructor)
        {
            var parameterEventArgs = from p in constructor.GetParameters() 
                                     select new ExpressionGeneratingEventArgs(p.ParameterType);

            foreach (var eventArgs in parameterEventArgs)
            {
                OnExpressionGenerating(eventArgs);
                yield return eventArgs.Expression;
            }
        }
    }

    class ConstantExpressionGenerator : ExpressionGenerator
    {
        private readonly object instance;

        public ConstantExpressionGenerator(object instance)
        {
            this.instance = instance;
            
        }

        public override Expression GenerateExpression(Type type)
        {
            return Expression.Constant(instance, type);
        }
    }

    class LambdaExpressionGenerator : ExpressionGenerator
    {
        private readonly Func<object> lambda;

        public LambdaExpressionGenerator(Func<object> lambda)
        {
            this.lambda = lambda;
        }

        public override Expression GenerateExpression(Type type)
        {
            // Extract body from lambda and 
            var lambdaTarget = lambda.Target == null ? null : Expression.Constant(lambda.Target);
            return Expression.Convert(Expression.Call(lambdaTarget, lambda.Method), type);
        }
    }

    #endregion
}