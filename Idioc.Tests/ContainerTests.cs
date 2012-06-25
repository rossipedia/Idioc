using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Idioc.Exceptions;
using NUnit.Framework;

namespace Idioc.Tests
{
    [TestFixture]
    public class ContainerTests
    {
        #region Private Definitions

        Container container;

        #endregion

        [SetUp]
        public void SetupTests()
        {
            container = new Container();
        }
        
        [Test]
        public void CannotRegisterPrimitiveType()
        {
            Assert.That(() => container.Register<int>(), Throws.InstanceOf<TypeNotConstructableException>().With.Property("Type").SameAs(typeof(int)));
        }

        [Test]
        public void CanRegisterClassType()
        {
            container.Register<A>();
        }

        [Test]
        public void ResolveOfUnregisteredThrows()
        {
            Assert.Throws<UnregisteredTypeException>(() => container.Resolve<A>());
        }

        [Test]
        public void ResolveOfUnregisteredAfterAnotherTypeHasRegisteredThrows()
        {
            container.Register<A>();
            Assert.Throws<UnregisteredTypeException>(() => container.Resolve<B>());
        }

        [Test]
        public void ResolveOfRegisteredReturnsInstance()
        {
            container.Register<A>();
            var a = container.Resolve<A>();
            Assert.IsNotNull(a);
        }

        [Test]
        public void CanRegisterMultipleTypes()
        {
            container.Register<A>();
            container.Register<B>();
        }

        [Test]
        public void CanResolveMultipleTypes()
        {
            container.Register<A>();
            container.Register<B>();

            var a = container.Resolve<A>();
            var b = container.Resolve<B>();

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
        }

        [Test]
        public void CanResolveTypeWithDependencies()
        {
            container.Register<A>();
            container.Register<C>();
            var c = container.Resolve<C>();
            Assert.IsNotNull(c);
            Assert.IsNotNull(c.A);
        }

        [Test]
        public void MultipleResolutionsReturnDifferentInstances()
        {
            container.Register<A>();

            var a1 = container.Resolve<A>();
            var a2 = container.Resolve<A>();

            Assert.AreNotSame(a1, a2);
        }

        [Test]
        public void ResolveTypeWithUnregisteredConstructorArgumentThrows()
        {
            Assert.That(() => container.Register<C>(), Throws.InstanceOf<UnregisteredTypeException>().With.Property("Type").SameAs(typeof(A)));
        }
        
        [Test]
        public void CanRegisterTypeWithSingleConstructorArgument()
        {
            container.Register<A>();
            container.Register<C>();
        }

        [Test]
        public void DuplicateTypeRegistrationThrows()
        {
            container.Register<A>();
            Assert.That(() => container.Register<A>(), Throws.InstanceOf<TypeRegistrationException>().With.Property("Type").SameAs(typeof(A)));
        }

        [Test]
        public void ConstructorExpressionWithSingleArgumentHasCorrectType()
        {
            var newExpression = container.GetConstructorResolverExpression(typeof(C)) as NewExpression;
            Assert.IsNotNull(newExpression);
            Assert.AreEqual(1, newExpression.Arguments.Count);

            var argType = newExpression.Arguments[0].Type;
            Assert.AreSame(typeof(A), argType);
        }

        [Test]
        public void ConstructorExpressionWithSingleArgumentCreatesNewInstance()
        {
            var newExpression = container.GetConstructorResolverExpression(typeof(C));
            var creatorFunction = Expression.Lambda<Func<object>>(newExpression).Compile();
            var c = (C)creatorFunction();
            Assert.IsNotNull(c);
            Assert.IsNotNull(c.A);
        }

        [Test]
        public void ConstructorExpressionWithSingleArgumentCreatesNewInstanceTwice()
        {
            var newExpression = container.GetConstructorResolverExpression(typeof(C));
            var creatorFunction = Expression.Lambda<Func<object>>(newExpression).Compile();
            var c1 = (C)creatorFunction();
            var c2 = (C)creatorFunction();
            Assert.AreNotSame(c1, c2);
            Assert.AreNotSame(c1.A, c2.A);
        }

        [Test]
        public void RegisterTypeWithUnregisteredNestedDependencyThrows()
        {
            Assert.That(() => container.Register<D>(), Throws.InstanceOf<UnregisteredTypeException>().With.Property("Type").SameAs(typeof(C)));
        }

        [Test]
        public void CanRegisterTypeWithNestedDependency()
        {
            container.Register<A>();
            container.Register<C>();
            container.Register<D>();
        }

