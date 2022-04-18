using System.Collections.Generic;

namespace Bite.Symbols
{

public class GlobalSymbolTable
{
    private static Dictionary < Symbol, int > GlobalSymbols = new Dictionary < Symbol, int >();
    private static int m_Counter = 0;

    public static void Define( Symbol symbol )
    {
        GlobalSymbols.Add( symbol, m_Counter );
        m_Counter++;
    }
    
    public static int GetIndex( Symbol symbol )
    {
        return GlobalSymbols[symbol];
    }
}

}
