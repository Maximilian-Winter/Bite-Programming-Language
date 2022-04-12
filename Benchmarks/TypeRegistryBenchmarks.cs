using BenchmarkDotNet.Attributes;
using Bite.Runtime.Functions.ForeignInterface;

namespace Benchmarks
{

public class TypeRegistryBenchmarks
{
    private readonly TypeRegistry typeRegistry = new TypeRegistry();

    public TypeRegistryBenchmarks()
    {
        typeRegistry.RegisterType < TestClass >();
    }


    [Benchmark]
    public void RunNative()
    {
        var instance = new TestClass();
        instance.TestMethod( "Hello", 1, 2f );
    }

    [Benchmark]
    public void RunGetMethod()
    {
        var type = System.Type.GetType( "Benchmarks.TestClass, Benchmarks" );
        var method = type.GetMethod( "TestMethod", new[] { typeof( string ), typeof( int ), typeof( float ) } );
        var instance = new TestClass();
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

    [Benchmark]
    public void RunCachedGetmethod()
    {
        typeRegistry.TryResolveType( "TestClass", out System.Type type );

        var method = typeRegistry.GetMethod( type,
            "TestMethod",
            new[] { typeof( string ), typeof( int ), typeof( float ) } );

        var instance = new TestClass();
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

}

}