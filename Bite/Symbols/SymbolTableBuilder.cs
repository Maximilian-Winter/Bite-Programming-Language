using System;
using System.Collections.Generic;
using Bite.Ast;

namespace Bite.Symbols
{
public class BiteSymbolTableException : Exception
{
    public BiteSymbolTableException(string message): base(message)
    {
        BiteSymbolTableExceptionMessage = message;
    }
    
    public string BiteSymbolTableExceptionMessage { get; }
}


public class SymbolTableBuilder : HeteroAstVisitor < object >, IAstVisitor
{
    public enum ClassType
    {
        CLASS,
        SUBCLASS,
        NONE
    }

    public enum FunctionType
    {
        FUNCTION,
        CONSTRUCTOR,
        METHOD,
        NONE
    }

    private FunctionType m_CurrentFunction = FunctionType.NONE;
    private ClassType m_CurrentClass = ClassType.NONE;

    private SymbolTable m_SymbolTable;

    #region Public

    public SymbolTableBuilder( SymbolTable symbolTable )
    {
        m_SymbolTable = symbolTable;
    }

    public void BuildModuleSymbolTable( ModuleNode moduleNode )
    {
        Resolve( moduleNode );
    }

    public void BuildProgramSymbolTable( ProgramNode programNode )
    {
        Resolve( programNode );
    }

    public void BuildStatementsSymbolTable( List < StatementNode > statementNodes )
    {
        foreach ( StatementNode statementNode in statementNodes )
        {
            Resolve( statementNode );
        }
    }

    public override object Visit( ProgramNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        foreach ( ModuleNode module in node.GetModulesInDepedencyOrder() )
        {
            Resolve( module );
        }

        return null;
    }

    public override object Visit( ModuleNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        int d = 0;
        ModuleSymbol m = m_SymbolTable.CurrentScope.resolve( node.ModuleIdent.ToString(), out int moduleId, ref d ) as ModuleSymbol;
        bool defineModule = false;

        if ( m == null )
        {
            m = new ModuleSymbol( node.ModuleIdent.ToString(), node.ImportedModules, node.UsedModules );
            defineModule = true;
            m.EnclosingScope = m_SymbolTable.CurrentScope;
        }

        m_SymbolTable.PushScope( m );

        foreach ( StatementNode statement in node.Statements )
        {
            if ( statement is ClassDeclarationNode classDeclarationNode )
            {
                Resolve( classDeclarationNode );
            }
            else if ( statement is StructDeclarationNode structDeclaration )
            {
                Resolve( structDeclaration );
            }
            else if ( statement is FunctionDeclarationNode functionDeclarationNode )
            {
                Resolve( functionDeclarationNode );
            }
            else if ( statement is VariableDeclarationNode variable )
            {
                Resolve( variable );
            }
            else if ( statement is ClassInstanceDeclarationNode classInstance )
            {
                Resolve( classInstance );
            }
            else if ( statement is StatementNode stat )
            {
                Resolve( stat );
            }
        }

        m_SymbolTable.PopScope();

        if ( defineModule )
        {
            m_SymbolTable.CurrentScope.define( m );
            
        }
        m.CheckForAmbiguousReferences();
        return null;
    }

