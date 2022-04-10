using System.Collections.Generic;

namespace Bite.Symbols
{

public class BiteClassType : Type
{
    private static readonly List < string > s_BiteClassTypes = new List < string >();
    private static int s_ClassTypeIndex = 0;

    public string Name { get; }

    public int TypeIndex => s_ClassTypeIndex;

    #region Public

    public BiteClassType( string typeName )
    {
        Name = typeName;

        if ( s_BiteClassTypes.Contains( typeName ) )
        {
            s_ClassTypeIndex = s_BiteClassTypes.FindIndex( s => s == typeName );
        }
        else
        {
            s_ClassTypeIndex = s_BiteClassTypes.Count;
            s_BiteClassTypes.Add( typeName );
        }
    }

    public override string ToString()
    {
        return $" Type: {Name}";
    }

    #endregion
}

}
