using System;

namespace Idioc
{
    class SingleInstanceProviderFactory : IInstanceProviderFactory
    {
        public IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator)
        {
            return new SingleInstanceProvider(type, generator);
        }
    }
}