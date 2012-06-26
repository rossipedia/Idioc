// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainerExtensions.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   The default extensions for the Container. Implement most of our register methods here
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;

    using Idioc.ExpressionGenerators;
    using Idioc.InstanceProviders;

    /// <summary>
    /// Default ContainerExtensions. This class provides the basic interface to
    /// the <see cref="Container"/>.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Registers a type with the container as Transient (new instance every call).
        /// This will call the type's constructor, and attempt to inject any dependent
        /// parameters.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <typeparam name="TConcrete">
        /// The Concrete / Implementation type. Also used as the Service / Interface type.
        /// </typeparam>
        public static void Register<TConcrete>(this Container container)
        {
            container.Register<TConcrete, TConcrete>();
        }

        /// <summary>
        /// Registers a concrete type with the container as Transient (new instance every call)
        /// for a given abstract type. Requests for <see cref="TAbstract"/> will return instances
        /// of <see cref="TConcrete" />
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Service / Interface type.
        /// </typeparam>
        /// <typeparam name="TConcrete">
        /// The Concrete / Implementation type.
        /// </typeparam>
        public static void Register<TAbstract, TConcrete>(this Container container)
        {
            RegisterUsingConstructor<TAbstract, TConcrete>(container, Container.InstanceProviderFactories.Transient);
        }

        /// <summary>
        /// Registers a concrete type with the container as Single (same instance every call)
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <typeparam name="TConcrete">
        /// The Concrete / Implementation type. Also used as the Service / Interface type.
        /// </typeparam>
        public static void RegisterSingle<TConcrete>(this Container container)
        {
            container.RegisterSingle<TConcrete, TConcrete>();
        }

        /// <summary>
        /// Registers a concrete type with the container as Single (same instance every call)
        /// for a given abstract type. Requests for <see cref="TAbstract"/> will return the
        /// same instance of <see cref="TConcrete" />
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Service / Interface type
        /// </typeparam>
        /// <typeparam name="TConcrete">
        /// The Concrete / Implementation type
        /// </typeparam>
        public static void RegisterSingle<TAbstract, TConcrete>(this Container container)
        {
            RegisterUsingConstructor<TAbstract, TConcrete>(container, Container.InstanceProviderFactories.Single);
        }

        /// <summary>
        /// Registers a concrete instance as Single. This instance 
        /// will be returned on every request for the given type.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="instance">
        /// The concrete instance to return.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Service / Interface Type
        /// </typeparam>
        public static void RegisterSingle<TAbstract>(this Container container, TAbstract instance)
        {
            RegisterUsingConstant(container, instance, Container.InstanceProviderFactories.Single);
        }

        /// <summary>
        /// Registers a type with the container as Transient (new instance every call).
        /// This will call the provided lambda to return an instance of the type.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="lambda">
        /// The lambda.
        /// </param>
        /// <typeparam name="TConcrete">
        /// The return type of the provided lambda, this also doubles as the type that is being
        /// registered
        /// </typeparam>
        public static void Register<TConcrete>(this Container container, Func<TConcrete> lambda) where TConcrete : class
        {
            container.Register<TConcrete, TConcrete>(lambda);
        }

        /// <summary>
        /// Registers a type with the container as Transient (new instance every call).
        /// This will call the provided lambda to return an instance of the type.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="lambda">
        /// The lambda.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Interface / Service type to register for.
        /// </typeparam>
        /// <typeparam name="TConcrete">
        /// The return type of the provided lambda
        /// </typeparam>
        public static void Register<TAbstract, TConcrete>(this Container container, Func<TConcrete> lambda) where TConcrete : class, TAbstract
        {
            RegisterUsingLambda<TAbstract, TConcrete>(container, lambda, Container.InstanceProviderFactories.Transient);
        }

        /// <summary>
        /// Resolves a type that has been previously registered with the container
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The requested Interface / Service type
        /// </typeparam>
        /// <returns>
        /// An instance of the requested type with its dependencies resolved.
        /// </returns>
        public static TAbstract Resolve<TAbstract>(this Container container)
        {
            Guard.ArgumentNotNull(container, "container");
            return (TAbstract)container.Resolve(typeof(TAbstract));
        }

        /// <summary>
        /// Registers a type for retrieval using a ConstantExpressionGenerator, providing an instance
        /// to return every time.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="instance">
        /// The instance to return
        /// </param>
        /// <param name="factory">
        /// The IInstanceProviderFactory used to create the IInstanceProvider
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Service / Interface type
        /// </typeparam>
        private static void RegisterUsingConstant<TAbstract>(Container container, TAbstract instance, IInstanceProviderFactory factory)
        {
            Guard.ArgumentNotNull(container, "container");
            container.Register(typeof(TAbstract), new TypeRegistration(typeof(TAbstract), new ConstantExpressionGenerator(instance), factory));
        }

        /// <summary>
        /// Registers a type with the container using the ConstructorExpressionGenerator
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="factory">
        /// The IInstanceProviderFactory used to create the IInstanceProvider
        /// </param>
        /// <typeparam name="TAbstract">
        /// The Interface / Service type
        /// </typeparam>
        /// <typeparam name="TConcrete">
        /// The Implementation / Concrete type
        /// </typeparam>
        private static void RegisterUsingConstructor<TAbstract, TConcrete>(Container container, IInstanceProviderFactory factory)
        {
            Guard.ArgumentNotNull(container, "container");
            
            var generator = new ConstructorExpressionGenerator(ConstructorSelectors.MostSpecific);
            
            // Constructor resolution is currently the only one that needs to resolve dependency
            generator.DependencyExpressionGenerating += container.DependencyResolver;

            container.Register(typeof(TAbstract), new TypeRegistration(typeof(TConcrete), generator, factory));
        }

        /// <summary>
        /// Registers a type for retrieval using a LambdaExpressionGenerator, 
        /// calling the lambda to return an instance every time
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="lambda">
        /// The lambda.
        /// </param>
        /// <param name="factory">
        /// The IInstanceProviderFactory used to create the IInstanceProvider
        /// </param>
        /// <typeparam name="TAbsract">
        /// The Interface / Service type
        /// </typeparam>
        /// <typeparam name="TConcrete">
        /// The Implementation / Concrete type
        /// </typeparam>
        private static void RegisterUsingLambda<TAbsract, TConcrete>(Container container, Func<TConcrete> lambda, IInstanceProviderFactory factory) where TConcrete : class, TAbsract
        {
            Guard.ArgumentNotNull(container, "container");
            container.Register(typeof(TAbsract), new TypeRegistration(typeof(TConcrete), new LambdaExpressionGenerator(lambda), factory));
        }
    }
}