using Bite.Runtime;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;

namespace Bite.Modules.Callables
{

public static class SystemModule
{
    public static void RegisterSystemModuleCallables( this BiteVm biteVm, TypeRegistry typeRegistry = null )
    {
        biteVm.RegisterCallable( "CSharpInterfaceCall", new ForeignLibraryInterfaceVm( typeRegistry ) );
        biteVm.RegisterCallable( "Print", new PrintFunctionVm() );
        biteVm.RegisterCallable( "PrintLine", new PrintLineFunctionVm() );
    }
}

}
