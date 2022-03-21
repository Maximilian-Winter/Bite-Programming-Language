using System.Collections.Generic;

namespace MemoizeSharp
{

    public class DeclarationsNode : DeclarationNode
    {
        public List<ClassDeclarationNode> Classes;
        public List<StructDeclarationNode> Structs;
        public List<ClassInstanceDeclarationNode> ClassInstances;
        public List<VariableDeclarationNode> Variables;
        public List<FunctionDeclarationNode> Functions;
        public List<StatementNode> Statements;

        #region Public

        public DeclarationsNode()
        {
            Classes = new List<ClassDeclarationNode>();
            Structs = new List<StructDeclarationNode>();
            ClassInstances = new List<ClassInstanceDeclarationNode>();
            Variables = new List<VariableDeclarationNode>();
            Functions = new List<FunctionDeclarationNode>();
            Statements = new List<StatementNode>();
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
