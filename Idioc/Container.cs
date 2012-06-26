using System;
using System.Collections.Generic;

namespace Idioc
{
    public class Container
    {
        public Container()
        {
            DependencyResolver = ResolveDependencyExpression;
        }

        private readonly Dictionary<Type, TypeRegistration> registrations = new Dictionary<Type, TypeRegistration>();

        public EventHandler<ExpressionGeneratingEventArgs> DependencyResolver { get; set; }

        public void Register(Type abstractType, TypeRegistration registration)
        {
            if (abstractType == null) throw new ArgumentNullException("abstractType");
            if (registration == null) throw new ArgumentNullException("registration");

            registrations.Add(abstractType, registration);
        }

        public object Resolve(Type type)
        {
            if (IsRegistered(type))
                return GetRegistration(type).GetInstance();

            throw new TypeNotRegisteredException(type);
        }

        public void ResolveDependencyExpression(object sender, ExpressionGeneratingEventArgs args)
        {
            if (IsRegistered(args.DependencyType))
                args.Expression = GetRegistration(args.DependencyType).Expression;
            // Not setting args.Expression will cause a DependencyResolution error
        }

        private bool IsRegistered(Type type)
        {
            return registrations.ContainsKey(type);
        }

        private TypeRegistration GetRegistration(Type forType)
        {
            return registrations[forType];
        }

        public static class InstanceProviderFactories
        {
            // These don't have any state, so they don't need to be ThreadStatic
            private static readonly Lazy<IInstanceProviderFactory> transient = new Lazy<IInstanceProviderFactory>(() => new TransientInstanceProviderFactory());
            private static readonly Lazy<IInstanceProviderFactory> single = new Lazy<IInstanceProviderFactory>(() => new SingleInstanceProviderFactory());

            public static IInstanceProviderFactory Transient { get { return transient.Value; } }
            public static IInstanceProviderFactory Single { get { return single.Value; } }
        }
    }
}