        [Test]
        public void ConstructorExpressionsForSameRegisteredDependencyAreSameExpressions()
        {
            container.Register<A>();

            var newAExpression = container.GetConstructorResolverExpression(typeof(A)) as NewExpression;
            var newCExpression = container.GetConstructorResolverExpression(typeof(C)) as NewExpression;
            var newEExpression = container.GetConstructorResolverExpression(typeof(E)) as NewExpression;

            Assert.IsNotNull(newCExpression);
            Assert.IsNotNull(newEExpression);

            Assert.AreSame(newCExpression.Arguments[0], newAExpression);
            Assert.AreSame(newEExpression.Arguments[0], newAExpression);
        }

        [Test]
        public void CanCreateObjectWithNoDependenciesFromConstructFunction()
        {
            var newAExpression = container.GetConstructorResolverExpression(typeof(A));
            var createFunc = Expression.Lambda<Func<A>>(newAExpression).Compile();
            var a = createFunc();
            Assert.IsNotNull(a);
        }

        [Test]
        public void CanCreateObjectAndDependenciesFromConstructFunction()
        {
            var newDExpression = container.GetConstructorResolverExpression(typeof(D));
            var createFunc = Expression.Lambda<Func<D>>(newDExpression).Compile();
            var d = createFunc();
            Assert.IsNotNull(d);
            Assert.IsNotNull(d.C);
            Assert.IsNotNull(d.C.A);
        }

        [Test]
        public void CanRegisterConcreteForInterface()
        {
            container.Register<IA, A>();
        }

        [Test]
        public void CanRegisterConcreteForInterfaceWeak()
        {
            container.Register(typeof(IA), typeof(A));
        }

        [Test]
        public void CanResolveInterfaceToConcrete()
        {
            container.Register<IA, A>();
            var a = container.Resolve<IA>();
            Assert.IsNotNull(a);
            Assert.IsInstanceOf<A>(a);
        }

        [Test]
        public void CanResolveInterfaceDependency()
        {
            container.Register<IA, A>();
            container.Register<F>();
            var f = container.Resolve<F>();
            Assert.IsNotNull(f);
            Assert.IsNotNull(f.A);
            Assert.IsInstanceOf<A>(f.A);
        }

        [Test]
        public void CanResolveToLambda()
        {
            int i = 0;
            container.Register<IA>(() => { i++; return new A(); });
            var a = container.Resolve<IA>();
            Assert.IsNotNull(a);
            Assert.IsInstanceOf<A>(a);
            Assert.AreEqual(1, i);
        }

        [Test]
        public void CanResolveToInstance()
        {
            var a = new A();
            container.RegisterSingle<IA>(a);

            var a2 = container.Resolve<IA>();
            Assert.AreSame(a, a2);
        }

        //[Test]
        //public void SingleRegistrationWithoutSuppliedInstanceReturnsSame()
        //{
        //    container.RegisterSingle<A>();

        //    var a1 = container.Resolve<A>();
        //    var a2 = container.Resolve<A>();

        //    Assert.AreSame(a1, a2);
        //}

        [Test]
        public void CanResolveToNullInstance()
        {
            A a = null;
            container.RegisterSingle<IA>(a);
            var a2 = container.Resolve<IA>();
            Assert.IsNull(a2);
        }
    }

    [TestFixture]
    public class InstanceResolverTests
    {
        Expression creatorExpression;

        [SetUp]
        public void SetupTests()
        {
            creatorExpression = Expression.New(typeof(A).GetConstructor(new Type[0]));
        }

        [Test]
        public void TransientResolverReturnsDifferentInstances()
        {
            var resolver = new TransientResolver(creatorExpression);

            var a1 = (A)resolver.GetInstance();
            var a2 = (A)resolver.GetInstance();

            Assert.AreNotSame(a1, a2);
        }

        [Test]
        public void SingleResolverReturnsSameInstances()
        {
            var resolver = new SingleResolver(creatorExpression);


            var a1 = (A)resolver.GetInstance();
            var a2 = (A)resolver.GetInstance();

            Assert.AreSame(a1, a2);
        }
    }

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedParameter.Local

    interface IA { }
    
    class A : IA { }

    class B { }

    class C
    {
        public readonly A A;
        public C(A a) { A = a; }
    }

    class D
    {
        public readonly C C;
        public D(C c) { C = c; }
    }

    class E
    {
        public E(A a) {}
    }

    class F
    {
        public readonly IA A;
        public F(IA a) { A = a; }
    }

    // ReSharper restore UnusedParameter.Local
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore ClassNeverInstantiated.Global
}
