using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Functions.Interop;
using Bite.Runtime.Memory;

namespace Benchmarks
{

public class MethodInvocationBenchmarks
{
    private readonly TypeRegistry typeRegistry = new TypeRegistry();
    private readonly ForeignLibraryInterfaceVm fli;
    private readonly InteropGetMethod interop;
    private readonly MethodInvoker m_MethodInvoker;
    private readonly TestClass instance;
    private readonly MethodInfo method;

    #region Public

    public MethodInvocationBenchmarks()
    {
        typeRegistry.RegisterType < TestClass >();
        fli = new ForeignLibraryInterfaceVm( typeRegistry );
        interop = new InteropGetMethod( typeRegistry );
        instance = new TestClass();

        Type type = Type.GetType( "Benchmarks.TestClass, Benchmarks" );
        method = type.GetMethod( "TestMethod", new[] { typeof( string ), typeof( int ), typeof( float ) } );

        m_MethodInvoker = ( MethodInvoker ) interop.Call( new DynamicBiteVariable[]
        {
            new DynamicBiteVariable( "TestClass" ),
            new DynamicBiteVariable( "TestMethod" ),
            new DynamicBiteVariable( "string" ),
            new DynamicBiteVariable( "int" ),
            new DynamicBiteVariable( "float" ),
        } );
    }

    [Benchmark]
    public void RunForeignLibraryInterfaceVm()
    {
        fli.Call( new DynamicBiteVariable[]
        {
            new DynamicBiteVariable( instance ),
            new DynamicBiteVariable( "TestMethod" ),
            new DynamicBiteVariable( "Hello" ),
            new DynamicBiteVariable( "string" ),
            new DynamicBiteVariable( 1 ),
            new DynamicBiteVariable( "int" ),
            new DynamicBiteVariable( 2f ),
            new DynamicBiteVariable( "float" ),
        } );
    }

    [Benchmark]
    public void RunReflection()
    {
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

    [Benchmark]
    public void RunInteropGetMethod()
    {
        m_MethodInvoker.Call( new DynamicBiteVariable[]
        {
            new DynamicBiteVariable( instance ),
            new DynamicBiteVariable( "Hello" ),
            new DynamicBiteVariable( 1 ),
            new DynamicBiteVariable( 2f ),
        } );
    }


    [Benchmark]
    public void RunNative()
    {
        instance.TestMethod( "Hello", 1, 2f );
    }

    #endregion
}

}
