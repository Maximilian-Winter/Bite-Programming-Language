using System.Collections.Generic;

namespace Srsl_Parser.Runtime
{

public class FastGlobalMemorySpace: FastMemorySpace
{
    private List < FastMemorySpace > m_Modules = new List < FastMemorySpace >();
    public FastGlobalMemorySpace(int memberCount) : base("Global", null, 0, new BinaryChunk(), 0, memberCount)
    {
    }

    public List < FastMemorySpace > Modules
    {
        get => m_Modules;
        set => m_Modules = value;
    }

    public override DynamicSrslVariable Get( string idStr, bool calledFromGlobalMemorySpace = false )
    {
        foreach ( FastMemorySpace fastMemorySpace in m_Modules )
        {
            if ( fastMemorySpace.Exist( idStr, true ) )
            {
                return fastMemorySpace.Get( idStr, true );
            }
        }
        return DynamicVariableExtension.ToDynamicVariable();
    }
}

}
