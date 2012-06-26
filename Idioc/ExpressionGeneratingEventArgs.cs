using System;
using System.Linq.Expressions;

namespace Idioc
{
    public class ExpressionGeneratingEventArgs : EventArgs
    {
        public ExpressionGeneratingEventArgs(Type dependencyType)
        {
            DependencyType = dependencyType;
        }
        public Type DependencyType { get; private set; }
        public Expression Expression { get; set; }
    }
}