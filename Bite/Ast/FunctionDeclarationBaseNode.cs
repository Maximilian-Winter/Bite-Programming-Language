namespace Bite.Ast
{

public class FunctionDeclarationBaseNode : DeclarationBaseNode
{
    public Identifier FunctionId;
    public Identifier LinkFunctionId;
    public ModifiersBaseNode ModifiersBase;
    public ParametersBaseNode ParametersBase;
    public BlockStatementBaseNode FunctionBlock;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
