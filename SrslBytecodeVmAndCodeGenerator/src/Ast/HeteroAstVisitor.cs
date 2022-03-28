namespace Srsl.Ast
{

    public abstract class HeteroAstVisitor<T>
    {
        #region Public

        public abstract T Visit(ProgramNode node);

        public abstract T Visit(ModuleNode node);

        public abstract T Visit(ModifiersNode node);

        public abstract T Visit(DeclarationNode node);

        public abstract T Visit(UsingStatementNode node);

        public abstract T Visit(DeclarationsNode node);

        public abstract T Visit(ClassDeclarationNode node);

        public abstract T Visit(FunctionDeclarationNode node);

        public abstract T Visit(VariableDeclarationNode node);

        public abstract T Visit(ClassInstanceDeclarationNode node);

        public abstract T Visit(CallNode node);

        public abstract T Visit(ArgumentsNode node);

        public abstract T Visit(ParametersNode node);

        public abstract T Visit(AssignmentNode node);

        public abstract T Visit(ExpressionNode node);

        public abstract T Visit(BlockStatementNode node);

        public abstract T Visit(StatementNode node);

        public abstract T Visit(ExpressionStatementNode node);

        public abstract T Visit(IfStatementNode node);

        public abstract T Visit(ForStatementNode node);

        public abstract T Visit(WhileStatementNode node);

        public abstract T Visit(ReturnStatementNode node);

        public abstract T Visit(InitializerNode node);

        public abstract T Visit(BinaryOperationNode node);

        public abstract T Visit(TernaryOperationNode node);

        public abstract T Visit(PrimaryNode node);

        public abstract T Visit(StructDeclarationNode node);

        public abstract T Visit(UnaryPostfixOperation node);

        public abstract T Visit(UnaryPrefixOperation node);

        public abstract T Visit(HeteroAstNode node);

        #endregion
    }

}
