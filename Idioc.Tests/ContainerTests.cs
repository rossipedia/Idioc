using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Idioc.Tests
{
    [TestFixture]
    public class ConstructorExpressionGeneratorTests
    {
        IExpressionGenerator generator;

        class NoConstructor { }

        class NoArgs { public NoArgs() { } }

        class SingleArg { public SingleArg(NoArgs noArgs) { } }

        class MultiArg { public MultiArg(NoArgs noArgs1, NoArgs noArgs2, NoArgs noArgs3) { } }

        [SetUp]
        public void SetUp()
        {
            generator = new ConstructorExpressionGenerator(new MostSpecificConstructorSelector());
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
    }

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

    [TestFixture]
    public class LamdaExpressionGeneratorTests
    {
        class Foo {}

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
            var newGenerator = new ConstructorExpressionGenerator(new MostSpecificConstructorSelector());
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
}
