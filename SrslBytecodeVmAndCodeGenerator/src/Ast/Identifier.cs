namespace MemoizeSharp
{

    public class Identifier : HeteroAstNode
    {
        public string Id;

        #region Public

        public Identifier()
        {
        }

        public Identifier(string id)
        {
            Id = id;
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
