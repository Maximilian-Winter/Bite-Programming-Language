using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bite.Compiler;
using Bite.Modules.Callables;
using Bite.Runtime;
using Bite.Runtime.Bytecode;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace TestApp
{
public class SampleEventArgs
{
    public SampleEventArgs(string text) { Text = text; }
    public string Text { get; } // readonly
}
public class DelegateTest
{
    public void InvokeEvent(object sender, SampleEventArgs sampleEventArgs)
    {
        OnSampleEvent?.Invoke( sender, sampleEventArgs );
    }
    public delegate object TestDelegate( object sender, SampleEventArgs sampleEventArgs );

    public event TestDelegate OnSampleEvent;
}
public class Program
{
    #region Public

   

    public static object Test( object sender, SampleEventArgs sampleEventArgs )
    {
        Console.WriteLine(sampleEventArgs.Text);
        return sender;
    }
    
    public static void Main( string[] args )
    {
        
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "MainModule.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
        DelegateTest delegateTest = new DelegateTest();
        foreach ( string file in files )
        {
            Console.WriteLine($"File: {file}");
            List < string > biteProg = new List < string >();
            biteProg.Add( File.ReadAllText( file ) );
            BiteProgram program = compiler.Compile(biteProg);

            program.TypeRegistry.RegisterType<TestClassCSharp>();
            biteVm.RegisterSystemModuleCallables(program.TypeRegistry);
            biteVm.SynchronizationContext = new SynchronizationContext();
            
            biteVm.RegisterExternalGlobalObject( "EventObject", new EventWrapper(delegateTest) );
            delegateTest.OnSampleEvent += Test;
            if ( program != null )
            {
                Task.Run(
                    () =>
                    {
                        biteVm.Interpret( program );
                    } ).ContinueWith(t=>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine(t.Exception.InnerException.Message);
                    }
                });

                while ( true )
                {
                    string line = Console.ReadLine();

                    if ( line == "exit" )
                    {
                        break;
                    }
                    
                    delegateTest.InvokeEvent( new object(), new SampleEventArgs( line ));
                }
             
                
                /*int k = 1;
                long elapsedMillisecondsAccu = 0;
            
                for ( int i = 0; i < k; i++ )
                {
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
                    program.Run();
                    stopwatch2.Stop();
                    Console.WriteLine( "--Elapsed Time for Interpreting Run {0} is {1} ms", i, stopwatch2.ElapsedMilliseconds );
                    elapsedMillisecondsAccu += stopwatch2.ElapsedMilliseconds;
                }
    
                Console.WriteLine( "--Average Elapsed Time for Interpreting per Run is {0} ms", elapsedMillisecondsAccu / k );
                Console.WriteLine( "--Total Elapsed Time for Interpreting {0} Runs is {1} ms", k, elapsedMillisecondsAccu );
    
                IOrderedEnumerable < KeyValuePair < string, long > > sortedDict =
                    from entry in ChunkDebugHelper.InstructionCounter orderby entry.Value descending select entry;
    
                long totalInstructions = 0;
    
                foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
                {
                    totalInstructions += keyValuePair.Value;
                }
    
                foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
                {
                    Console.WriteLine(
                        "--Instruction Count for Instruction {0}: {2}     {1}%",
                        keyValuePair.Key,
                        ( 100.0 / totalInstructions * keyValuePair.Value ).ToString( "00.0" ),
                        keyValuePair.Value );
                }
    
                ChunkDebugHelper.InstructionCounter.Clear();*/

            }
        }

        Console.ReadLine();

    }

    #endregion
}

}
