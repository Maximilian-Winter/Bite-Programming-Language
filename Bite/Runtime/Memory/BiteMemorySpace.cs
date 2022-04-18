using System.Collections.Generic;

namespace Bite.Runtime.Memory
{

public class BiteMemorySpace
{
    private Dictionary < string, DynamicBiteVariable > m_FunctionByName =
        new Dictionary < string, DynamicBiteVariable >();
    
    private Dictionary < string, DynamicBiteVariable > m_ClassInstanceByName =
        new Dictionary < string, DynamicBiteVariable >();


    private DynamicBiteVariable[] m_Properties;

    public BiteMemorySpace(int size)
    {
        m_Properties = new DynamicBiteVariable[size];
    }

    public void DefineVariable( int i, DynamicBiteVariable dynamicBiteVariable )
    {
        
    }
}

}
