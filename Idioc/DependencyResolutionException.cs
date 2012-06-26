using System;

namespace Idioc
{
    class DependencyResolutionException : Exception
    {
        public Type DependencyType { get; private set; }
        public DependencyResolutionException(Type dependencyType) : this(dependencyType, string.Format("Could not resolve dependency for type: {0}", dependencyType.FullName)) {}
        DependencyResolutionException(Type dependencyType, string message) : base(message)
        {
            DependencyType = dependencyType; 
        }
    }
}