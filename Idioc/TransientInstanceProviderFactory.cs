using System;

namespace Idioc
{
    class TransientInstanceProviderFactory : IInstanceProviderFactory
    {
        public IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator)
        {
            return new TransientInstanceProvider(type, generator);
        }
    }
}