using System;

namespace Idioc
{
    public static class ContainerExtensions
    {
        public static void Register<TConcrete>(this Container container)
        {
            container.Register<TConcrete, TConcrete>();
        }

        public static void Register<TAbstract, TConcrete>(this Container container)
        {
            RegisterSingleUsingConstructor<TAbstract, TConcrete>(container, Container.InstanceProviderFactories.Transient);
        }

        public static void RegisterSingle<TConcrete>(this Container container)
        {
            container.RegisterSingle<TConcrete, TConcrete>();
        }

        public static void RegisterSingle<TAbstract, TConcrete>(this Container container)
        {
            RegisterSingleUsingConstructor<TAbstract, TConcrete>(container, Container.InstanceProviderFactories.Single);
        }

        public static void RegisterSingle<TAbstract>(this Container container, TAbstract instance)
        {
            RegisterSingleUsingConstant(container, instance, Container.InstanceProviderFactories.Single);
        }

        static void RegisterSingleUsingConstant<TAbstract>(Container container, TAbstract instance, IInstanceProviderFactory factory)
        {
            Guard.ArgumentNotNull(container, "container");
            container.Register(typeof(TAbstract), new TypeRegistration(typeof(TAbstract), new ConstantExpressionGenerator(instance), factory));
        }

        private static void RegisterSingleUsingConstructor<TAbstract, TConcrete>(Container container, IInstanceProviderFactory factory)
        {
            Guard.ArgumentNotNull(container, "container");
            var generator = new ConstructorExpressionGenerator();
            generator.DependencyExpressionGenerating += container.DependencyResolver;
            container.Register(typeof(TAbstract), new TypeRegistration(typeof(TConcrete), generator, factory));
        }

        public static TAbstract Resolve<TAbstract>(this Container container)
        {
            if (container == null) throw new ArgumentNullException("container");
            return (TAbstract)container.Resolve(typeof(TAbstract));
        }
    }
}