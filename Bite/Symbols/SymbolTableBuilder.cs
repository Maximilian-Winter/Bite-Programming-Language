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


public class SymbolTableBuilder : AstVisitor < object >, IAstVisitor
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

    public void BuildModuleSymbolTable( ModuleBaseNode moduleBaseNode )
    {
        Resolve( moduleBaseNode );
    }

    public void BuildProgramSymbolTable( ProgramBaseNode programBaseNode )
    {
        Resolve( programBaseNode );
    }

    public void BuildStatementsSymbolTable( List < StatementBaseNode > statementNodes )
    {
        foreach ( StatementBaseNode statementNode in statementNodes )
        {
            Resolve( statementNode );
        }
    }

    public override object Visit( ProgramBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        foreach ( ModuleBaseNode module in node.GetModulesInDepedencyOrder() )
        {
            Resolve( module );
        }

        return null;
    }

    public override object Visit( ModuleBaseNode node )
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

        foreach ( StatementBaseNode statement in node.Statements )
        {
            if ( statement is ClassDeclarationBaseNode classDeclarationNode )
            {
                Resolve( classDeclarationNode );
            }
            else if ( statement is StructDeclarationBaseNode structDeclaration )
            {
                Resolve( structDeclaration );
            }
            else if ( statement is FunctionDeclarationBaseNode functionDeclarationNode )
            {
                Resolve( functionDeclarationNode );
            }
            else if ( statement is VariableDeclarationBaseNode variable )
            {
                Resolve( variable );
            }
            else if ( statement is ClassInstanceDeclarationBaseNode classInstance )
            {
                Resolve( classInstance );
            }
            else if ( statement is StatementBaseNode stat )
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

    public override object Visit( ModifiersBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( DeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( UsingStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        Resolve( node.UsingBaseNode );
        Resolve( node.UsingBlock );

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( BreakStatementBaseNode node )
    {
        return null;
    }

    public override object Visit( DeclarationsBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        if ( node.Classes != null )
        {
            foreach ( ClassDeclarationBaseNode declaration in node.Classes )
            {
                Resolve( declaration );
            }
        }

        if ( node.Structs != null )
        {
            foreach ( StructDeclarationBaseNode declaration in node.Structs )
            {
                Resolve( declaration );
            }
        }

        if ( node.Functions != null )
        {
            foreach ( FunctionDeclarationBaseNode declaration in node.Functions )
            {
                Resolve( declaration );
            }
        }

        if ( node.ClassInstances != null )
        {
            foreach ( ClassInstanceDeclarationBaseNode declaration in node.ClassInstances )
            {
                Resolve( declaration );
            }
        }

        if ( node.Variables != null )
        {
            foreach ( VariableDeclarationBaseNode declaration in node.Variables )
            {
                Resolve( declaration );
            }
        }

        if ( node.Statements != null )
        {
            foreach ( StatementBaseNode declaration in node.Statements )
            {
                Resolve( declaration );
            }
        }

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( ClassDeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool isPublicClass = node.ModifiersBase.Modifiers != null &&
                             node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclarePublic );

        bool isStaticClass = node.ModifiersBase.Modifiers != null &&
                             node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareStatic );

        bool isAbstractClass = node.ModifiersBase.Modifiers != null &&
                               node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareAbstract );

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
        classSymbol.m_DefinitionBaseNode = node;

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

        foreach ( VariableDeclarationBaseNode memberDeclarationContext in node.BlockStatementBase.DeclarationsBase.Variables )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.VarId != null )
            {
                if ( memberDeclarationContext.InitializerBase != null )
                {
                    Resolve( memberDeclarationContext.InitializerBase );
                }

                bool isPublicField = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                     memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                         ModifiersBaseNode.ModifierTypes.DeclarePublic );

                bool isStaticField = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                     memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                         ModifiersBaseNode.ModifierTypes.DeclareStatic );

                FieldSymbol fieldSymbol = new FieldSymbol(
                    memberDeclarationContext.VarId.Id,
                    isPublicField ? AccesModifierType.Public : AccesModifierType.Private,
                    isStaticField ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

                fieldSymbol.Type = new BiteClassType( "Object" );
                fieldSymbol.DefinitionBaseNode = memberDeclarationContext;
                m_SymbolTable.CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( ClassInstanceDeclarationBaseNode memberDeclarationContext in node.BlockStatementBase.DeclarationsBase.
                     ClassInstances )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.InstanceId != null )
            {
                bool isPublicField = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                     memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                         ModifiersBaseNode.ModifierTypes.DeclarePublic );

                bool isStaticField = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                     memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                         ModifiersBaseNode.ModifierTypes.DeclareStatic );

                FieldSymbol fieldSymbol = new FieldSymbol(
                    memberDeclarationContext.InstanceId.Id,
                    isPublicField ? AccesModifierType.Public : AccesModifierType.Private,
                    isStaticField ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

                fieldSymbol.Type = new BiteClassType( memberDeclarationContext.ClassName.Id );
                fieldSymbol.DefinitionBaseNode = memberDeclarationContext;
                m_SymbolTable.CurrentScope.define( fieldSymbol );
            }
        }

        foreach ( FunctionDeclarationBaseNode memberDeclarationContext in node.BlockStatementBase.DeclarationsBase.Functions )
        {
            memberDeclarationContext.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( memberDeclarationContext.FunctionId.Id != null )
            {
                bool isPublic = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                    ModifiersBaseNode.ModifierTypes.DeclarePublic );

                bool isStatic = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                    ModifiersBaseNode.ModifierTypes.DeclareStatic );

                bool isAbstract = memberDeclarationContext.ModifiersBase.Modifiers != null &&
                                  memberDeclarationContext.ModifiersBase.Modifiers.Contains(
                                      ModifiersBaseNode.ModifierTypes.DeclareAbstract );

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
                f.m_DefBaseNode = memberDeclarationContext;
                f.EnclosingScope = m_SymbolTable.CurrentScope;
                m_SymbolTable.PushScope( f );

                if ( memberDeclarationContext.ParametersBase != null &&
                     memberDeclarationContext.ParametersBase.Identifiers != null )
                {
                    memberDeclarationContext.ParametersBase.AstScopeNode = m_SymbolTable.CurrentScope;

                    foreach ( Identifier id in memberDeclarationContext.ParametersBase.Identifiers )
                    {
                        id.AstScopeNode = m_SymbolTable.CurrentScope;
                        m_SymbolTable.CurrentScope.define( new ParameterSymbol( id.Id ) );
                    }
                }

                if ( memberDeclarationContext.FunctionBlock != null )
                {
                    ResolveDeclarations( memberDeclarationContext.FunctionBlock.DeclarationsBase );
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

    public override object Visit( FunctionDeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool declaredPublicOrPrivate = node.ModifiersBase.Modifiers != null &&
                                       node.ModifiersBase.Modifiers.Contains(
                                           ModifiersBaseNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareStatic );

        bool isAbstract = node.ModifiersBase.Modifiers != null &&
                          node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareAbstract );

        bool isExtern = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareExtern );

        bool isCallable = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareCallable );

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
        f.m_DefBaseNode = node;
        f.EnclosingScope = m_SymbolTable.CurrentScope;
        m_SymbolTable.PushScope( f );

        if ( node.ParametersBase != null )
        {
            node.ParametersBase.AstScopeNode = m_SymbolTable.CurrentScope;

            if ( node.ParametersBase.Identifiers != null )
            {
                foreach ( Identifier id in node.ParametersBase.Identifiers )
                {
                    id.AstScopeNode = m_SymbolTable.CurrentScope;
                    m_SymbolTable.CurrentScope.define( new ParameterSymbol( id.Id ) );
                }
            }
        }

        if ( node.FunctionBlock != null && node.FunctionBlock.DeclarationsBase != null )
        {
            ResolveDeclarations( node.FunctionBlock.DeclarationsBase );
        }

        m_SymbolTable.PopScope();

        if ( node.AstScopeNode.resolve( node.FunctionId.Id, out int moduleId, ref depth, false ) == null )
        {
            m_SymbolTable.CurrentScope.define( f );
        }

        m_CurrentFunction = FunctionType.NONE;

        return null;
    }

    public override object Visit( LocalVariableInitializerBaseNode node )
    {
        foreach ( var variableDeclaration in node.VariableDeclarations )
        {
            Resolve( variableDeclaration );
        }

        return null;
    }

    public override object Visit( LocalVariableDeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.ExpressionBase != null )
        {
            Resolve( node.ExpressionBase );
        }

        bool declaredPublicOrPrivate = node.ModifiersBase.Modifiers != null &&
                                       node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareStatic );

        DynamicVariable variableSymbol = new DynamicVariable( node.VarId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        variableSymbol.Type = new BiteClassType( "Object" );
        variableSymbol.DefinitionBaseNode = node;
        m_SymbolTable.CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( VariableDeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.InitializerBase != null )
        {
            Resolve( node.InitializerBase );
        }

        bool declaredPublicOrPrivate = node.ModifiersBase.Modifiers != null &&
                                       node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareStatic );

        DynamicVariable variableSymbol = new DynamicVariable( node.VarId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        variableSymbol.Type = new BiteClassType( "Object" );
        variableSymbol.DefinitionBaseNode = node;
        m_SymbolTable.CurrentScope.define( variableSymbol );

        return null;
    }

    public override object Visit( ClassInstanceDeclarationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        bool declaredPublicOrPrivate = node.ModifiersBase.Modifiers != null &&
                                       node.ModifiersBase.Modifiers.Contains(
                                           ModifiersBaseNode.ModifierTypes.DeclarePublic );

        bool isStatic = node.ModifiersBase.Modifiers != null &&
                        node.ModifiersBase.Modifiers.Contains( ModifiersBaseNode.ModifierTypes.DeclareStatic );

        BiteClassType classType = new BiteClassType( node.ClassName.Id );

        DynamicVariable classInstance = new DynamicVariable(
            node.InstanceId.Id,
            declaredPublicOrPrivate ? AccesModifierType.Public : AccesModifierType.Private,
            isStatic ? ClassAndMemberModifiers.Static : ClassAndMemberModifiers.None );

        classInstance.Type = classType;
        classInstance.DefinitionBaseNode = node;

        if ( !node.IsVariableRedeclaration )
        {
            m_SymbolTable.CurrentScope.define( classInstance );
        }

        return null;
    }

    public override object Visit( CallBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node.ArgumentsBase != null && node.ArgumentsBase.Expressions != null )
        {
            foreach ( ExpressionBaseNode argumentsExpression in node.ArgumentsBase.Expressions )
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
                    Resolve( callElementEntry.CallBase );
                }
            }
        }

        if ( node.IsFunctionCall )
        {
            /* LocalScope l = new LocalScope( SymbolTable.CurrentScope );
             SymbolTable.CurrentScope.nest( l );
             SymbolTable.pushScope( l );*/
            if ( node.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
            {
                int d = 0;
                node.AstScopeNode.resolve( node.PrimaryBase.PrimaryId.Id, out int moduleId, ref d );
                Resolve( node.PrimaryBase );
            }

            //SymbolTable.popScope();
        }
        else
        {
            if ( node.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
            {
                int d = 0;
                //node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d, false);
                Resolve( node.PrimaryBase );
            }
            else
            {
                Resolve( node.PrimaryBase );
            }
        }

        if ( node.CallEntries != null )
        {
            int i = 0;

            foreach ( CallEntry terminalNode in node.CallEntries )
            {
                if ( terminalNode.ArgumentsBase != null && terminalNode.ArgumentsBase.Expressions != null )
                {
                    foreach ( ExpressionBaseNode argumentsExpression in terminalNode.ArgumentsBase.Expressions )
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
                            Resolve( callElementEntry.CallBase );
                        }
                    }
                }

                if ( terminalNode.IsFunctionCall )
                {
                    /* LocalScope l = new LocalScope( SymbolTable.CurrentScope );
                     SymbolTable.CurrentScope.nest( l );
                     SymbolTable.pushScope( l );*/
                    if ( terminalNode.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
                    {
                        int d = 0;
                        /*DynamicVariable symbol = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d ) as DynamicVariable;
                        ClassSymbol classSymbol = symbol.resolve( symbol.Type.Name, out moduleId, ref d ) as ClassSymbol;
                        classSymbol.resolve( terminalNode.Primary.PrimaryId.Id, out moduleId, ref d );*/
                        Resolve( terminalNode.PrimaryBase);
                    }

                    //SymbolTable.popScope();
                }
                else
                {
                    if ( terminalNode.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
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
                        
                        Resolve( terminalNode.PrimaryBase );
                    }
                    
                }

                i++;
            }
        }

        return null;
    }

    public override object Visit( ArgumentsBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( ParametersBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        return null;
    }

    public override object Visit( AssignmentBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        switch ( node.Type )
        {
            case AssignmentTypes.Assignment:
                Resolve( node.CallBase );
                Resolve( node.AssignmentBase );

                break;

            case AssignmentTypes.Binary:
                Resolve( node.Binary );

                break;

            case AssignmentTypes.Ternary:
                Resolve( node.Ternary );

                break;

            case AssignmentTypes.Call:
                Resolve( node.CallBase );

                break;

            case AssignmentTypes.Primary:
                Resolve( node.PrimaryBaseNode );

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

    public override object Visit( ExpressionBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.AssignmentBase );

        return null;
    }

    public override object Visit( BlockStatementBaseNode node )
    {
        ResolveDeclarations( node.DeclarationsBase );

        return null;
    }

    public override object Visit( StatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( node is ExpressionStatementBaseNode expressionStatementNode )
        {
            Resolve( expressionStatementNode );

            return null;
        }

        if ( node is ForStatementBaseNode forStatementNode )
        {
            Resolve( forStatementNode );

            return null;
        }

        if ( node is IfStatementBaseNode ifStatementNode )
        {
            Resolve( ifStatementNode );

            return null;
        }

        if ( node is WhileStatementBaseNode whileStatement )
        {
            Resolve( whileStatement );

            return null;
        }

        if ( node is ReturnStatementBaseNode returnStatement )
        {
            Resolve( returnStatement );

            return null;
        }

        if ( node is BlockStatementBaseNode blockStatementNode )
        {
            Resolve( blockStatementNode );

            return null;
        }

        return null;
    }

    public override object Visit( ExpressionStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.ExpressionBase );

        return null;
    }

    public override object Visit( IfStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.ExpressionBase );
        Resolve(node.ThenStatementBase);

        if (node.ElseStatementBase != null)
        {
            Resolve(node.ElseStatementBase);
        }

        return null;
    }

    public override object Visit( ForStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        LocalScope l = new LocalScope( m_SymbolTable.CurrentScope );
        m_SymbolTable.CurrentScope.nest( l );
        m_SymbolTable.PushScope( l );

        if ( node.InitializerBase != null )
        {
            if ( node.InitializerBase.Expressions != null )
            {
                foreach ( var expression in node.InitializerBase.Expressions )
                {
                    Resolve( expression );
                }
            }
            else if ( node.InitializerBase.LocalVariableInitializerBase != null )
            {
                Resolve( node.InitializerBase.LocalVariableInitializerBase );
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

        if ( node.StatementBase != null )
        {
            Resolve( node.StatementBase );
        }

        m_SymbolTable.PopScope();

        return null;
    }

    public override object Visit( WhileStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.ExpressionBase );
        Resolve( node.WhileBlock );

        return null;
    }

    public override object Visit( ReturnStatementBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;

        if ( m_CurrentFunction == FunctionType.NONE )
        {
            return null;
        }

        if ( node.ExpressionStatementBase != null )
        {
            if ( m_CurrentFunction == FunctionType.CONSTRUCTOR )
            {
                return null;
            }

            Resolve( node.ExpressionStatementBase );
        }

        return null;
    }

    public override object Visit( InitializerBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.Expression );

        return null;
    }

    public override object Visit( BinaryOperationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( TernaryOperationBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        Resolve( node.LeftOperand );
        Resolve( node.MidOperand );
        Resolve( node.RightOperand );

        return null;
    }

    public override object Visit( PrimaryBaseNode node )
    {
        node.AstScopeNode = m_SymbolTable.CurrentScope;
        
        if ( node.PrimaryType == PrimaryBaseNode.PrimaryTypes.InterpolatedString )
        {
            foreach ( InterpolatedStringPart interpolatedStringStringPart in node.InterpolatedString.StringParts )
            {
                Resolve( interpolatedStringStringPart.ExpressionBaseNode );
            }
        }
        
        if ( node.PrimaryType == PrimaryBaseNode.PrimaryTypes.Expression )
        {
            Resolve( node.Expression );
        }

        return null;
    }

    public override object Visit( StructDeclarationBaseNode node )
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

    public override object Visit( AstBaseNode baseNode )
    {
        switch ( baseNode )
        {
            case ProgramBaseNode program:
                return Visit( program );

            case ModuleBaseNode module:
                return Visit( module );

            case ClassDeclarationBaseNode classDeclarationNode:
                return Visit( classDeclarationNode );

            case StructDeclarationBaseNode structDeclaration:
                return Visit( structDeclaration );

            case FunctionDeclarationBaseNode functionDeclarationNode:
                return Visit( functionDeclarationNode );
            
            case LocalVariableDeclarationBaseNode localVar:
                return Visit(localVar);

            case LocalVariableInitializerBaseNode initializer:
                return Visit(initializer);

            case VariableDeclarationBaseNode variable:
                return Visit( variable );

            case ClassInstanceDeclarationBaseNode classInstance:
                return Visit( classInstance );

            case UsingStatementBaseNode usingStatementNode:
                return Visit( usingStatementNode );

            case ExpressionStatementBaseNode expressionStatementNode:
                return Visit( expressionStatementNode );

            case ForStatementBaseNode forStatementNode:
                return Visit( forStatementNode );

            case WhileStatementBaseNode whileStatement:
                return Visit( whileStatement );

            case IfStatementBaseNode ifStatementNode:
                return Visit( ifStatementNode );

            case ReturnStatementBaseNode returnStatement:
                return Visit( returnStatement );

            case BlockStatementBaseNode blockStatementNode:
                return Visit( blockStatementNode );

            case AssignmentBaseNode assignmentNode:
                return Visit( assignmentNode );

            case CallBaseNode callNode:
                return Visit( callNode );

            case BinaryOperationBaseNode binaryOperation:
                return Visit( binaryOperation );

            case TernaryOperationBaseNode ternaryOperationNode:
                return Visit( ternaryOperationNode );

            case PrimaryBaseNode primaryNode:
                return Visit( primaryNode );

            case DeclarationsBaseNode declarationsNode:
                return Visit( declarationsNode );

            case UnaryPostfixOperation postfixOperation:
                return Visit( postfixOperation );

            case UnaryPrefixOperation prefixOperation:
                return Visit( prefixOperation );

            case StatementBaseNode stat:
                return Visit( stat );

            case ExpressionBaseNode expression:
                return Visit( expression );

            case InitializerBaseNode initializerNode:
                return Visit( initializerNode.Expression );

            default:
                return null;
        }
    }

    #endregion

    #region Private


    private object Resolve( AstBaseNode astBaseNode )
    {
        return astBaseNode.Accept( this );
    }

    private void ResolveDeclarations( DeclarationsBaseNode declarationsBaseNode )
    {
        Resolve( declarationsBaseNode );
    }

    #endregion
}

}
