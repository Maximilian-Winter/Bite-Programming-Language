using Bite.Runtime;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;

namespace Bite.Modules.Callables
{

public static class SystemModule
{
    #region Public

    public static void RegisterSystemModuleCallables( this BiteVm biteVm, TypeRegistry typeRegistry = null )
    {
        biteVm.RegisterCallable( "NetLanguageInterface", new ForeignLibraryInterfaceVm( typeRegistry ) );
        biteVm.RegisterCallable( "Print", new PrintFunctionVm() );
        biteVm.RegisterCallable( "PrintLine", new PrintLineFunctionVm() );
    }

    #endregion
}

}
