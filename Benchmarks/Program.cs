using BenchmarkDotNet.Running;

namespace Benchmarks
{

internal class Program
{
    #region Private

    private static void Main( string[] args )
    {
        //var b = new Benchmarks();
        // BenchmarkRunner.Run < Benchmarks >();
        //BenchmarkRunner.Run < TypeRegistryBenchmarks >();
        //BenchmarkRunner.Run < PropertyAccessBenchmarks >();
        BenchmarkRunner.Run < MethodInvocationBenchmarks >();
        //var b = new MethodInvocationBenchmarks();
        //b.RunForeignLibraryInterfaceVm();
    }

        #endregion
    }

}
