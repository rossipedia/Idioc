using System;
using System.Linq.Expressions;

namespace Idioc
{
    abstract class ExpressionGenerator : IDependencyVisitor, IExpressionGenerator
    {
        protected ExpressionGenerator()
        {
            DependencyExpressionGenerating = (sender, args) => { };
            ExpressionGenerated = (sender, args) => { };
        }

        protected abstract Expression CreateExpression(Type type);

        public Expression GenerateExpression(Type type)
        {
            var expression = CreateExpression(type);
            var args = new ExpressionGeneratedEventArgs(type, expression);
            OnExpressionGenerated(args);
            while (args.Expression.CanReduce)
                args.Expression = args.Expression.Reduce();
            return args.Expression;
        }

        public event EventHandler<ExpressionGeneratedEventArgs> ExpressionGenerated;
        public event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;

        protected virtual void OnExpressionGenerating(ExpressionGeneratingEventArgs args)
        {
            var handler = DependencyExpressionGenerating;
            handler(this, args);
        }

        protected virtual void OnExpressionGenerated(ExpressionGeneratedEventArgs args)
        {
            var handler = ExpressionGenerated;
            handler(this, args);
        }
    }
}