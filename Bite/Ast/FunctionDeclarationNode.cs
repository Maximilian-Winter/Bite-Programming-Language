namespace Bite.Ast
{

public class FunctionDeclarationNode : DeclarationNode
{
    public Identifier FunctionId;
    public ModifiersNode Modifiers;
    public ParametersNode Parameters;
    public BlockStatementNode FunctionBlock;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
