using System;
using System.Collections.Generic;
using Bite.Ast;
using Bite.SymbolTable;

namespace Bite.Runtime.SymbolTable
{

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

    public Scope CurrentScope { get; private set; }

    #region Public

    public void BuildModuleSymbolTable( ModuleNode moduleNode )
    {
        if ( CurrentScope == null )
        {
            InitScopes();
        }

        Resolve( moduleNode );
    }

    public void BuildProgramSymbolTable( ProgramNode programNode )
    {
        Resolve( programNode );
    }

    public void BuildStatementsSymbolTable( List < StatementNode > statementNodes )
    {
        if ( CurrentScope == null )
        {
            InitScopes();
        }

        foreach ( StatementNode statementNode in statementNodes )
        {
            Resolve( statementNode );
        }
    }

    public override object Visit( ProgramNode node )
    {
        InitScopes();
        node.AstScopeNode = CurrentScope;

        foreach ( ModuleNode module in node.GetModulesInDepedencyOrder() )
        {
            Resolve( module );
        }

        return null;
    }

    public override object Visit( ModuleNode node )
    {
        node.AstScopeNode = CurrentScope;
        int d = 0;
        ModuleSymbol m = CurrentScope.resolve( node.ModuleIdent.ToString(), out int moduleId, ref d ) as ModuleSymbol;
        bool defineModule = false;

        if ( m == null )
        {
            m = new ModuleSymbol( node.ModuleIdent.ToString(), node.ImportedModules, node.UsedModules );
            defineModule = true;
            m.EnclosingScope = CurrentScope;
        }

        pushScope( m );

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

        popScope();

        if ( defineModule )
        {
            CurrentScope.define( m );
        }

        return null;
    }

    public override object Visit( ModifiersNode node )
    {
        node.AstScopeNode = CurrentScope;

        return null;
    }

    public override object Visit( DeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

        return null;
    }

    public override object Visit( UsingStatementNode node )
    {
        node.AstScopeNode = CurrentScope;
        LocalScope l = new LocalScope( CurrentScope );
        CurrentScope.nest( l );
        pushScope( l );

        Resolve( node.UsingNode );
        Resolve( node.UsingBlock );

        popScope();

        return null;
    }

    public override object Visit( BreakStatementNode node )
    {
        return null;
    }

    public override object Visit( DeclarationsNode node )
    {
        node.AstScopeNode = CurrentScope;

        LocalScope l = new LocalScope( CurrentScope );
        CurrentScope.nest( l );
        pushScope( l );

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

        popScope();

        return null;
    }

