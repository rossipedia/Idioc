using System;

namespace Idioc
{
    public interface IInstanceProviderFactory
    {
        IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator);
    }
}