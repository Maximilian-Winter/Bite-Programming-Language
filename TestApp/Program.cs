using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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

public class BitmapEx
{
    private readonly Bitmap tempBitmap;
    private BitmapData tempData;
    private int tempNumBytes;
    private byte[] tempRgbValues;

    public BitmapEx( int width, int height )
    {
        tempBitmap = new Bitmap( width, height );
    }

    public void Lock()
    {
        var rect = new Rectangle( 0, 0, tempBitmap.Width, tempBitmap.Height );
        tempData = tempBitmap.LockBits( rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb );
        IntPtr ptr = tempData.Scan0;
        tempNumBytes = tempData.Stride * tempData.Height;
        tempRgbValues = new byte[tempNumBytes];
        Marshal.Copy( ptr, tempRgbValues, 0, tempNumBytes );
    }

    public void SetPixel( int x, int y, Color color )
    {

        var tempPos = x * 4 + y * tempData.Stride;

        if ( tempPos < tempNumBytes )
        {
            tempRgbValues[tempPos] = color.B;
            tempRgbValues[tempPos + 1] = color.G;
            tempRgbValues[tempPos + 2] = color.R;
            tempRgbValues[tempPos + 3] = color.A;
        }

        tempBitmap.UnlockBits( tempData );
    }

    public void Unlock()
    {
        tempBitmap.UnlockBits( tempData );
    }

    public void Save( string fileName, ImageFormat format )
    {
        tempBitmap.Save( fileName, format );
    }

}

public class Program
{
    #region Public

    public static void Main( string[] args )
    {
        Test( "TestProgram\\Interop.bite",
            new[]
            {
                typeof( Bitmap ),
                typeof( ImageFormat ),
                typeof( Color ),
            } );

        Console.ReadLine();
    }

    public static object Test( object sender, SampleEventArgs sampleEventArgs )
    {
        Console.WriteLine( sampleEventArgs.Text );

        return sender;
    }

    public static void Test2()
    {

        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "CSharpEventReceiverExample.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();

        BiteVm biteVmReciever = new BiteVm();
        BiteVm biteVmSender = new BiteVm();

        biteVmReciever.InitVm();
        biteVmSender.InitVm();

        DelegateTest delegateTest = new DelegateTest();

        ICSharpEvent cSharpEvent =
            new CSharpEvent < DelegateTest.TestDelegate, object, SampleEventArgs >( delegateTest );

        //        //delegateTest.OnSampleEvent += Test;
        //        BiteProgram programReciever = null;
        //        foreach ( string file in files )
        //        {
        //            Console.WriteLine( $"File: {file}" );
        //            List < string > biteProg = new List < string >();
        //            biteProg.Add( File.ReadAllText( file ) );
        //            programReciever = compiler.Compile( biteProg );

        //            programReciever.TypeRegistry.RegisterType < SampleEventArgs >();
        //            programReciever.TypeRegistry.RegisterType < TestClassCSharp >();
        //#if fsharp
        //            programReciever.TypeRegistry.RegisterType < FSharpTest.Line >();
        //#endif
        //            programReciever.TypeRegistry.RegisterType (typeof(Console),"Console");

        //            biteVmReciever.RegisterSystemModuleCallables( programReciever.TypeRegistry );
        //            biteVmReciever.SynchronizationContext = new SynchronizationContext();

        //            biteVmReciever.RegisterExternalGlobalObject( "EventObject", cSharpEvent );

        //            Task.Run(
        //                     () =>
        //                     {
        //                         Stopwatch stopwatch = new Stopwatch();
        //                         stopwatch.Start();
        //                         biteVmReciever.Interpret( programReciever );
        //                         stopwatch.Stop();
        //                         Console.WriteLine($"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- ");
        //                     } ).
        //                 ContinueWith(
        //                     t =>
        //                     {
        //                         if ( t.IsFaulted )
        //                         {
        //                             Console.WriteLine( t.Exception.InnerException.Message );
        //                         }
        //                     } );
        //        }


        files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "Mandelbrot.bite",
            SearchOption.AllDirectories );

        BiteProgram programSender = null;

        foreach ( string file in files )
        {

        }

        while ( true )
        {
            string line = Console.ReadLine();

            if ( line == "exit" )
            {
                break;
            }

            delegateTest.InvokeEvent( new object(), new SampleEventArgs( line ) );
        }

    }

    public static void Test( string file, IEnumerable <Type> types )
    {
        BiteCompiler compiler = new BiteCompiler();

        BiteVm biteVm = new BiteVm();

        biteVm.InitVm();

        Console.WriteLine( $"File: {file}" );
        List < string > biteProg = new List < string >();
        biteProg.Add( File.ReadAllText( file ) );
        BiteProgram program = compiler.Compile( biteProg );

        foreach ( var type in types)
        {
            program.TypeRegistry.RegisterType( type );
        }

        biteVm.RegisterSystemModuleCallables( program.TypeRegistry );

        try
        {
            Console.WriteLine( $"Running {file}" );
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            biteVm.Interpret( program );
            stopwatch.Stop();
            Console.WriteLine( $"{file} completed in {stopwatch.ElapsedMilliseconds}" );
        }
        catch (Exception e)
        {
            Console.WriteLine( e.Message );
        }

        Console.ReadLine();
    }

    #endregion
}

}