    public override object Visit( ModifiersNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( DeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( UsingStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        Resolve( node.UsingNode );
        Resolve( node.UsingBlock );

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( BreakStatementNode node )
    {
        return null;
    }

    public override object Visit( DeclarationsNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        if ( node.Classes != null )
        {
            foreach ( ClassDeclarationNode declaration in node.Classes )
            {
                Resolve( declaration );
            }
        }

        if ( node.Structs != null )
        {
            foreach ( StructDeclarationNode declaration in node.Structs )
            {
                Resolve( declaration );
            }
        }

        if ( node.Functions != null )
        {
            foreach ( FunctionDeclarationNode declaration in node.Functions )
            {
                Resolve( declaration );
            }
        }

        if ( node.ClassInstances != null )
        {
            foreach ( ClassInstanceDeclarationNode declaration in node.ClassInstances )
            {
                Resolve( declaration );
            }
        }

        if ( node.Variables != null )
        {
            foreach ( VariableDeclarationNode declaration in node.Variables )
            {
                Resolve( declaration );
            }
        }

        if ( node.Statements != null )
        {
            foreach ( StatementNode declaration in node.Statements )
            {
                Resolve( declaration );
            }
        }

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( ClassDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool isPublicClass = node.Modifiers.Modifiers != null &&
                             node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStaticClass = node.Modifiers.Modifiers != null &&
                             node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        bool isAbstractClass = node.Modifiers.Modifiers != null &&
                               node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareAbstract );

        if ( isStaticClass && isAbstractClass )
        {
            throw new Exception( "Classes can't be abstract and static!" );
        }

        bool noModifiers = !isAbstractClass && !isStaticClass;
        int depth = 0;
        ClassType enclosingClass = m_CurrentClass;
        m_CurrentClass = ClassType.CLASS;
        BiteClassType classType = new BiteClassType( node.ClassId.Id );

        ClassSymbol classSymbol = new ClassSymbol(
            node.ClassId.Id,
            isPublicClass ? AccesModifierType.Public : AccesModifierType.Private,
            noModifiers ? ClassAndMemberModifiers.None :
            isAbstractClass ? ClassAndMemberModifiers.Abstract : ClassAndMemberModifiers.Static );

        classSymbol.EnclosingScope = m_SymbolTable.CurrentScope;
        classSymbol.typeIndex = classType.TypeIndex;
        classSymbol.m_DefinitionNode = node;

        if ( node.Inheritance != null )
        {
            List < string > baseClasses = new List < string >();

            foreach ( Identifier identifier in node.Inheritance )
            {
                if ( identifier.Id != null && !string.IsNullOrEmpty( identifier.Id ) )
                {
                    m_CurrentClass = ClassType.SUBCLASS;

                    if ( identifier.Id.Equals( node.ClassId.Id ) )
                    {
                        throw new Exception( "Error: Classes cannot inherit from themselves!" );
                    }

                    baseClasses.Add( identifier.Id );
                }
                else
                {
                    throw new Exception( "Error: Base Class Name is null or empty!" );
                }
            }

            classSymbol.BaseClassNames = baseClasses;
        }

        m_SymbolTable.PushScope( classSymbol );

        foreach ( FieldSymbol fieldSymbol in classSymbol.Fields )
        {
            if ( !fieldSymbol.Name.Equals( "this" ) )
            {
                m_SymbolTable.CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
        {
            m_SymbolTable.CurrentScope.define( methodSymbol );
        }

        foreach ( VariableDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.Variables )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.VarId != null )
            {
                if ( memberDeclarationContext.Initializer != null )
                {
                    Resolve( memberDeclarationContext.Initializer );
                }

                bool isPublicField = memberDeclarationContext.Modifiers.Modifiers != null &&
                                     memberDeclarationContext.Modifiers.Modifiers.Contains(
                                         ModifiersNode.ModifierTypes.DeclarePublic );

                bool isStaticField = memberDeclarationContext.Modifiers.Modifiers != null &&
                                     memberDeclarationContext.Modifiers.Modifiers.Contains(
                                         ModifiersNode.ModifierTypes.DeclareStatic );

                FieldSymbol fieldSymbol = new FieldSymbol(
                    memberDeclarationContext.VarId.Id,
                    isPublicField ? AccesModifierType.Public : AccesModifierType.Private,
                    isStaticField ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

                fieldSymbol.Type = new BiteClassType( "Object" );
                fieldSymbol.DefinitionNode = memberDeclarationContext;
                m_SymbolTable.CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( ClassInstanceDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.
                     ClassInstances )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.InstanceId != null )
            {
                bool isPublicField = memberDeclarationContext.Modifiers.Modifiers != null &&
                                     memberDeclarationContext.Modifiers.Modifiers.Contains(
                                         ModifiersNode.ModifierTypes.DeclarePublic );

                bool isStaticField = memberDeclarationContext.Modifiers.Modifiers != null &&
                                     memberDeclarationContext.Modifiers.Modifiers.Contains(
                                         ModifiersNode.ModifierTypes.DeclareStatic );

                FieldSymbol fieldSymbol = new FieldSymbol(
                    memberDeclarationContext.InstanceId.Id,
                    isPublicField ? AccesModifierType.Public : AccesModifierType.Private,
                    isStaticField ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

                fieldSymbol.Type = new BiteClassType( memberDeclarationContext.ClassName.Id );
                fieldSymbol.DefinitionNode = memberDeclarationContext;
                m_SymbolTable.CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( FunctionDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.Functions )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.FunctionId.Id != null )
            {
                bool isPublic = memberDeclarationContext.Modifiers.Modifiers != null &&
                                memberDeclarationContext.Modifiers.Modifiers.Contains(
                                    ModifiersNode.ModifierTypes.DeclarePublic );

                bool isStatic = memberDeclarationContext.Modifiers.Modifiers != null &&
                                memberDeclarationContext.Modifiers.Modifiers.Contains(
                                    ModifiersNode.ModifierTypes.DeclareStatic );

                bool isAbstract = memberDeclarationContext.Modifiers.Modifiers != null &&
                                  memberDeclarationContext.Modifiers.Modifiers.Contains(
                                      ModifiersNode.ModifierTypes.DeclareAbstract );

                if ( isStatic && isAbstract )
                {
                    throw new Exception( "Methods can't be abstract and static!" );
                }

                if ( isAbstract && !isAbstractClass )
                {
                    throw new Exception( "Methods can't be abstract in a normal class!" );
                }

                FunctionType declarationType = memberDeclarationContext.FunctionId.Id.Equals( node.ClassId.Id )
                    ? FunctionType.CONSTRUCTOR
                    : FunctionType.METHOD;

                m_CurrentFunction = declarationType;

                MethodSymbol f = new MethodSymbol(
                    memberDeclarationContext.FunctionId.Id,
                    isPublic ? AccesModifierType.Public : AccesModifierType.Private,
                    isStatic ? ClassAndMemberModifiers.Static :
                    isAbstract ? ClassAndMemberModifiers.Abstract : ClassAndMemberModifiers.None );

                f.IsConstructor = declarationType == FunctionType.CONSTRUCTOR;
                f.defNode = memberDeclarationContext;
                f.EnclosingScope = m_SymbolTable.CurrentScope;
                m_SymbolTable.PushScope( f );

                if ( memberDeclarationContext.Parameters != null &&
                     memberDeclarationContext.Parameters.Identifiers != null )
                {
                    memberDeclarationContext.Parameters.AstScopeNode = m_SymbolTable.CurrentScope;

                    foreach ( Identifier id in memberDeclarationContext.Parameters.Identifiers )
                    {
                        id.AstScopeNode = m_SymbolTable.CurrentScope;
                        m_SymbolTable.CurrentScope.define( new ParameterSymbol( id.Id ) );
                    }
                }

                if ( memberDeclarationContext.FunctionBlock != null )
                {
                    ResolveDeclarations( memberDeclarationContext.FunctionBlock.Declarations );
                }

                m_SymbolTable.PopScope();
                m_SymbolTable.CurrentScope.define( f );
                m_CurrentFunction = FunctionType.NONE;
            }
        }

        FieldSymbol thisSymbol = new FieldSymbol( "this", AccesModifierType.Private, ClassAndMemberModifiers.None );
        thisSymbol.Type = classType;
        m_SymbolTable.CurrentScope.define( thisSymbol );

        m_SymbolTable.PopScope();

        m_SymbolTable.CurrentScope.define( classSymbol );
        m_CurrentClass = enclosingClass;

        return null;
    }

    public override object Visit( FunctionDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool declaredPublicOrPrivate = node.Modifiers.Modifiers != null &&
                                       node.Modifiers.Modifiers.Contains(
                                           ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        bool isAbstract = node.Modifiers.Modifiers != null &&
                          node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareAbstract );

        bool isExtern = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareExtern );

        bool isCallable = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareCallable );

        if ( isAbstract || isStatic )
        {
            throw new Exception( "Only methods in classes can be abstract or static!" );
        }

        if ( declaredPublicOrPrivate )
        {
            throw new Exception( "Only members in classes can be public or private!" );
        }

        m_CurrentFunction = FunctionType.FUNCTION;
        int depth = 0;

        FunctionSymbol f = new FunctionSymbol(
            node.FunctionId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static :
            isAbstract ? ClassAndMemberModifiers.Abstract : ClassAndMemberModifiers.None,
            isExtern,
            isCallable
            );

        if ( isCallable )
        {
            f.LinkName = node.LinkFunctionId.Id;
        }

        BiteClassType functionType = new BiteClassType( "Object" );

        f.Type = functionType;
        f.defNode = node;
        f.EnclosingScope = m_SymbolTable.CurrentScope;
        m_SymbolTable.PushScope( f );

        if ( node.Parameters != null )
        {
            node.Parameters.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( node.Parameters.Identifiers != null )
            {
                foreach ( Identifier id in node.Parameters.Identifiers )
                {
                    id.AstScopeNode = m_SymbolTable.CurrentScope;
                    m_SymbolTable.CurrentScope.define( new ParameterSymbol( id.Id ) );
                }
            }
        }

        if ( node.FunctionBlock != null && node.FunctionBlock.Declarations != null )
        {
            ResolveDeclarations( node.FunctionBlock.Declarations );
        }

        m_SymbolTable.PopScope();

        if ( node.AstScopeNode.resolve( node.FunctionId.Id, out int moduleId, ref depth, false ) == null )
        {
            m_SymbolTable.CurrentScope.define( f );
        }

        m_CurrentFunction = FunctionType.NONE;

        return null;
    }

    public override object Visit( LocalVariableInitializerNode node )
    {
        foreach ( var variableDeclaration in node.VariableDeclarations )
        {
            Resolve( variableDeclaration );
        }

        return null;
    }

    public override object Visit( LocalVariableDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.Expression != null )
        {
            Resolve( node.Expression );
        }

        bool declaredPublicOrPrivate = node.Modifiers.Modifiers != null &&
                                       node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        DynamicVariable variableSymbol = new DynamicVariable( node.VarId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        variableSymbol.Type = new BiteClassType( "Object" );
        variableSymbol.DefinitionNode = node;
        m_SymbolTable.CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( VariableDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.Initializer != null )
        {
            Resolve( node.Initializer );
        }

        bool declaredPublicOrPrivate = node.Modifiers.Modifiers != null &&
                                       node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        DynamicVariable variableSymbol = new DynamicVariable( node.VarId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        variableSymbol.Type = new BiteClassType( "Object" );
        variableSymbol.DefinitionNode = node;
        m_SymbolTable.CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( ClassInstanceDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool declaredPublicOrPrivate = node.Modifiers.Modifiers != null &&
                                       node.Modifiers.Modifiers.Contains(
                                           ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        BiteClassType classType = new BiteClassType( node.ClassName.Id );

        DynamicVariable classInstance = new DynamicVariable(
            node.InstanceId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        classInstance.Type = classType;
        classInstance.DefinitionNode = node;

        if ( !node.IsVariableRedeclaration )
        {
            m_SymbolTable.CurrentScope.define( classInstance );
        }

        return null;
    }

    public override object Visit( CallNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.Arguments != null && node.Arguments.Expressions != null )
        {
            foreach ( ExpressionNode argumentsExpression in node.Arguments.Expressions )
            {
                Resolve( argumentsExpression );
            }
        }

        if ( node.ElementAccess != null )
        {
            foreach ( CallElementEntry callElementEntry in node.ElementAccess )
            {
                if ( callElementEntry.CallElementType == CallElementTypes.Call )
                {
                    Resolve( callElementEntry.Call );
                }
            }
        }

        if ( node.IsFunctionCall )
        {
            /* LocalScope l = new LocalScope( SymbolTable.CurrentScope );
             SymbolTable.CurrentScope.nest( l );
             SymbolTable.pushScope( l );*/
            if ( node.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
            {
                int d = 0;
                node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d );
                Resolve( node.Primary );
            }

            //SymbolTable.popScope();
        }
        else
        {
            if ( node.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
            {
                int d = 0;
                //node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d, false);
                Resolve( node.Primary );
            }
            else
            {
                Resolve( node.Primary );
            }
        }

        if ( node.CallEntries != null )
        {
            int i = 0;

            foreach ( CallEntry terminalNode in node.CallEntries )
            {
                if ( terminalNode.Arguments != null && terminalNode.Arguments.Expressions != null )
                {
                    foreach ( ExpressionNode argumentsExpression in terminalNode.Arguments.Expressions )
                    {
                        Resolve( argumentsExpression );
                    }
                }

                if ( terminalNode.ElementAccess != null )
                {
                    foreach ( CallElementEntry callElementEntry in terminalNode.ElementAccess )
                    {
                        if ( callElementEntry.CallElementType == CallElementTypes.Call )
                        {
                            Resolve( callElementEntry.Call );
                        }
                    }
                }

                if ( terminalNode.IsFunctionCall )
                {
                    /* LocalScope l = new LocalScope( SymbolTable.CurrentScope );
                     SymbolTable.CurrentScope.nest( l );
                     SymbolTable.pushScope( l );*/
                    if ( terminalNode.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
                    {
                        int d = 0;
                        /*DynamicVariable symbol = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d ) as DynamicVariable;
                        ClassSymbol classSymbol = symbol.resolve( symbol.Type.Name, out moduleId, ref d ) as ClassSymbol;
                        classSymbol.resolve( terminalNode.Primary.PrimaryId.Id, out moduleId, ref d );*/
                        Resolve( terminalNode.Primary);
                    }

                    //SymbolTable.popScope();
                }
                else
                {
                    if ( terminalNode.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
                    {
                        int d = 0;
                        /*DynamicVariable symbol = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d ) as DynamicVariable;

                        if ( symbol == null )
                        {
                            throw new CompilerException(
                                $"Failed to resolve symbol '{node.Primary.PrimaryId.Id}' in scope '{node.AstScopeNode.Name}'",
                                node );
                        }

                        if ( !(symbol is ParameterSymbol) )
                        {
                            ClassSymbol classSymbol = symbol.resolve( symbol.Type.Name, out moduleId, ref d ) as ClassSymbol;
                            classSymbol.resolve( terminalNode.Primary.PrimaryId.Id, out moduleId, ref d );
                        }*/
                        
                        Resolve( terminalNode.Primary );
                    }
                    
                }

                i++;
            }
        }

        return null;
    }

    public override object Visit( ArgumentsNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( ParametersNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( AssignmentNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        switch ( node.Type )
        {
            case AssignmentTypes.Assignment:
                Resolve( node.Call );
                Resolve( node.Assignment );

                break;

            case AssignmentTypes.Binary:
                Resolve( node.Binary );

                break;

            case AssignmentTypes.Ternary:
                Resolve( node.Ternary );

                break;

            case AssignmentTypes.Call:
                Resolve( node.Call );

                break;

            case AssignmentTypes.Primary:
                Resolve( node.PrimaryNode );

                break;

            case AssignmentTypes.UnaryPostfix:
                Resolve( node.UnaryPostfix );

                break;

            case AssignmentTypes.UnaryPrefix:
                Resolve( node.UnaryPrefix );

                break;

            default:
                throw new Exception( "Invalid Type" );
        }

        return null;
    }

    public override object Visit( ExpressionNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Assignment );

        return null;
    }

    public override object Visit( BlockStatementNode node )
    {
        ResolveDeclarations( node.Declarations );

        return null;
    }

    public override object Visit( StatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node is ExpressionStatementNode expressionStatementNode )
        {
            Resolve( expressionStatementNode );

            return null;
        }

        if ( node is ForStatementNode forStatementNode )
        {
            Resolve( forStatementNode );

            return null;
        }

        if ( node is IfStatementNode ifStatementNode )
        {
            Resolve( ifStatementNode );

            return null;
        }

        if ( node is WhileStatementNode whileStatement )
        {
            Resolve( whileStatement );

            return null;
        }

        if ( node is ReturnStatementNode returnStatement )
        {
            Resolve( returnStatement );

            return null;
        }

        if ( node is BlockStatementNode blockStatementNode )
        {
            Resolve( blockStatementNode );

            return null;
        }

        return null;
    }

    public override object Visit( ExpressionStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Expression );

        return null;
    }

    public override object Visit( IfStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Expression );
        Resolve(node.ThenStatement);

        if (node.ElseStatement != null)
        {
            Resolve(node.ElseStatement);
        }

        return null;
    }

    public override object Visit( ForStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        if ( node.Initializer != null )
        {
            if ( node.Initializer.Expressions != null )
            {
                foreach ( var expression in node.Initializer.Expressions )
                {
                    Resolve( expression );
                }
            }
            else if ( node.Initializer.LocalVariableInitializer != null )
            {
                Resolve( node.Initializer.LocalVariableInitializer );
            }
        }

        if ( node.Condition != null )
        {
            Resolve( node.Condition );
        }

        if ( node.Iterators != null )
        {
            foreach ( var iterator in node.Iterators )
            {
                Resolve( iterator );
            }
        }

        if ( node.Statement != null )
        {
            Resolve( node.Statement );
        }

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( WhileStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Expression );
        Resolve( node.WhileBlock );

        return null;
    }

    public override object Visit( ReturnStatementNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( m_CurrentFunction == FunctionType.NONE )
        {
            return null;
        }

        if ( node.ExpressionStatement != null )
        {
            if ( m_CurrentFunction == FunctionType.CONSTRUCTOR )
            {
                return null;
            }

            Resolve( node.ExpressionStatement );
        }

        return null;
    }

    public override object Visit( InitializerNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Expression );

        return null;
    }

    public override object Visit( BinaryOperationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( TernaryOperationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.MidOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( PrimaryNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        
        if ( node.PrimaryType == PrimaryNode.PrimaryTypes.InterpolatedString )
        {
            foreach ( InterpolatedStringPart interpolatedStringStringPart in node.InterpolatedString.StringParts )
            {
                Resolve( interpolatedStringStringPart.ExpressionNode );
            }
        }
        
        if ( node.PrimaryType == PrimaryNode.PrimaryTypes.Expression )
        {
            Resolve( node.Expression );
        }

        return null;
    }

    public override object Visit( StructDeclarationNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Primary );

        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Primary );

        return null;
    }

    public override object Visit( HeteroAstNode node )
    {
        switch ( node )
        {
            case ProgramNode program:
                return Visit( program );

            case ModuleNode module:
                return Visit( module );

            case ClassDeclarationNode classDeclarationNode:
                return Visit( classDeclarationNode );

            case StructDeclarationNode structDeclaration:
                return Visit( structDeclaration );

            case FunctionDeclarationNode functionDeclarationNode:
                return Visit( functionDeclarationNode );
            
            case LocalVariableDeclarationNode localVar:
                return Visit(localVar);

            case LocalVariableInitializerNode initializer:
                return Visit(initializer);

            case VariableDeclarationNode variable:
                return Visit( variable );

            case ClassInstanceDeclarationNode classInstance:
                return Visit( classInstance );

            case UsingStatementNode usingStatementNode:
                return Visit( usingStatementNode );

            case ExpressionStatementNode expressionStatementNode:
                return Visit( expressionStatementNode );

            case ForStatementNode forStatementNode:
                return Visit( forStatementNode );

            case WhileStatementNode whileStatement:
                return Visit( whileStatement );

            case IfStatementNode ifStatementNode:
                return Visit( ifStatementNode );

            case ReturnStatementNode returnStatement:
                return Visit( returnStatement );

            case BlockStatementNode blockStatementNode:
                return Visit( blockStatementNode );

            case AssignmentNode assignmentNode:
                return Visit( assignmentNode );

            case CallNode callNode:
                return Visit( callNode );

            case BinaryOperationNode binaryOperation:
                return Visit( binaryOperation );

            case TernaryOperationNode ternaryOperationNode:
                return Visit( ternaryOperationNode );

            case PrimaryNode primaryNode:
                return Visit( primaryNode );

            case DeclarationsNode declarationsNode:
                return Visit( declarationsNode );

            case UnaryPostfixOperation postfixOperation:
                return Visit( postfixOperation );

            case UnaryPrefixOperation prefixOperation:
                return Visit( prefixOperation );

            case StatementNode stat:
                return Visit( stat );

            case ExpressionNode expression:
                return Visit( expression );

            case InitializerNode initializerNode:
                return Visit( initializerNode.Expression );

            default:
                return null;
        }
    }

    #endregion

    #region Private


    private object Resolve( HeteroAstNode astNode )
    {
        return astNode.Accept( this );
    }

    private void ResolveDeclarations( DeclarationsNode declarationsNode )
    {
        Resolve( declarationsNode );
    }

    #endregion
}

}