    public override object Visit( ClassDeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

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

        ClassType enclosingClass = m_CurrentClass;
        m_CurrentClass = ClassType.CLASS;
        BiteClassType classType = new BiteClassType( node.ClassId.Id );

        ClassSymbol classSymbol = new ClassSymbol(
            node.ClassId.Id,
            isPublicClass ? AccesModifierType.Public : AccesModifierType.Private,
            noModifiers ? ClassAndMemberModifiers.None :
            isAbstractClass ? ClassAndMemberModifiers.Abstract : ClassAndMemberModifiers.Static );

        classSymbol.EnclosingScope = CurrentScope;
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

        pushScope( classSymbol );

        foreach ( FieldSymbol fieldSymbol in classSymbol.Fields )
        {
            if ( !fieldSymbol.Name.Equals( "this" ) )
            {
                CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
        {
            CurrentScope.define( methodSymbol );
        }

        foreach ( VariableDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.Variables )
        {
            memberDeclarationContext.AstScopeNode = CurrentScope;

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
                CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( ClassInstanceDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.
                     ClassInstances )
        {
            memberDeclarationContext.AstScopeNode = CurrentScope;

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
                CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( FunctionDeclarationNode memberDeclarationContext in node.BlockStatement.Declarations.Functions )
        {
            memberDeclarationContext.AstScopeNode = CurrentScope;

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
                f.EnclosingScope = CurrentScope;
                pushScope( f );

                if ( memberDeclarationContext.Parameters != null &&
                     memberDeclarationContext.Parameters.Identifiers != null )
                {
                    memberDeclarationContext.Parameters.AstScopeNode = CurrentScope;

                    foreach ( Identifier id in memberDeclarationContext.Parameters.Identifiers )
                    {
                        id.AstScopeNode = CurrentScope;
                        CurrentScope.define( new ParameterSymbol( id.Id ) );
                    }
                }

                if ( memberDeclarationContext.FunctionBlock != null )
                {
                    ResolveDeclarations( memberDeclarationContext.FunctionBlock.Declarations );
                }

                popScope();
                CurrentScope.define( f );
                m_CurrentFunction = FunctionType.NONE;
            }
        }

        FieldSymbol thisSymbol = new FieldSymbol( "this", AccesModifierType.Private, ClassAndMemberModifiers.None );
        thisSymbol.Type = classType;
        CurrentScope.define( thisSymbol );

        popScope();

        CurrentScope.define( classSymbol );
        m_CurrentClass = enclosingClass;

        return null;
    }

    public override object Visit( FunctionDeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

        bool declaredPublicOrPrivate = node.Modifiers.Modifiers != null &&
                                       node.Modifiers.Modifiers.Contains(
                                           ModifiersNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.Modifiers.Modifiers != null &&
                        node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareStatic );

        bool isAbstract = node.Modifiers.Modifiers != null &&
                          node.Modifiers.Modifiers.Contains( ModifiersNode.ModifierTypes.DeclareAbstract );

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
            isAbstract ? ClassAndMemberModifiers.Abstract : ClassAndMemberModifiers.None );

        BiteClassType functionType = new BiteClassType( "Object" );

        f.Type = functionType;
        f.defNode = node;
        f.EnclosingScope = CurrentScope;
        pushScope( f );

        if ( node.Parameters != null )
        {
            node.Parameters.AstScopeNode = CurrentScope;

            if ( node.Parameters.Identifiers != null )
            {
                foreach ( Identifier id in node.Parameters.Identifiers )
                {
                    id.AstScopeNode = CurrentScope;
                    CurrentScope.define( new ParameterSymbol( id.Id ) );
                }
            }
        }

        if ( node.FunctionBlock != null && node.FunctionBlock.Declarations != null )
        {
            ResolveDeclarations( node.FunctionBlock.Declarations );
        }

        popScope();

        if ( node.AstScopeNode.resolve( node.FunctionId.Id, out int moduleId, ref depth ) == null )
        {
            CurrentScope.define( f );
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
        node.AstScopeNode = CurrentScope;

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
        CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( VariableDeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

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
        CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( ClassInstanceDeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

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
            CurrentScope.define( classInstance );
        }

        return null;
    }

    public override object Visit( CallNode node )
    {
        node.AstScopeNode = CurrentScope;

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
            /* LocalScope l = new LocalScope( CurrentScope );
             CurrentScope.nest( l );
             pushScope( l );*/
            Resolve( node.Primary );

            //popScope();
        }
        else
        {
            Resolve( node.Primary );
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
                    /* LocalScope l = new LocalScope( CurrentScope );
                     CurrentScope.nest( l );
                     pushScope( l );*/
                    Resolve( node.Primary );

                    //popScope();
                }
                else
                {
                    Resolve( terminalNode.Primary );
                }

                i++;
            }
        }

        return null;
    }

    public override object Visit( ArgumentsNode node )
    {
        node.AstScopeNode = CurrentScope;

        return null;
    }

    public override object Visit( ParametersNode node )
    {
        node.AstScopeNode = CurrentScope;

        return null;
    }

    public override object Visit( AssignmentNode node )
    {
        node.AstScopeNode = CurrentScope;

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
        node.AstScopeNode = CurrentScope;
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
        node.AstScopeNode = CurrentScope;

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
        node.AstScopeNode = CurrentScope;
        Resolve( node.Expression );

        return null;
    }

    public override object Visit( IfStatementNode node )
    {
        node.AstScopeNode = CurrentScope;
        Resolve( node.Expression );
        Resolve(node.ThenStatement);

        if (node.ElseStatement != null)
        {
            Resolve(node.ElseStatement);
        }

        // TODO: Remove
        if ( node.IfStatementEntries != null )
        {
            foreach ( IfStatementEntry nodeIfStatementEntry in node.IfStatementEntries )
            {
                if ( nodeIfStatementEntry.IfStatementType == IfStatementEntryType.ElseIf )
                {
                    Resolve( nodeIfStatementEntry.ExpressionElseIf );
                }

                Resolve( nodeIfStatementEntry.ElseBlock );
            }
        }

        return null;
    }

    public override object Visit( ForStatementNode node )
    {
        node.AstScopeNode = CurrentScope;
        LocalScope l = new LocalScope( CurrentScope );
        CurrentScope.nest( l );
        pushScope( l );

        if ( node.VariableDeclaration != null )
        {
            Resolve( node.VariableDeclaration );
        }
        else if ( node.ExpressionStatement != null )
        {
            Resolve( node.ExpressionStatement );
        }
        else if ( node.Initializer != null )
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

        if ( node.Iterator != null )
        {
            Resolve( node.Iterator );
        }

        if ( node.Block != null )
        {
            Resolve( node.Block );
        }

        popScope();

        return null;
    }

    public override object Visit( WhileStatementNode node )
    {
        node.AstScopeNode = CurrentScope;
        Resolve( node.Expression );
        Resolve( node.WhileBlock );

        return null;
    }

    public override object Visit( ReturnStatementNode node )
    {
        node.AstScopeNode = CurrentScope;

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
        node.AstScopeNode = CurrentScope;
        Resolve( node.Expression );

        return null;
    }

    public override object Visit( BinaryOperationNode node )
    {
        node.AstScopeNode = CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( TernaryOperationNode node )
    {
        node.AstScopeNode = CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.MidOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( PrimaryNode node )
    {
        node.AstScopeNode = CurrentScope;

        if ( node.PrimaryType == PrimaryNode.PrimaryTypes.Expression )
        {
            Resolve( node.Expression );
        }

        return null;
    }

    public override object Visit( StructDeclarationNode node )
    {
        node.AstScopeNode = CurrentScope;

        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        node.AstScopeNode = CurrentScope;
        Resolve( node.Primary );

        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        node.AstScopeNode = CurrentScope;
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

    private void InitScopes()
    {
        GlobalScope g = new GlobalScope( null );
        pushScope( g );

        ModuleSymbol m = new ModuleSymbol(
            "System",
            new List < ModuleIdentifier >(),
            new List < ModuleIdentifier >() );

        m.EnclosingScope = CurrentScope;
        pushScope( m );
        BiteClassType classType = new BiteClassType( "Object" );

        ClassSymbol classSymbol = new ClassSymbol(
            "Object",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        classSymbol.EnclosingScope = CurrentScope;
        classSymbol.typeIndex = classType.TypeIndex;
        classSymbol.m_DefinitionNode = null;
        CurrentScope.define( classSymbol );

        FunctionSymbol functionSymbol = new FunctionSymbol(
            "Print",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        ParameterSymbol parameterSymbol = new ParameterSymbol( "x" );
        BiteClassType functionType = new BiteClassType( "Object" );

        functionSymbol.Type = functionType;
        functionSymbol.EnclosingScope = CurrentScope;
        FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier( "Print" );
        functionDeclarationNode.Parameters = new ParametersNode();
        functionDeclarationNode.Parameters.Identifiers = new List < Identifier >();
        functionDeclarationNode.Parameters.Identifiers.Add( new Identifier( "x" ) );
        functionSymbol.defNode = functionDeclarationNode;
        pushScope( functionSymbol );

        functionSymbol.define( parameterSymbol );

        popScope();
        CurrentScope.define( functionSymbol );

        functionSymbol = new FunctionSymbol(
            "CSharpInterfaceCall",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        ParameterSymbol parameterCSharpInterfaceSymbol = new ParameterSymbol( "x" );
        BiteClassType functionTypeCSharpInterface = new BiteClassType( "Object" );

        functionSymbol.Type = functionTypeCSharpInterface;
        functionSymbol.EnclosingScope = CurrentScope;
        functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier( "CSharpInterfaceCall" );
        functionDeclarationNode.Parameters = new ParametersNode();
        functionDeclarationNode.Parameters.Identifiers = new List < Identifier >();
        functionDeclarationNode.Parameters.Identifiers.Add( new Identifier( "x" ) );
        functionSymbol.defNode = functionDeclarationNode;
        pushScope( functionSymbol );

        functionSymbol.define( parameterCSharpInterfaceSymbol );

        popScope();
        CurrentScope.define( functionSymbol );

        BiteClassType classTypeFli = new BiteClassType( "CSharpInterface" );

        ClassSymbol classSymbolFli = new ClassSymbol(
            "CSharpInterface",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        classSymbolFli.EnclosingScope = CurrentScope;
        classSymbolFli.typeIndex = classTypeFli.TypeIndex;
        classSymbolFli.m_DefinitionNode = null;

        pushScope( classSymbolFli );
        FieldSymbol fieldSymbol = new FieldSymbol( "Type", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "Method", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "Arguments", AccesModifierType.Public, ClassAndMemberModifiers.None );

        ClassInstanceDeclarationNode typeDecl = new ClassInstanceDeclarationNode();
        typeDecl.AstScopeNode = CurrentScope;
        typeDecl.ClassName = new Identifier( "Object" );
        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol(
            "ConstructorArguments",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol(
            "ConstructorArgumentsTypes",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "ObjectInstance", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        popScope();
        CurrentScope.define( classSymbolFli );
        popScope();
        CurrentScope.define( m );
    }

    private void popScope()
    {
        CurrentScope = CurrentScope.EnclosingScope;
    }

    private void pushScope( Scope s )
    {
        CurrentScope = s;
    }

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
