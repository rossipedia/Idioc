using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Idioc.Exceptions;

namespace Idioc
{
    public class Container
    {
        readonly Dictionary<Type, IResolver> registeredTypes = new Dictionary<Type, IResolver>();

        /// <summary>
        /// Registers a type to itself
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>()
        {
            Register(typeof(T));
        }

        /// <summary>
        /// Registers a type for later resolution to itself
        /// </summary>
        /// <param name="type"></param>
        public void Register(Type type)
        {
            Register(type, type);
        }
        
        /// <summary>
        /// Registers a type for later resolution
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <typeparam name="TResolved">The type to return as the implementation of the requested type</typeparam>
        public void Register<TRequested, TResolved>() where TResolved: TRequested
        {
            Register(typeof(TRequested), typeof(TResolved));
        }

        /// <summary>
        /// Weakly typed version of Register&lt;&gt;
        /// </summary>
        /// <param name="requested"></param>
        /// <param name="resolved"></param>
        public void Register(Type requested, Type resolved)
        {
            RegisterInternal(requested, resolved, new TransientResolver(GetConstructorResolverExpression(resolved)));
        }

        /// <summary>
        /// Registers a type for resolution using a lambda function
        /// </summary>
        /// <param name="requested"></param>
        /// <param name="resolved"></param>
        /// <param name="lambda"></param>
        public void Register(Type requested, Type resolved, Func<object> lambda)
        {
            RegisterInternal(requested, resolved, new TransientResolver(GetLambdaResolver(lambda)));
        }

        /// <summary>
        /// Registers a type to be resolved by a method/lambda call
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <param name="lambda">The lambda/method to use to return the resolved instance</param>
        public void Register<TRequested>(Func<TRequested> lambda)
        {
            RegisterInternal(typeof(TRequested), typeof(TRequested), new TransientResolver(GetLambdaResolver(lambda)));
        }

        /// <summary>
        /// Registers a type to be resolved later, but only once.
        /// Ever subsequent resolution will return the same instance
        /// </summary>
        /// <typeparam name="TRequested"></typeparam>
        public void RegisterSingle<TRequested>()
        {
            RegisterSingle(typeof(TRequested));
        }

        /// <summary>
        /// Registers a type to be resolved later, but only once.
        /// Ever subsequent resolution will return the same instance
        /// </summary>
        /// <param name="requestedType"></param>
        public void RegisterSingle(Type requestedType)
        {
            RegisterSingle(requestedType, requestedType);
        }

        /// <summary>
        /// Registers a type to be resolved later, but only once.
        /// Ever subsequent resolution will return the same instance
        /// </summary>
        /// <param name="requestedType"></param>
        /// <param name="resolvedType"></param>
        public void RegisterSingle(Type requestedType, Type resolvedType)
        {
            RegisterInternal(requestedType, resolvedType, new SingleResolver(GetConstructorResolverExpression(resolvedType)));
        }

        /// <summary>
        /// Registers an instance as a singleton for a type
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <param name="instance">The instance to return as the resolved type</param>
        public void RegisterSingle<TRequested>(TRequested instance)
        {
            RegisterInternal(typeof(TRequested), typeof(TRequested), new SingleResolver(Expression.Constant(instance)));
        }
        
        /// <summary>
        /// Creates an expression resolver from a lambda
        /// </summary>
        /// <typeparam name="TRequested"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        static Expression GetLambdaResolver<TRequested>(Func<TRequested> lambda)
        {
            return Expression.Call(lambda.Method.IsStatic ? null : Expression.Constant(lambda.Target), lambda.Method);
        }

        /// <summary>
        /// Performs the actual work of registering a type for resolution
        /// </summary>
        /// <param name="requestedType">The type to register for resolution</param>
        /// <param name="resolvedType">The type to return as the implementation of the requested type</param>
        /// <param name="resolver">The expression to use to resolve the type</param>
        void RegisterInternal(Type requestedType, Type resolvedType, IResolver resolver)
        {
            EnsureTypeIsRegisterable(requestedType, resolvedType);
            EnsureDependenciesAreRegistered(resolver.Expression);
            registeredTypes[requestedType] = resolver;
        }

        /// <summary>
        /// Validates that a type is ok to register with the container
        /// </summary>
        /// <param name="requestedType">The type requestest</param>
        /// <param name="resolvedType">The type to resolve to</param>
        void EnsureTypeIsRegisterable(Type requestedType, Type resolvedType)
        {
            if (IsRegistered(requestedType))
                throw new TypeRegistrationException(requestedType, String.Format("Type {0} has already been registered with Idioc Container", requestedType.FullName));

            if (!requestedType.IsAssignableFrom(resolvedType))
                throw new TypeRegistrationException(resolvedType, String.Format("Type {0} is not assignable from type {1}", requestedType.FullName, resolvedType.FullName));
        }

