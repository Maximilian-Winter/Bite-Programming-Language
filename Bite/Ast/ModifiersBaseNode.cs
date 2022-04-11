using System.Collections.Generic;

namespace Bite.Ast
{

public class ModifiersBaseNode : AstBaseNode
{
    public enum ModifierTypes
    {
        DeclareExtern,
        DeclareCallable,
        DeclarePrivate,
        DeclarePublic,
        DeclareAbstract,
        DeclareStatic
    }

    public List < ModifierTypes > Modifiers;

    #region Public

    public ModifiersBaseNode( string accessMod, string staticAbstractMod )
    {
        Modifiers = new List < ModifierTypes >();

        if ( accessMod != null && accessMod == "public" )
        {
            Modifiers.Add( ModifierTypes.DeclarePublic );
        }
        else
        {
            Modifiers.Add( ModifierTypes.DeclarePrivate );
        }

        if ( staticAbstractMod != null && staticAbstractMod == "static")
        {
            Modifiers.Add( ModifierTypes.DeclareStatic );
        }
        else if ( staticAbstractMod != null && staticAbstractMod == "abstract" )
        {
            Modifiers.Add( ModifierTypes.DeclareAbstract );
        }
        
    }

    public ModifiersBaseNode( string accessMod, string staticAbstractMod, bool isExtern, bool isCallable )
    {
        Modifiers = new List<ModifierTypes>();

        if (accessMod != null && accessMod == "public")
        {
            Modifiers.Add( ModifierTypes.DeclarePublic );
        }
        else
        {
            Modifiers.Add( ModifierTypes.DeclarePrivate );
        }

        if (staticAbstractMod != null && staticAbstractMod == "static")
        {
            Modifiers.Add( ModifierTypes.DeclareStatic );
        }
        else if (staticAbstractMod != null && staticAbstractMod == "abstract")
        {
            Modifiers.Add( ModifierTypes.DeclareAbstract );
        }

        if (isExtern)
        {
            Modifiers.Add( ModifierTypes.DeclareExtern );
        }

        if (isCallable)
        {
            Modifiers.Add( ModifierTypes.DeclareCallable );
        }

    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
