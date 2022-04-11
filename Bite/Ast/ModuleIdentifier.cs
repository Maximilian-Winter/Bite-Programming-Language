using System.Collections.Generic;

namespace Bite.Ast
{

public class ModuleIdentifier : AstBaseNode
{
    public Identifier ModuleId;
    private readonly List < Identifier > ParentModules;

    #region Public

    public ModuleIdentifier()
    {
    }

    public ModuleIdentifier( string id )
    {
        ModuleId = new Identifier( id );
        ParentModules = new List < Identifier >();
    }

    public ModuleIdentifier( string id, List < string > parentModules )
    {
        ModuleId = new Identifier( id );
        ParentModules = new List < Identifier >();

        foreach ( string parentModule in parentModules )
        {
            ParentModules.Add( new Identifier( parentModule ) );
        }
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    public override string ToString()
    {
        string qualifiedName = "";

        foreach ( Identifier parentModule in ParentModules )
        {
            qualifiedName += parentModule.Id + ".";
        }

        qualifiedName += ModuleId.Id;

        return qualifiedName;
    }

    #endregion
}

}
