using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bite.Compiler;
using Bite.Modules.Callables;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Functions.ForeignInterface;

namespace TestApp
{

public class SampleEventArgs
{
    public string Text { get; set; } // readonly

    #region Public

    public SampleEventArgs( string text )
    {
        Text = text;
    }

    #endregion
}

public class DelegateTest
{
    public delegate object TestDelegate( object sender, SampleEventArgs sampleEventArgs );

    public event TestDelegate OnSampleEvent;

    #region Public

    public void InvokeEvent( object sender, SampleEventArgs sampleEventArgs )
    {
        OnSampleEvent?.Invoke( sender, sampleEventArgs );
    }

    #endregion
}

public class Program
{
    #region Public

    public static void Main( string[] args )
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "FibonacciExample.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();
        
        BiteVm biteVmReciever = new BiteVm();
        BiteVm biteVmSender = new BiteVm();
        
        biteVmReciever.InitVm();
        biteVmSender.InitVm();
        
        DelegateTest delegateTest = new DelegateTest();
        
        ICSharpEvent cSharpEvent =
            new CSharpEvent < DelegateTest.TestDelegate, object, SampleEventArgs >( delegateTest );

        //delegateTest.OnSampleEvent += Test;
        BiteProgram programReciever = null;
        foreach ( string file in files )
        {
            Console.WriteLine( $"File: {file}" );
            List < string > biteProg = new List < string >();
            biteProg.Add( File.ReadAllText( file ) );
            programReciever = compiler.Compile( biteProg );
            
            programReciever.TypeRegistry.RegisterType < SampleEventArgs >();
            programReciever.TypeRegistry.RegisterType < TestClassCSharp >();
            programReciever.TypeRegistry.RegisterType < FSharpTest.Line >();
            programReciever.TypeRegistry.RegisterType (typeof(Console),"Console");
            
            biteVmReciever.RegisterSystemModuleCallables( programReciever.TypeRegistry );
            biteVmReciever.SynchronizationContext = new SynchronizationContext();
            
            biteVmReciever.RegisterExternalGlobalObject( "EventObject", cSharpEvent );
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            biteVmReciever.Interpret( programReciever );
            stopwatch.Stop();
            Console.WriteLine($"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- ");
        }
        
        Console.ReadLine();
    }

    public static object Test( object sender, SampleEventArgs sampleEventArgs )
    {
        Console.WriteLine( sampleEventArgs.Text );

        return sender;
    }

    #endregion
}

}
