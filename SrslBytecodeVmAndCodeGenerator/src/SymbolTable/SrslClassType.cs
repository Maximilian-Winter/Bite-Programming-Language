using System.Collections.Generic;

namespace Srsl_Parser.SymbolTable
{

public class SrslClassType : Type
{
    private static readonly List < string > s_SrslClassTypes = new List < string >();
    private static int s_ClassTypeIndex = 0;

    public string Name { get; }

    public int TypeIndex => s_ClassTypeIndex;

    #region Public

    public SrslClassType( string typeName )
    {
        Name = typeName;

        if ( s_SrslClassTypes.Contains( typeName ) )
        {
            s_ClassTypeIndex = s_SrslClassTypes.FindIndex( s => s == typeName );
        }
        else
        {
            s_ClassTypeIndex = s_SrslClassTypes.Count;
            s_SrslClassTypes.Add( typeName );
        }
    }

    public override string ToString()
    {
        return $" Type: {Name}";
    }

    #endregion
}

}
