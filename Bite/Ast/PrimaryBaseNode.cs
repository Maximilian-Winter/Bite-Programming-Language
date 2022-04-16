using System.Collections.Generic;

namespace Bite.Ast
{

public class InterpolatedStringPart
{
    public string TextBeforeExpression;
    public ExpressionBaseNode ExpressionBaseNode;

    #region Public

    public InterpolatedStringPart( string textBeforeExpression, ExpressionBaseNode expressionBaseNode )
    {
        TextBeforeExpression = textBeforeExpression;
        ExpressionBaseNode = expressionBaseNode;
    }

    #endregion
}

public class InterpolatedString : ExpressionBaseNode
{
    public List < InterpolatedStringPart > StringParts;
    public string TextAfterLastExpression;
}

public class ArrayExpressionNode : ExpressionBaseNode
{
    public List < ExpressionBaseNode > Expressions;
}

public class DictionaryInitializerNode : ExpressionBaseNode
{
    public Dictionary < Identifier, ExpressionBaseNode > ElementInitializers;
}

public class PrimaryBaseNode : ExpressionBaseNode
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
        ThisReference,
        ArrayExpression,
        DictionaryExpression
    }

    public PrimaryTypes PrimaryType;
    public Identifier PrimaryId;
    public bool? BooleanLiteral = null;
    public int? IntegerLiteral = null;
    public double? FloatLiteral = null;
    public string StringLiteral;

    public InterpolatedString InterpolatedString;
    public AstBaseNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
