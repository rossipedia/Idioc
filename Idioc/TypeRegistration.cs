using System;
using System.Linq.Expressions;

namespace Idioc
{
    public class TypeRegistration : IInstanceProvider
    {
        readonly Type concreteType;
        readonly IInstanceProviderFactory providerFactory;
        readonly IExpressionGenerator generator;
        readonly Lazy<IInstanceProvider> provider;

        public Expression Expression { get { return provider.Value.Expression;  } }

        public TypeRegistration(Type concreteType, IExpressionGenerator generator, IInstanceProviderFactory providerFactory)
        {
            if (concreteType == null) throw new ArgumentNullException("concreteType");
            if (generator == null) throw new ArgumentNullException("generator");
            if (providerFactory == null) throw new ArgumentNullException("providerFactory");

            this.concreteType = concreteType;
            this.providerFactory = providerFactory;
            this.generator = generator;

            provider = new Lazy<IInstanceProvider>(CreateProvider);
        }
        
        public object GetInstance()
        {
            return provider.Value.GetInstance();
        }

        IInstanceProvider CreateProvider()
        {
            return providerFactory.CreateProvider(concreteType, generator);
        }
    }
 }