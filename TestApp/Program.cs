using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
            "Mandelbrot.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();

        BiteVm biteVm = new BiteVm();

        biteVm.InitVm();

        DelegateTest delegateTest = new DelegateTest();

        ICSharpEvent cSharpEvent =
            new CSharpEvent < DelegateTest.TestDelegate, object, SampleEventArgs >( delegateTest );

        //delegateTest.OnSampleEvent += Test;
        foreach ( string file in files )
        {
            Console.WriteLine( $"File: {file}" );
            List < string > biteProg = new List < string >();
            biteProg.Add( File.ReadAllText( file ) );
            BiteProgram biteProgram = compiler.Compile( biteProg );

            biteProgram.TypeRegistry.RegisterType < SampleEventArgs >();
            biteProgram.TypeRegistry.RegisterType < TestClassCSharp >();
            biteProgram.TypeRegistry.RegisterType < Bitmap >();
            biteProgram.TypeRegistry.RegisterType < ImageFormat >();
            biteProgram.TypeRegistry.RegisterType < Color >();
            // biteProgram.TypeRegistry.RegisterType < FSharpTest.Line >();
            biteProgram.TypeRegistry.RegisterType( typeof( Console ), "Console" );

            biteVm.RegisterSystemModuleCallables( biteProgram.TypeRegistry );
            biteVm.SynchronizationContext = new SynchronizationContext();
            biteVm.RegisterExternalGlobalObject( "EventObject", cSharpEvent );

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            biteVm.Interpret( biteProgram );
            stopwatch.Stop();

            Console.WriteLine( $"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- " );
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