        /// <summary>
        /// Walks an expression tree to make sure that all the dependencies 
        /// of the given type have been registered with the container. 
        /// Does not check the type itself for registration
        /// </summary>
        /// <param name="forExpression"> </param>
        void EnsureDependenciesAreRegistered(Expression forExpression)
        {
            IEnumerable<Expression> arguments = null;
            if (forExpression is NewExpression)
                arguments = ((NewExpression)forExpression).Arguments;
            else if (forExpression is LambdaExpression)
                arguments = ((LambdaExpression)forExpression).Parameters;


            if (arguments == null) return;

            foreach (var argument in arguments)
            {
                if (!IsRegistered(argument.Type))
                    throw new UnregisteredTypeException(argument.Type);
                EnsureDependenciesAreRegistered(argument);
            }
        }

        /// <summary>
        /// Resolve a requested type to an actual instance
        /// </summary>
        /// <typeparam name="TRequested">The type requested (usually an interface, but not necessarily)</typeparam>
        /// <returns></returns>
        public TRequested Resolve<TRequested>()
        {
            return (TRequested)Resolve(typeof(TRequested));
        }

        /// <summary>
        /// Weakly typed version of Resolve
        /// </summary>
        /// <param name="forType"> </param>
        /// <returns></returns>
        public object Resolve(Type forType)
        {
            if (!IsRegistered(forType))
                throw new UnregisteredTypeException(forType);
            return GetRegistration(forType).GetInstance();
        }

        /// <summary>
        /// Returns the constructor info for the specified type.
        /// This is used for transient requests
        /// </summary>
        /// <param name="forType">The type requested</param>
        /// <returns>A System.Reflection.ConstructorInfo object 
        /// describing the constructor to use for resolution</returns>
        ConstructorInfo GetConstructor(Type forType)
        {
            // Look for constructor with most args
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            var ctors = from c in forType.GetConstructors(bindingFlags)
                        orderby c.GetParameters().Length descending
                        select c;

            var ctor = ctors.FirstOrDefault();
            if (ctor == null)
            {
                throw new TypeNotConstructableException(forType);
            }

            return ctor;
        }

        /// <summary>
        /// Checks if a type has been registered with this container,
        /// either as a requested type, or a resolved type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if registered, false if not</returns>
        public bool IsRegistered(Type type)
        {
            return registeredTypes.Any(r => r.Key == type || r.Value.ResolvedType == type);
        }

        /// <summary>
        /// Generates an expression to create a new instance
        /// of an object using a constructor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Expression GetConstructorResolverExpression(Type type)
        {
            if (IsRegistered(type))
                return GetRegistration(type).Expression;
            // Primitive for now
            var ctor = GetConstructor(type);
            var parameterExpressions = from p in ctor.GetParameters()
                                       select GetConstructorResolverExpression(p.ParameterType);
            return Expression.New(ctor, parameterExpressions);
        }
        
        /// <summary>
        /// Returns the internal registration object for a given type
        /// </summary>
        /// <param name="type">The type to retrieve the registration object for</param>
        /// <returns>The registration object</returns>
        IResolver GetRegistration(Type type)
        {
            return registeredTypes[type];
        }
    }
    
    interface IResolver
    {
        Type ResolvedType { get; }
        Expression Expression { get; }
        object GetInstance();
    }

    #region Resolvers

    class SingleResolver : IResolver
    {
        readonly Lazy<object> instance;
        readonly Func<object> function;

        public SingleResolver(Expression expression)
        {
            Expression = expression;
            ResolvedType = expression.Type;
            function = Expression.Lambda<Func<object>>(expression).Compile();
            instance = new Lazy<object>(() => function());
        }

        public Expression Expression { get; private set; }

        public object GetInstance()
        {
            return instance.Value;
        }

        public Type ResolvedType { get; private set; }
    }

    class TransientResolver : IResolver
    {
        readonly Func<object> function;

        public TransientResolver(Expression expression)
        {
            Expression = expression;
            ResolvedType = expression.Type;
            function = Expression.Lambda<Func<object>>(expression).Compile();
        }

        public Type ResolvedType { get; private set; }
        public Expression Expression { get; private set; }

        public object GetInstance()
        {
            return function();
        }
    }

    #endregion
}

namespace Idioc.Wrappers
{
    public class ServiceLocatorWrapper : IServiceProvider
    {
        readonly Container container;

        public ServiceLocatorWrapper(Container container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }
    }
}

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Idioc.Exceptions
{
    public abstract class IdiocException : Exception
    {
        protected IdiocException(string message, Exception innerException) : base(message, innerException) { }
    }

    public abstract class TypeException : IdiocException
    {
        public Type Type { get; set; }
        protected TypeException(Type type, string message, Exception innerException = null) : base(message, innerException) { Type = type; }
    }

    public class UnregisteredTypeException : TypeException
    {
        internal UnregisteredTypeException(Type type) : base(type, String.Format("Type {0} is not registered with Idioc Container", type.FullName)) { }
    }

    public class TypeNotConstructableException : TypeException
    {
        internal TypeNotConstructableException(Type type) : base(type, String.Format("Type {0} is not constructable", type.FullName)) { }
    }

    public class TypeRegistrationException : TypeException
    {
        public TypeRegistrationException(Type type, string message) : base(type, message) { }
    }
}
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global