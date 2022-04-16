using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Bite.Runtime.Functions.ForeignInterface;

namespace Benchmarks
{

public class TypeRegistryBenchmarks
{
    private readonly TypeRegistry typeRegistry = new TypeRegistry();

    #region Public

    public TypeRegistryBenchmarks()
    {
        typeRegistry.RegisterType < TestClass >();
    }

    [Benchmark]
    public void RunCachedGetmethod()
    {
        typeRegistry.TryResolveType( "TestClass", out Type type );

        MethodInfo method = typeRegistry.GetMethod(
            type,
            "TestMethod",
            new[] { typeof( string ), typeof( int ), typeof( float ) } );

        TestClass instance = new TestClass();
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

    [Benchmark]
    public void RunGetMethod()
    {
        Type type = Type.GetType( "Benchmarks.TestClass, Benchmarks" );
        MethodInfo method = type.GetMethod( "TestMethod", new[] { typeof( string ), typeof( int ), typeof( float ) } );
        TestClass instance = new TestClass();
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

    [Benchmark]
    public void RunNative()
    {
        TestClass instance = new TestClass();
        instance.TestMethod( "Hello", 1, 2f );
    }

    #endregion
}

}
