using System;
using System.Linq.Expressions;
using NUnit.Framework;

// ReSharper disable UnusedParameter.Local
// ReSharper disable EmptyConstructor
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Idioc.Tests
{
    #region ConstructorExpressionGeneratorTests
    [TestFixture]
    public class ConstructorExpressionGeneratorTests
    {
        
        ExpressionGenerator generator;

        class NoConstructor { }

        class NoArgs { public NoArgs() { } }

        class SingleArg { public SingleArg(NoArgs noArgs) { } }

        class MultiArg { public MultiArg(NoArgs noArgs1, NoArgs noArgs2, NoArgs noArgs3) { } }

        class NonConstructable { private NonConstructable() { } }
        interface IFace {}

        [SetUp]
        public void SetUp()
        {
            generator = new ConstructorExpressionGenerator();
            generator.DependencyExpressionGenerating += (sender, args) => { args.Expression = generator.GenerateExpression(args.DependencyType); };
        }

        [Test]
        public void ShouldCreateExpressionWithNoConstructor()
        {
            var expression = generator.GenerateExpression(typeof(NoConstructor));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOf<NewExpression>(expression);
            Assert.AreEqual(0, ((NewExpression)expression).Arguments.Count);
        }

        [Test]
        public void ShouldCreateExpressionWithNoArgs()
        {
            var expression = generator.GenerateExpression(typeof(NoArgs));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOf<NewExpression>(expression);
            Assert.AreEqual(0, ((NewExpression)expression).Arguments.Count);
        }

        [Test]
        public void ShouldCreateExpressionWithSingleArg()
        {
            var expression = generator.GenerateExpression(typeof(SingleArg));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOf<NewExpression>(expression);
            Assert.AreEqual(1, ((NewExpression)expression).Arguments.Count);
        }

        [Test]
        public void ShouldCreateExpressionWithMultiArgs()
        {
            var expression = generator.GenerateExpression(typeof(MultiArg));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOf<NewExpression>(expression);
            Assert.AreEqual(3, ((NewExpression)expression).Arguments.Count);
        }

        [Test]
        public void ShouldCreateCompilableExpression()
        {
            var expression = generator.GenerateExpression(typeof(MultiArg));

            var func = Expression.Lambda<Func<MultiArg>>(expression).Compile();
            var o = func();
            Assert.IsNotNull(o);
        }

        [Test]
        public void NonConstructableTypeShouldThrow()
        {
            Assert.Throws<TypeNotConstructableException>(() => generator.GenerateExpression(typeof(NonConstructable)));
            Assert.Throws<TypeNotConstructableException>(() => generator.GenerateExpression(typeof(IFace)));
        }
    }
    #endregion

    #region ConstantExpressionGeneratorTests
    [TestFixture]
    public class ConstantExpressionGeneratorTests
    {
        IExpressionGenerator generator;
        private object instance;
        private Expression expression;

        [SetUp]
        public void SetUp()
        {
            instance = new object();
            generator = new ConstantExpressionGenerator(instance);
            expression = generator.GenerateExpression(typeof(object));
        }

        [Test]
        public void ShouldCreateExpressionWithTypeObject()
        {
            Assert.IsNotNull(expression);
            Assert.IsInstanceOf<ConstantExpression>(expression);
            Assert.AreSame(((ConstantExpression)expression).Value, instance);
        }

        [Test]
        public void ShouldCreateCompilableExpression()
        {
            var func = Expression.Lambda<Func<object>>(expression).Compile();
            var o = func();
            Assert.AreSame(o, instance);
        }
    }
    #endregion

    #region LamdaExpressionGeneratorTests
    [TestFixture]
    public class LamdaExpressionGeneratorTests
    {
        class Foo { }

        [Test]
        public void ShouldCreateExpressionWithSimpleLambda()
        {
            var generator = new LambdaExpressionGenerator(() => new Foo());
            var expression = generator.GenerateExpression(typeof(Foo));

            Assert.NotNull(expression);
            var unary = expression as UnaryExpression;
            Assert.IsNotNull(unary);
            Assert.AreEqual(ExpressionType.Convert, unary.NodeType);
        }

        [Test]
        public void ShouldCreateCompilableExpression()
        {
            var generator = new LambdaExpressionGenerator(() => new Foo());
            var expression = generator.GenerateExpression(typeof(Foo));
            var func = Expression.Lambda<Func<Foo>>(expression).Compile();

            var o = func();
            Assert.IsNotNull(o);
        }
    }
    #endregion

    #region ComplexGeneratorTests
    [TestFixture]
    public class ComplexGeneratorTests
    {
        class ServiceFoo { }
        class ServiceBar
        {
            public ServiceFoo Foo { get; private set; }

            public ServiceBar(ServiceFoo foo)
            {
                Foo = foo;
            }
        }

        class Root
        {
            public ServiceFoo Foo { get; private set; }
            public ServiceBar Bar { get; private set; }

            public Root(ServiceFoo foo, ServiceBar bar)
            {
                Foo = foo;
                Bar = bar;
            }
        }

        [Test]
        public void TestComplexExpression()
        {
            var newGenerator = new ConstructorExpressionGenerator();
            int numFoos = 0;
            var fooGenerator = new LambdaExpressionGenerator(() => { numFoos++; return new ServiceFoo(); });
            var singleBar = new ServiceBar(new ServiceFoo());
            var barGenerator = new ConstantExpressionGenerator(singleBar);

            newGenerator.DependencyExpressionGenerating += (sender, args) =>
                {
                    IExpressionGenerator generator;
                    if (args.DependencyType == typeof(ServiceFoo))
                        generator = fooGenerator;
                    else if (args.DependencyType == typeof(ServiceBar))
                        generator = barGenerator;
                    else
                        generator = (IExpressionGenerator)sender;

                    args.Expression = generator.GenerateExpression(args.DependencyType);
                };

            var expression = newGenerator.GenerateExpression(typeof(Root));

            var func = Expression.Lambda<Func<Root>>(expression).Compile();

            var r = func();

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Bar);
            Assert.IsNotNull(r.Foo);
            Assert.Greater(numFoos, 0);
            Assert.AreSame(r.Bar, singleBar);
            Assert.AreSame(r.Bar.Foo, singleBar.Foo);
            Assert.AreNotSame(r.Foo, r.Bar.Foo);
            Assert.AreNotSame(r.Foo, singleBar.Foo);
        }
    }
    #endregion

    #region InstanceProviderTests
    [TestFixture]
    public class InstanceProviderTests
    {
        [Test]
        public void TransientShouldCreateNewObjectEveryTime()
        {
            IExpressionGenerator generator = new ConstructorExpressionGenerator();
            IInstanceProvider provider = new TransientInstanceProvider(typeof(object), generator);

            AssertTransientsAreDifferent(provider.GetInstance(), provider.GetInstance());
        }

        static void AssertTransientsAreDifferent(object o1, object o2)
        {
            Assert.NotNull(o1);
            Assert.NotNull(o2);
            Assert.AreNotSame(o1, o2);
        }

        [Test]
        public void SingleShouldReturnSameObjectEveryTimeFromLambdaExpression()
        {
            IInstanceProvider provider = new SingleInstanceProvider(typeof(object), new LambdaExpressionGenerator(() => new object()));

            var o1 = provider.GetInstance();
            var o2 = provider.GetInstance();

            AssertSinglesAreSame(o1, o2);
        }

        [Test]
        public void SingleShouldReturnSameObjectEveryTimeFromConstantExpression()
        {
            var o = new object();
            IInstanceProvider provider = new SingleInstanceProvider(typeof(object), new ConstantExpressionGenerator(o));

            var o1 = provider.GetInstance();
            var o2 = provider.GetInstance();

            AssertSinglesAreSame(o1, o2);
            AssertSinglesAreSame(o1, o);
            AssertSinglesAreSame(o2, o);
        }

        [Test]
        public void ShouldCreateInstanceOnlyOnce()
        {
            IExpressionGenerator generator = new ConstructorExpressionGenerator();
            IInstanceProvider provider = new TransientInstanceProvider(typeof(object), generator);
            Expression expr = null;
            generator.ExpressionGenerated += (sender, args) => { expr = args.Expression; };

            provider.GetInstance();

            Assert.NotNull(expr);
            Assert.AreSame(expr, provider.Expression);
        }

        static void AssertSinglesAreSame(object o1, object o2)
        {
            Assert.NotNull(o1);
            Assert.NotNull(o2);
            Assert.AreSame(o1, o2);
        }
    }
    #endregion

    #region RegistrationTests
    [TestFixture]
    public class RegistrationTests
    {
        class A { }

        class B
        {
            public A A { get; private set; }

            public B(A a)
            {
                A = a;
            }
        }

        class C
        {
            public B B { get; private set; }

            public C(B b)
            {
                B = b;
            }
        }
        
        [Test]
        public void ShouldRegisterConcreteTypeWithNoDependencies()
        {
            var registration = CreateTransientWithConstructorTypeRegistration(typeof(A));
            var o = registration.GetInstance();
            Assert.NotNull(o);
            Assert.IsInstanceOf<A>(o);
        }


        [Test]
        public void ShouldRegisterConcreteTypeWithSingleDependency()
        {
            var registration = CreateTransientWithConstructorTypeRegistration(typeof(B));
            var o = registration.GetInstance();
            Assert.NotNull(o);
            Assert.IsInstanceOf<B>(o);
            Assert.NotNull(((B)o).A);
        }

        [Test]
        public void ShouldRegisterConcreteTypeWithNestedDependencies()
        {
            var registration = CreateTransientWithConstructorTypeRegistration(typeof(C));
            var o = registration.GetInstance();
            Assert.NotNull(o);
            Assert.IsInstanceOf<C>(o);
            Assert.NotNull(((C)o).B);
            Assert.NotNull(((C)o).B.A);
        }

        static TypeRegistration CreateTransientWithConstructorTypeRegistration(Type concreteType)
        {
            var generator = new ConstructorExpressionGenerator();
            generator.DependencyExpressionGenerating +=
                (sender, args) => { args.Expression = generator.GenerateExpression(args.DependencyType); };

            return new TypeRegistration(concreteType, generator, new TransientInstanceProviderFactory());
        }

        [Test]
        public void ShouldRegisterSingleConcreteTypeWithNoDependencies()
        {
            var registration = CreateSingleWithConstructorTypeRegistration(typeof(A));
            var o1 = registration.GetInstance() as A;
            var o2 = registration.GetInstance() as A;

            Assert.NotNull(o1);
            Assert.NotNull(o2);

            Assert.AreSame(o1, o2);
        }

        [Test]
        public void ShouldRegisterSingleConcreteTypeWithNestedDependencies()
        {
            var registration = CreateSingleWithConstructorTypeRegistration(typeof(C));
            var o1 = registration.GetInstance() as C;
            var o2 = registration.GetInstance() as C;

            Assert.NotNull(o1);
            Assert.NotNull(o2);

            Assert.AreSame(o1, o2);

            Assert.NotNull(o1.B);
            Assert.NotNull(o2.B);

            Assert.AreSame(o1.B, o2.B);

            Assert.NotNull(o1.B.A);
            Assert.NotNull(o2.B.A);

            Assert.AreSame(o1.B.A, o2.B.A);
        }


        static TypeRegistration CreateSingleWithConstructorTypeRegistration(Type concreteType)
        {
            var generator = new ConstructorExpressionGenerator();
            generator.DependencyExpressionGenerating +=
                (sender, args) => { args.Expression = generator.GenerateExpression(args.DependencyType); };

            return new TypeRegistration(concreteType, generator, new SingleInstanceProviderFactory());
        }
    }
    #endregion

    #region Container Tests

    // Let's see if I can wire up a container with just what I've got
    
    [TestFixture]
    public class ContainerTests
    {
        Container container;

        interface IFoo { }

        class Foo : IFoo { }

        class Bar
        {
            public readonly Foo Foo;
            public Bar(Foo foo)
            {
                Foo = foo;
            }
        }

        class Baz
        {

            public readonly IFoo Foo;

            public Baz(IFoo foo)
            {
                Foo = foo;
            }
        }


        [SetUp]
        public void SetUp()
        {
            container = new Container();
        }

        [Test]
        public void ShouldRegisterConcreteTypeToItself()
        {
            container.Register<Foo>();
            var a = container.Resolve<Foo>();
            Assert.NotNull(a);
        }

        [Test]
        public void ShouldRegisterAbstractTypeToConcrete()
        {
            container.Register<IFoo, Foo>();
            var i = container.Resolve<IFoo>();
            Assert.NotNull(i);
        }

        [Test]
        public void ShouldRegisterConcreteTypeWithDependency()
        {
            container.Register<Foo>();
            container.Register<Bar>();
            var b = container.Resolve<Bar>();
            Assert.NotNull(b);
            Assert.NotNull(b.Foo);
        }

        [Test]
        public void ShouldRegisterConcreteTypeWithAbstractDependency()
        {
            container.Register<IFoo, Foo>();
            container.Register<Baz>();
            var b = container.Resolve<Baz>();
            Assert.NotNull(b);
            Assert.NotNull(b.Foo);
        }

        [Test]
        public void ShouldAllowCustomDependencyResolver()
        {
            int numResolved = 0;
            container.DependencyResolver = (sender, args) =>
            {
                // sender is the expression generator
                numResolved++;
                container.ResolveDependencyExpression(sender, args);
            };

            container.Register<IFoo, Foo>();
            container.Register<Baz>();
            var b = container.Resolve<Baz>();
            Assert.NotNull(b);
            Assert.Greater(numResolved, 0);
        }

        [Test]
        public void ShouldRegisterSingleConcreteType()
        {
            container.RegisterSingle<Foo>();
            var foo1 = container.Resolve<Foo>();
            var foo2 = container.Resolve<Foo>();
            Assert.NotNull(foo1);
            Assert.AreSame(foo1, foo2);
        }

        [Test]
        public void ShouldRegisterSingleAbstractTypeToConcrete()
        {
            container.RegisterSingle<IFoo, Foo>();
            var foo1 = container.Resolve<IFoo>();
            var foo2 = container.Resolve<IFoo>();
            Assert.NotNull(foo1);
            Assert.AreSame(foo1, foo2);
        }
    }

    #endregion
}

// ReSharper restore UnusedParameter.Local
// ReSharper restore EmptyConstructor
// ReSharper restore ClassNeverInstantiated.Local
// ReSharper restore MemberHidesStaticFromOuterClass