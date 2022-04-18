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

namespace TestMandelbrot
{

internal class Program
{
    public static void Main( string[] args )
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\",
            "Mandelbrot.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();

        BiteVm biteVmReciever = new BiteVm();

        biteVmReciever.InitVm();

        BiteProgram programReciever = null;

        foreach ( string file in files )
        {
            Console.WriteLine( $"File: {file}" );
            List < string > biteProg = new List < string >();
            biteProg.Add( File.ReadAllText( file ) );
            programReciever = compiler.Compile( biteProg );

            programReciever.TypeRegistry.RegisterType < Bitmap >();
            programReciever.TypeRegistry.RegisterType < ImageFormat >();
            programReciever.TypeRegistry.RegisterType < Color >();

            biteVmReciever.RegisterSystemModuleCallables( programReciever.TypeRegistry );
            biteVmReciever.SynchronizationContext = new SynchronizationContext();
            biteVmReciever.TypeRegistry = programReciever.TypeRegistry;
            Task.Run(
                     () =>
                     {
                         Stopwatch stopwatch = new Stopwatch();
                         stopwatch.Start();
                         biteVmReciever.Interpret( programReciever );
                         stopwatch.Stop();

                         Console.WriteLine(
                             $"--- Elapsed Time Interpreting in Milliseconds: {stopwatch.ElapsedMilliseconds}ms --- " );
                     } ).
                 ContinueWith(
                     t =>
                     {
                         if ( t.IsFaulted )
                         {
                             Console.WriteLine( t.Exception.InnerException.Message );
                         }
                     } );
        }

        Console.ReadLine();
    }
}

}
