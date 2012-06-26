using System;

namespace Idioc
{
    interface IDependencyVisitor
    {
        event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;
    }
}