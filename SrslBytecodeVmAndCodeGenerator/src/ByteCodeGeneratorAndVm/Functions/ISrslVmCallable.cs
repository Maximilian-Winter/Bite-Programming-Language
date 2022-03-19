using System.Collections.Generic;
using Srsl_Parser.SymbolTable;

namespace Srsl_Parser.Runtime
{

public interface ISrslVmCallable
{
    object Call( List < DynamicSrslVariable > arguments );
}

}
