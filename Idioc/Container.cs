using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Idioc.Exceptions;

namespace Idioc
{
    public class Container
    {
        readonly Dictionary<Type, Registration> registeredTypes = new Dictionary<Type, Registration>();

        /// <summary>
        /// Registers a type to itself
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>()
        {
            Register<T, T>();
        }
        
        /// <summary>
        /// Registers a type for later resolution
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <typeparam name="TResolved">The type to return as the implementation of the requested type</typeparam>
        public void Register<TRequested, TResolved>() where TResolved: TRequested
        {
            RegisterInternal(typeof(TRequested), typeof(TResolved), null);
        }

        /// <summary>
        /// Registers a type to be resolved by a method/lambda call
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <param name="lambda">The lambda/method to use to return the resolved instance</param>
        public void Register<TRequested>(Func<TRequested> lambda)
        {
            var resolver = Expression.Call(lambda.Method.IsStatic ? null : Expression.Constant(lambda.Target), lambda.Method);
            RegisterInternal(typeof(TRequested), typeof(TRequested), resolver);
        }

        /// <summary>
        /// Registers an instance as a singleton for a type
        /// </summary>
        /// <typeparam name="TRequested">The type to register for resolution</typeparam>
        /// <param name="instance">The instance to return as the resolved type</param>
        public void RegisterInstance<TRequested>(TRequested instance)
        {
            var resolver = Expression.Constant(instance);
            RegisterInternal(typeof(TRequested), typeof(TRequested), resolver);
        }
        
        /// <summary>
        /// Performs the actual work of registering a type for resolution
        /// </summary>
        /// <param name="requestedType">The type to register for resolution</param>
        /// <param name="resolvedType">The type to return as the implementation of the requested type</param>
        /// <param name="resolver">The expression to use to resolve the type</param>
        void RegisterInternal(Type requestedType, Type resolvedType, Expression resolver)
        {
            EnsureTypeIsRegisterable(requestedType, resolvedType);
            var resolverExpression = resolver ?? GetConstructorResolverExpression(resolvedType);
            EnsureDependenciesAreRegistered(resolverExpression);
            registeredTypes[requestedType] = new Registration(resolverExpression);
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
            var type = typeof(TRequested);
            if (!IsRegistered(type))
                throw new UnregisteredTypeException(type);
            return (TRequested)GetRegistration(type).ResolverFunction();
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
            if (IsRegistered(forType))
                return GetRegistration(forType).ConstructorInfo;

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
        bool IsRegistered(Type type)
        {
            return registeredTypes.Any(r => r.Key == type || r.Value.ResolvedType == type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Expression GetConstructorResolverExpression(Type type)
        {
            if (IsRegistered(type))
                return GetRegistration(type).ResolverExpression;
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
        Registration GetRegistration(Type type)
        {
            return registeredTypes[type];
        }

        /// <summary>
        /// Stores the information necessary to resolve types
        /// </summary>
        class Registration
        {
            public readonly Type ResolvedType;
            public readonly ConstructorInfo ConstructorInfo;
            public readonly Expression ResolverExpression;
            public readonly Func<object> ResolverFunction;

            /// <summary>
            /// Constructs a new Type Registration
            /// </summary>
            /// <param name="resolverExpression"></param>
            public Registration(Expression resolverExpression)
            {
                ResolvedType = resolverExpression.Type;
                ConstructorInfo = (resolverExpression is NewExpression) ? ((NewExpression)resolverExpression).Constructor : null;
                ResolverExpression = resolverExpression;
                ResolverFunction = Expression.Lambda<Func<object>>(resolverExpression).Compile();
            }
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