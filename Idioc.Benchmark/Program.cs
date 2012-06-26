namespace Idioc.Benchmark
{
    using System;
    using System.Diagnostics;

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable UnusedParameter.Local

    public interface IA { }
    public interface IB { }
    public interface IC { }

    public class A : IA { }
    public class B : IB { }

    public class C : IC { public C(IA a, IB b) { } }

    // ReSharper restore UnusedParameter.Local
    // ReSharper restore ClassNeverInstantiated.Global

    static class Program
    {
        static void Main()
        {
            RunIdiocTests();

            Console.ReadKey();
        }
        
        static void RunIdiocTests()
        {
            var container = new Container();

            var a = new A();
            container.RegisterSingle<IA>(a);
            container.Register<IB, B>();
            container.Register<IC, C>();

            RunIdiocTest<IA>("Single", container);
            RunIdiocTest<IB>("Transient", container);
            RunIdiocTest<IC>("Combined", container);


        }

        static void RunIdiocTest<T>(string label, Container container)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; ++i)
                container.Resolve<T>();
            watch.Stop();
            Console.WriteLine("{0}: {1}ms", label, watch.ElapsedMilliseconds);
        }
    }
}
