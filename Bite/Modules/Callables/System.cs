using Bite.Runtime;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Functions.Interop;

namespace Bite.Modules.Callables
{

public static class SystemModule
{
    #region Public

    public static void RegisterSystemModuleCallables( this BiteVm biteVm, TypeRegistry typeRegistry = null )
    {
        biteVm.RegisterCallable( "GetConstructor", new InteropGetConstructor( typeRegistry ) );
        biteVm.RegisterCallable( "GetStaticMember", new InteropGetStaticMember( typeRegistry ) );
        biteVm.RegisterCallable( "GetStaticMethod", new InteropGetStaticMethod( typeRegistry ) );
        biteVm.RegisterCallable( "GetMethod", new InteropGetMethod( typeRegistry ) );
        biteVm.RegisterCallable( "GetStaticClass", new InteropGetStaticClass( typeRegistry ) );
        biteVm.RegisterCallable( "GetGenericMethod", new InteropGetGenericMethod( typeRegistry ) );
        biteVm.RegisterCallable( "Print", new PrintFunctionVm() );
        biteVm.RegisterCallable( "PrintLine", new PrintLineFunctionVm() );
    }

    #endregion
}

}
