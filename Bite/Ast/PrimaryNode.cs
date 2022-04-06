using System.Collections.Generic;

namespace Bite.Ast
{

public class InterpolatedStringPart
{
    public string TextBeforeExpression;
    public ExpressionNode ExpressionNode;

    public InterpolatedStringPart( string textBeforeExpression, ExpressionNode expressionNode )
    {
        TextBeforeExpression = textBeforeExpression;
        ExpressionNode = expressionNode;
    }
}
public class InterpolatedString : ExpressionNode
{
    public List < InterpolatedStringPart > StringParts;
    public string TextAfterLastExpression;
}
public class PrimaryNode : ExpressionNode
{
    public enum PrimaryTypes
    {
        Default,
        Identifier,
        BooleanLiteral,
        IntegerLiteral,
        FloatLiteral,
        StringLiteral,
        InterpolatedString,
        Expression,
        NullReference,
        ThisReference
    }

    public PrimaryTypes PrimaryType;
    public Identifier PrimaryId;
    public bool? BooleanLiteral = null;
    public int? IntegerLiteral = null;
    public double? FloatLiteral = null;
    public string StringLiteral;

    public InterpolatedString InterpolatedString;
    public HeteroAstNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
