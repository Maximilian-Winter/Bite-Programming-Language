using System;
using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime.Bytecode;
using Bite.Symbols;

namespace Bite.Runtime.CodeGen
{

public class CodeGenerator : HeteroAstVisitor < object >, IAstVisitor
{
    private int m_CurrentEnterBlockCount = 0;
    private string m_CurrentModuleName = "";
    private string m_CurrentClassName = "";

    private bool m_IsCompilingAssignmentLhs = false;
    private bool m_IsCompilingAssignmentRhs = false;
    
    private bool m_IsCompilingPostfixOperation = false;

    private int m_PreviousLoopBlockCount = 0;
    private BiteVmOpCodes m_ConstructingOpCode;
    private List < int > m_ConstructingOpCodeData;
    private int m_ConstructingLine;

    private BytecodeListStack m_PostfixInstructions = new BytecodeListStack();
    private BiteProgram m_BiteProgram;

    #region Public

    public CodeGenerator( BiteProgram biteProgram )
    {
        m_BiteProgram = biteProgram;
    }

    public override object Visit( ProgramNode node )
    {
        m_BiteProgram.PushChunk();

        //m_BiteProgram.SetCurrentChunk( SystemModuleCodeGenerator.GenerateFrom( node ) );

        //m_BiteProgram.SaveCurrentChunk( "System.CSharpInterface" );

        m_BiteProgram.PopChunk();

        foreach ( ModuleNode module in node.GetModulesInDepedencyOrder() )
        {
            Compile( module );
        }

        return null;
    }

    public override object Visit( ModuleNode node )
    {
        m_CurrentModuleName = node.ModuleIdent.ToString();

        m_BiteProgram.PushChunk();

        int d = 0;

        ModuleSymbol mod =
            m_BiteProgram.BaseScope.resolve( m_CurrentModuleName, out int moduleId, ref d ) as ModuleSymbol;

        // TODO: What happens when we encounter a module that has a name that is already in the program?

        if ( m_BiteProgram.HasChunk( m_CurrentModuleName ) )
        {
            m_BiteProgram.RestoreChunk( m_CurrentModuleName );
        }
        else
        {
            m_BiteProgram.CurrentChunk.
                          WriteToChunk(
                              BiteVmOpCodes.OpDefineModule,
                              new ConstantValue( m_CurrentModuleName ),
                              mod.NumberOfSymbols,
                              0 );

            m_BiteProgram.NewChunk();

            m_BiteProgram.SaveCurrentChunk( m_CurrentModuleName );
        }

        /*foreach ( ModuleIdentifier importedModule in node.ImportedModules )
        {
            EmitByteCode( BiteVmOpCodes.OpImportModule, importedModule.ToString(), importedModule.DebugInfoAstNode.LineNumber );
        }*/

        foreach ( StatementNode statement in node.Statements )
        {
            switch ( statement )
            {
                case ClassDeclarationNode classDeclarationNode:
                    Compile( classDeclarationNode );

                    break;

                case StructDeclarationNode structDeclaration:
                    Compile( structDeclaration );

                    break;

                case FunctionDeclarationNode functionDeclarationNode:
                    Compile( functionDeclarationNode );

                    break;

                case VariableDeclarationNode variable:
                    Compile( variable );

                    break;

                case ClassInstanceDeclarationNode classInstance:
                    Compile( classInstance );

                    break;

                case StatementNode stat:
                    Compile( stat );

                    break;
            }
        }

        m_BiteProgram.PopChunk();
        ;

        return null;
    }

    public override object Visit( ModifiersNode node )
    {
        return null;
    }

    public override object Visit( DeclarationNode node )
    {
        return null;
    }

    public override object Visit( UsingStatementNode node )
    {
        EmitByteCode( BiteVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, 0 );
        Compile( node.UsingNode );
        EmitByteCode( BiteVmOpCodes.OpUsingStatmentHead );
        Compile( node.UsingBlock );
        EmitByteCode( BiteVmOpCodes.OpUsingStatmentEnd );
        EmitByteCode( BiteVmOpCodes.OpExitBlock );

        return null;
    }

    public override object Visit( DeclarationsNode node )
    {
        EmitByteCode( BiteVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, 0 );

        m_CurrentEnterBlockCount++;

        if ( node.Classes != null )
        {
            foreach ( ClassDeclarationNode declaration in node.Classes )
            {
                Compile( declaration );
            }
        }

        if ( node.Structs != null )
        {
            foreach ( StructDeclarationNode declaration in node.Structs )
            {
                Compile( declaration );
            }
        }

        if ( node.Functions != null )
        {
            foreach ( FunctionDeclarationNode declaration in node.Functions )
            {
                Compile( declaration );
            }
        }

        if ( node.ClassInstances != null )
        {
            foreach ( ClassInstanceDeclarationNode declaration in node.ClassInstances )
            {
                Compile( declaration );
            }
        }

        if ( node.Variables != null )
        {
            foreach ( VariableDeclarationNode declaration in node.Variables )
            {
                Compile( declaration );
            }
        }

        if ( node.Statements != null )
        {
            foreach ( StatementNode declaration in node.Statements )
            {
                Compile( declaration );
            }
        }

        EmitByteCode( BiteVmOpCodes.OpExitBlock );
        m_CurrentEnterBlockCount--;

        return null;
    }

    public override object Visit( ClassDeclarationNode node )
    {
        int d = 0;
        ClassSymbol symbol = ( ClassSymbol ) node.AstScopeNode.resolve( node.ClassId.Id, out int moduleId, ref d );
        m_CurrentClassName = symbol.QualifiedName;

        m_BiteProgram.PushChunk();

        if ( m_BiteProgram.HasChunk( m_CurrentClassName ) )
        {
            m_BiteProgram.RestoreChunk( m_CurrentClassName );
        }
        else
        {
            EmitByteCode( BiteVmOpCodes.OpDefineClass, new ConstantValue( m_CurrentClassName ) );

            //EmitByteCode( symbol.InsertionOrderNumber );

            m_BiteProgram.NewChunk();
            m_BiteProgram.SaveCurrentChunk( m_CurrentClassName );
        }

        foreach ( FieldSymbol field in symbol.Fields )
        {
            if ( field.DefinitionNode != null &&
                 field.DefinitionNode is VariableDeclarationNode variableDeclarationNode )
            {
                Compile( variableDeclarationNode );
            }
            else if ( field.DefinitionNode != null &&
                      field.DefinitionNode is ClassInstanceDeclarationNode classInstance )
            {
                Compile( classInstance );
            }
            else
            {
                if ( !field.Name.Equals( "this" ) )
                {
                    EmitByteCode( BiteVmOpCodes.OpDefineVar );
                    EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( field.Name ) );
                }

                //EmitByteCode( field.Type );
            }
        }

        foreach ( MethodSymbol method in symbol.Methods )
        {
            if ( method.DefNode != null )
            {
                Compile( method.DefNode );
            }
            else
            {
                EmitByteCode( BiteVmOpCodes.OpDefineMethod, new ConstantValue( method.QualifiedName ) );

                //EmitByteCode( method.InsertionOrderNumber );
            }
        }

        EmitByteCode( BiteVmOpCodes.OpDefineVar );
        EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( "this" ) );
        m_BiteProgram.PopChunk();

        return null;
    }

    public override object Visit( FunctionDeclarationNode node )
    {
        m_BiteProgram.PushChunk();

        int d = 0;

        FunctionSymbol symbol =
            node.AstScopeNode.resolve( node.FunctionId.Id, out int moduleId, ref d ) as FunctionSymbol;

        EmitConstant( new ConstantValue(node.FunctionId.Id) );
        
        if ( m_BiteProgram.HasChunk( symbol.QualifiedName ) )
        {
            if (symbol.m_IsExtern && symbol.IsCallable)
            {
                EmitByteCode( BiteVmOpCodes.OpDefineCallableMethod, new ConstantValue( symbol.QualifiedName ) );
            }
            else
            {
                EmitByteCode( BiteVmOpCodes.OpDefineMethod, new ConstantValue( symbol.QualifiedName ) );
            }

            //m_CompilingChunk = CompilingChunks[symbol.QualifiedName];
            m_BiteProgram.RestoreChunk( symbol.QualifiedName );
        }
        else
        {
            if ( symbol.m_IsExtern && symbol.IsCallable )
            {
                EmitByteCode( BiteVmOpCodes.OpDefineCallableMethod, new ConstantValue( symbol.QualifiedName ) );
            }
            else
            {
                EmitByteCode( BiteVmOpCodes.OpDefineMethod, new ConstantValue( symbol.QualifiedName ) );
            }

            //EmitByteCode( symbol.InsertionOrderNumber );

            m_BiteProgram.NewChunk();
            m_BiteProgram.SaveCurrentChunk( symbol.QualifiedName );
        }

        if ( node.Parameters != null &&  node.Parameters.Identifiers != null )
        {
            foreach ( Identifier parametersIdentifier in node.Parameters.Identifiers )
            {
                EmitConstant(  new ConstantValue(parametersIdentifier.Id) );
            }

            EmitByteCode( BiteVmOpCodes.OpSetFunctionParameterName, node.Parameters.Identifiers.Count, 0 );
        }

        if ( node.FunctionBlock != null )
        {
            Compile( node.FunctionBlock );
        }

        m_BiteProgram.PopChunk();

        return null;
    }

    public override object Visit( VariableDeclarationNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol =
            node.AstScopeNode.resolve( node.VarId.Id, out int moduleId, ref d ) as DynamicVariable;

        if ( node.Initializer != null )
        {
            m_PostfixInstructions.Push( new BytecodeList() );
            m_IsCompilingAssignmentRhs = true;
            Compile( node.Initializer );
            m_IsCompilingAssignmentRhs = false;
            EmitByteCode( BiteVmOpCodes.OpDefineVar );
            EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ) );
            
            BytecodeList byteCodes = m_PostfixInstructions.Pop();

            foreach ( ByteCode code in byteCodes.ByteCodes )
            {
                EmitByteCode( code );
            }
        }
        else
        {
            EmitByteCode( BiteVmOpCodes.OpDeclareVar );
            EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ) );
        }

        return null;
    }

    public override object Visit( ClassInstanceDeclarationNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol =
            node.AstScopeNode.resolve( node.InstanceId.Id, out int moduleId, ref d ) as DynamicVariable;

        int d2 = 0;

        ClassSymbol classSymbol =
            node.AstScopeNode.resolve( node.ClassName.Id, out int moduleId2, ref d2 ) as ClassSymbol;

        if ( node.IsVariableRedeclaration )
        {
            ByteCode byteCode = new ByteCode(
                BiteVmOpCodes.OpSetInstance,
                moduleId,
                d,
                variableSymbol.InsertionOrderNumber,
                moduleId2,
                d2,
                classSymbol.InsertionOrderNumber,
                classSymbol.NumberOfSymbols );

            EmitByteCode( byteCode );
            EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ) );
        }
        else
        {
            ByteCode byteCode = new ByteCode(
                BiteVmOpCodes.OpDefineInstance,
                moduleId2,
                d2,
                classSymbol.InsertionOrderNumber,
                classSymbol.NumberOfSymbols );

            EmitByteCode( byteCode );
            EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ) );
        }

        if ( node.Arguments != null && node.Arguments.Expressions.Count > 0 )
        {
            foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
            {
                if ( methodSymbol.IsConstructor && node.Arguments.Expressions.Count == methodSymbol.NumberOfParameters )
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpGetVar,
                        moduleId,
                        d,
                        -1,
                        variableSymbol.InsertionOrderNumber );
                    
                    ByteCode byteCode2 = new ByteCode(
                        BiteVmOpCodes.OpGetNextVarByRef );

                    m_PostfixInstructions.Push( new BytecodeList() );
                    foreach ( ExpressionNode argument in node.Arguments.Expressions )
                    {
                        Compile( argument );
                    }
                    
                    
                    EmitByteCode( BiteVmOpCodes.OpBindToFunction, node.Arguments.Expressions.Count, 0 );
                    BytecodeList byteCodes = m_PostfixInstructions.Pop();

                    foreach ( ByteCode code in byteCodes.ByteCodes )
                    {
                        EmitByteCode( code );
                    }
                    EmitByteCode( byteCode2 );
                    EmitByteCode( byteCode );
                    EmitByteCode( BiteVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.Name ) );
                }
            }
        }
        else
        {
            foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
            {
                if ( methodSymbol.IsConstructor && methodSymbol.NumberOfParameters == 0 )
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpGetVar,
                        moduleId,
                        d,
                        -1,
                        variableSymbol.InsertionOrderNumber );

                    ByteCode byteCode2 = new ByteCode(
                        BiteVmOpCodes.OpGetNextVarByRef );
                    EmitByteCode( byteCode2 );
                    EmitByteCode( byteCode );
                    EmitByteCode( BiteVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.Name ) );
                }
            }
        }

        /*foreach ( var methodSymbol in classSymbol.Methods )
        {
            if ( methodSymbol.IsConstructor )
            {
                ByteCode byteCode = new ByteCode(
                    BiteVmOpCodes.OpGetLocalInstance,
                    moduleId,
                    d,
                    variableSymbol.InsertionOrderNumber);
                
                EmitByteCode(byteCode);
                EmitByteCode( BiteVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.QualifiedName ) );
            }
        }*/
        return null;
    }

    public override object Visit( CallNode node )
    {
        if ( node.Arguments != null && node.Arguments.Expressions != null )
        {
            m_PostfixInstructions.Push( new BytecodeList() );
            foreach ( ExpressionNode argument in node.Arguments.Expressions )
            {
                Compile( argument );
            }
           
            ByteCode byteCode = new ByteCode(
                BiteVmOpCodes.OpBindToFunction,
                node.Arguments.Expressions.Count );

            EmitByteCode( byteCode );
            
            BytecodeList byteCodes = m_PostfixInstructions.Pop();

            foreach ( ByteCode code in byteCodes.ByteCodes )
            {
                EmitByteCode( code );
            }
        }

        if ( node.IsFunctionCall )
        {
            int d = 0;

            if ( node.ElementAccess != null )
            {
                if ( node.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
                {
                    if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                    {
                        int d2 = 0;
                        if ( node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId2, ref d2, false ) != null )
                        {
                            BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpSetVar );
                            Compile( node.Primary );
                        }
                        else
                        {
                            EmitByteCode( BiteVmOpCodes.OpSetVarExternal, new ConstantValue( node.Primary.PrimaryId.Id ) );
                        }
                    
                    
                    }
                    else
                    {
                        int d2 = 0;
                        Symbol var = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId2, ref d2, false );

                        if ( var is ModuleSymbol m )
                        {
                            BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpGetModule );
                            AddToConstuctingByteCodeInstruction( m.InsertionOrderNumber );
                            EndConstuctingByteCodeInstruction();
                        }
                        else
                        {
                            if ( var != null )
                            {
                                BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpGetVar );
                                Compile( node.Primary );
                               
                            }
                            else
                            {
                                EmitByteCode( BiteVmOpCodes.OpGetVarExternal, new ConstantValue( node.Primary.PrimaryId.Id ) );
                            }
                     
                        }
                    }
                }
                else
                {
                    Compile( node.Primary );
                }
                foreach ( CallElementEntry callElementEntry in node.ElementAccess )
                {
                    if ( callElementEntry.CallElementType == CallElementTypes.Call )
                    {
                        Compile( callElementEntry.Call );
                    }
                    else
                    {
                        EmitConstant( new ConstantValue( callElementEntry.Identifier ) );
                    }
                }

                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpSetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode );
                }
                else
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpGetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode );
                }

                int dObject = 0;

                Symbol objectToCall = node.AstScopeNode.resolve(
                    node.Primary.PrimaryId.Id,
                    out int moduleIdObject,
                    ref dObject, false );
                
                ByteCode byteCodeObj = new ByteCode(
                    BiteVmOpCodes.OpGetVar,
                    moduleIdObject,
                    dObject,
                    -1,
                    objectToCall.InsertionOrderNumber );

                ByteCode byteCode2 = new ByteCode(
                    BiteVmOpCodes.OpGetNextVarByRef );
                EmitByteCode( byteCode2 );
                EmitByteCode( byteCodeObj );
                
                EmitByteCode( BiteVmOpCodes.OpCallFunctionFromStack );
            }
            else
            {
                EmitByteCode( BiteVmOpCodes.OpCallFunctionByName, new ConstantValue( node.Primary.PrimaryId.Id ) );
            }
        }
        else
        {
            if ( node.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
            {
                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    int d = 0;
                    if ( node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d , false) != null )
                    {
                        BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpSetVar );
                        Compile( node.Primary );
                    }
                    else
                    {
                        EmitByteCode( BiteVmOpCodes.OpSetVarExternal, new ConstantValue( node.Primary.PrimaryId.Id ) );
                    }
                    
                    
                }
                else
                {
                    int d = 0;
                    Symbol var = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d, false );

                    if ( var is ModuleSymbol m )
                    {
                        BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpGetModule );
                        AddToConstuctingByteCodeInstruction( m.InsertionOrderNumber );
                        EndConstuctingByteCodeInstruction();
                    }
                    else
                    {
                        if ( var.IsExternal )
                        {
                            EmitByteCode( BiteVmOpCodes.OpGetVarExternal,
                                new ConstantValue( node.Primary.PrimaryId.Id ) );
                        }
                        else
                        {
                            BeginConstuctingByteCodeInstruction( BiteVmOpCodes.OpGetVar );
                            Compile( node.Primary );
                        }

                    }
                }
            }
            else
            {
                Compile( node.Primary );
            }
            if ( node.ElementAccess != null )
            {
                foreach ( CallElementEntry callElementEntry in node.ElementAccess )
                {
                    if ( callElementEntry.CallElementType == CallElementTypes.Call )
                    {
                        Compile( callElementEntry.Call );
                    }
                    else
                    {
                        EmitConstant( new ConstantValue( callElementEntry.Identifier ) );
                    }
                }

                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpSetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode );
                }
                else
                {
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpGetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode );
                }
            }
        }

        

        if ( node.CallEntries != null )
        {
            int i = 0;
            int d = 0;
            int d2 = 0;
            bool isModuleSymbol = false;
            bool isClassSymbol = false;
            Symbol var = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d, false );
            ModuleSymbol moduleSymbol = null;
            ClassSymbol classSymbol = null;

            if ( var is ModuleSymbol m )
            {
                moduleSymbol = m;
                isModuleSymbol = true;
            }
            else
            {
                if ( var is DynamicVariable dynamicVariable && dynamicVariable.Type != null )
                {
                    classSymbol =
                        node.AstScopeNode.resolve(
                            dynamicVariable.Type.Name,
                            out int moduleId2,
                            ref d2, false ) as ClassSymbol;

                    isClassSymbol = true;
                }
            }

            //DynamicVariable dynamicVariable = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d) as DynamicVariable;

            foreach ( CallEntry terminalNode in node.CallEntries )
            {
                if ( terminalNode.Arguments != null && terminalNode.Arguments.Expressions != null )
                {
                    
                    m_PostfixInstructions.Push( new BytecodeList() );
                    foreach ( ExpressionNode argumentsExpression in terminalNode.Arguments.Expressions )
                    {
                        Compile( argumentsExpression );
                    }
                    
                    
                    ByteCode byteCode = new ByteCode(
                        BiteVmOpCodes.OpBindToFunction,
                        terminalNode.Arguments.Expressions.Count );
                    EmitByteCode( byteCode );
                    BytecodeList byteCodes = m_PostfixInstructions.Pop();

                    foreach ( ByteCode code in byteCodes.ByteCodes )
                    {
                        EmitByteCode( code );
                    }
                }

                if ( terminalNode.IsFunctionCall )
                {
                    if ( isModuleSymbol )
                    {

                        EmitByteCode(
                            BiteVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                    }
                    else if ( isClassSymbol )
                    {
                        EmitByteCode(
                            BiteVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                    }
                    else
                    {
                        EmitByteCode(
                            BiteVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                    }
                }
                else
                {
                    if ( terminalNode.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier )
                    {
                        if ( m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1 )
                        {
                            if ( isModuleSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = moduleSymbol.resolve(
                                    terminalNode.Primary.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4,false );

                                ByteCode byteCode = new ByteCode(
                                    BiteVmOpCodes.OpSetMember,
                                    memberSymbol.InsertionOrderNumber );

                                EmitByteCode( byteCode );
                            }
                            else if ( isClassSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = classSymbol.resolve(
                                    terminalNode.Primary.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4, false );

                                if ( memberSymbol == null )
                                {
                                    EmitByteCode(
                                        BiteVmOpCodes.OpSetMemberWithString,
                                        new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                                }
                                else
                                {
                                    ByteCode byteCode = new ByteCode(
                                        BiteVmOpCodes.OpSetMember,
                                        memberSymbol.InsertionOrderNumber );

                                    EmitByteCode( byteCode );
                                }
                            }
                            else
                            {
                                EmitByteCode(
                                    BiteVmOpCodes.OpSetMemberWithString,
                                    new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                            }
                        }
                        else
                        {
                            if ( isModuleSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = moduleSymbol.resolve(
                                    terminalNode.Primary.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4, false );

                                ByteCode byteCode = new ByteCode(
                                    BiteVmOpCodes.OpGetMember,
                                    memberSymbol.InsertionOrderNumber );

                                EmitByteCode( byteCode );
                            }
                            else if ( isClassSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = classSymbol.resolve(
                                    terminalNode.Primary.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4, false );

                                if ( memberSymbol == null )
                                {
                                    EmitByteCode(
                                        BiteVmOpCodes.OpGetMemberWithString,
                                        new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                                }
                                else
                                {
                                    ByteCode byteCode = new ByteCode(
                                        BiteVmOpCodes.OpGetMember,
                                        memberSymbol.InsertionOrderNumber );

                                    EmitByteCode( byteCode );
                                }
                            }
                            else
                            {
                                EmitByteCode(
                                    BiteVmOpCodes.OpGetMemberWithString,
                                    new ConstantValue( terminalNode.Primary.PrimaryId.Id ) );
                            }
                        }
                    }
                }

                if ( terminalNode.ElementAccess != null )
                {
                    foreach ( CallElementEntry callElementEntry in terminalNode.ElementAccess )
                    {
                        if ( callElementEntry.CallElementType == CallElementTypes.Call )
                        {
                            Compile( callElementEntry.Call );
                        }
                        else
                        {
                            EmitConstant( new ConstantValue( callElementEntry.Identifier ) );
                        }
                    }

                    if ( m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1 )
                    {
                        ByteCode byteCode = new ByteCode(
                            BiteVmOpCodes.OpSetElement,
                            terminalNode.ElementAccess.Count );

                        EmitByteCode( byteCode );
                    }
                    else
                    {
                        ByteCode byteCode = new ByteCode(
                            BiteVmOpCodes.OpGetElement,
                            terminalNode.ElementAccess.Count );

                        EmitByteCode( byteCode );
                    }
                }

                i++;
            }
        }

        return null;
    }

    public override object Visit( ArgumentsNode node )
    {
        return null;
    }

    public override object Visit( ParametersNode node )
    {
        return null;
    }

    public override object Visit( AssignmentNode node )
    {
        switch ( node.Type )
        {
            case AssignmentTypes.Assignment:
                if ( m_IsCompilingAssignmentRhs )
                {
                    EmitByteCode( BiteVmOpCodes.OpPushNextAssignmentOnStack, 0 );
                }
                m_PostfixInstructions.Push( new BytecodeList() );
                m_IsCompilingAssignmentRhs = true;
                Compile( node.Assignment );
                m_IsCompilingAssignmentRhs = false;
                m_IsCompilingAssignmentLhs = true;
                Compile( node.Call );
                m_IsCompilingAssignmentLhs = false;

                switch ( node.OperatorType )
                {
                    case AssignmentOperatorTypes.Assign:
                        EmitByteCode( BiteVmOpCodes.OpAssign );

                        break;

                    case AssignmentOperatorTypes.DivAssign:
                        EmitByteCode( BiteVmOpCodes.OpDivideAssign );

                        break;

                    case AssignmentOperatorTypes.MultAssign:
                        EmitByteCode( BiteVmOpCodes.OpMultiplyAssign );

                        break;

                    case AssignmentOperatorTypes.PlusAssign:
                        EmitByteCode( BiteVmOpCodes.OpPlusAssign );

                        break;

                    case AssignmentOperatorTypes.MinusAssign:
                        EmitByteCode( BiteVmOpCodes.OpMinusAssign );

                        break;

                    case AssignmentOperatorTypes.ModuloAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpModuloAssign );

                        break;

                    case AssignmentOperatorTypes.BitwiseAndAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpBitwiseAndAssign );

                        break;

                    case AssignmentOperatorTypes.BitwiseOrAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpBitwiseOrAssign );

                        break;

                    case AssignmentOperatorTypes.BitwiseXorAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpBitwiseXorAssign );

                        break;

                    case AssignmentOperatorTypes.BitwiseLeftShiftAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpBitwiseLeftShiftAssign );

                        break;

                    case AssignmentOperatorTypes.BitwiseRightShiftAssignOperator:
                        EmitByteCode( BiteVmOpCodes.OpBitwiseRightShiftAssign );

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                BytecodeList byteCodes = m_PostfixInstructions.Pop();

                foreach ( ByteCode code in byteCodes.ByteCodes )
                {
                    EmitByteCode( code );
                }
                break;

            case AssignmentTypes.Binary:
                Compile( node.Binary );

                break;

            case AssignmentTypes.Ternary:
                Compile( node.Ternary );

                break;

            case AssignmentTypes.Call:
                Compile( node.Call );

                break;

            case AssignmentTypes.Primary:
                Compile( node.PrimaryNode );

                break;

            case AssignmentTypes.UnaryPostfix:
                Compile( node.UnaryPostfix );

                break;

            case AssignmentTypes.UnaryPrefix:
                Compile( node.UnaryPrefix );

                break;

            default:
                throw new Exception( "Invalid Type" );
        }

        return null;
    }

    public override object Visit( ExpressionNode node )
    {
        Compile( node.Assignment );

        return null;
    }

    public override object Visit( BlockStatementNode node )
    {
        Compile( node.Declarations );

        return null;
    }

    public override object Visit( StatementNode node )
    {
        if ( node is ExpressionStatementNode expressionStatementNode )
        {
            Compile( expressionStatementNode );

            return null;
        }

        if ( node is ForStatementNode forStatementNode )
        {
            Compile( forStatementNode );

            return null;
        }

        if ( node is IfStatementNode ifStatementNode )
        {
            Compile( ifStatementNode );

            return null;
        }

        if ( node is WhileStatementNode whileStatement )
        {
            Compile( whileStatement );

            return null;
        }

        if ( node is ReturnStatementNode returnStatement )
        {
            Compile( returnStatement );

            return null;
        }

        if ( node is BlockStatementNode blockStatementNode )
        {
            Compile( blockStatementNode );

            return null;
        }

        return null;
    }

    public override object Visit( ExpressionStatementNode node )
    {
        Compile( node.Expression );

        return null;
    }

    public override object Visit( IfStatementNode node )
    {
        Compile( node.Expression );
        int thenJump = EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );
        Compile( node.ThenStatement );
        int overElseJump = EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );

        m_BiteProgram.CurrentChunk.Code[thenJump] = new ByteCode(
            BiteVmOpCodes.OpJumpIfFalse,
            m_BiteProgram.CurrentChunk.SerializeToBytes().Length );

        Stack < int > endJumpStack = new Stack < int >();

        if (node.ElseStatement != null)
        {
            Compile(node.ElseStatement);
        }

        int endJumpStackCount = endJumpStack.Count;

        for ( int i = 0; i < endJumpStackCount; i++ )
        {
            int endJump = endJumpStack.Pop();

            m_BiteProgram.CurrentChunk.Code[endJump] = new ByteCode(
                BiteVmOpCodes.OpJump,
                m_BiteProgram.CurrentChunk.SerializeToBytes().Length );
        }

        m_BiteProgram.CurrentChunk.Code[overElseJump] = new ByteCode(
            BiteVmOpCodes.OpJump,
            m_BiteProgram.CurrentChunk.SerializeToBytes().Length );

        return null;
    }

    public override object Visit( LocalVariableDeclarationNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol = node.AstScopeNode.resolve( node.VarId.Id, out int moduleId, ref d ) as DynamicVariable;

        Compile( node.Expression );
        EmitByteCode( BiteVmOpCodes.OpDefineVar );
        EmitByteCode( BiteVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ) );

        return null;
    }

    public override object Visit( LocalVariableInitializerNode node )
    {
        foreach ( var variableDeclaration in node.VariableDeclarations )
        {
            Compile( variableDeclaration );
        }

        return null;
    }

    public override object Visit( ForStatementNode node )
    {
        EmitByteCode( BiteVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, 0 );
        m_CurrentEnterBlockCount++;
        m_PreviousLoopBlockCount = m_CurrentEnterBlockCount;

        if (node.Initializer != null)
        {
            if (node.Initializer.Expressions != null)
            {
                foreach (var expression in node.Initializer.Expressions)
                {
                    Compile(expression);
                }
            }
            else if (node.Initializer.LocalVariableInitializer != null)
            {
                Compile(node.Initializer.LocalVariableInitializer);
            }
        }

        int jumpCodeWhileBegin = m_BiteProgram.CurrentChunk.SerializeToBytes().Length;

        if ( node.Condition != null )
        {
            Compile( node.Condition);
        }
        else
        {
            EmitConstant( new ConstantValue( true ) );
        }

        int toFix = EmitByteCode( BiteVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, 0 );

        if ( node.Statement != null )
        {
            Compile( node.Statement );
        }
        
        if ( node.Iterators != null )
        {
            foreach ( var iterator in node.Iterators )
            {
                Compile( iterator );
            }
        }

        /*if ( node.Iterators != null )
        {
            foreach ( var iterator in node.Iterators )
            {
                EmitByteCode( BiteVmOpCodes.OpPopStack );
            }
        }*/
        
        m_BiteProgram.CurrentChunk.Code[toFix] = new ByteCode(
        BiteVmOpCodes.OpWhileLoop,
        jumpCodeWhileBegin,
        m_BiteProgram.CurrentChunk.SerializeToBytes().Length );

        EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );
        EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );
        EmitByteCode( BiteVmOpCodes.OpExitBlock );
        m_CurrentEnterBlockCount--;

        return null;
    }

    public override object Visit( WhileStatementNode node )
    {
        m_PreviousLoopBlockCount = m_CurrentEnterBlockCount;

        int jumpCodeWhileBegin = m_BiteProgram.CurrentChunk.SerializeToBytes().Length;
        Compile( node.Expression );
        int toFix = EmitByteCode( BiteVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, 0 );
        Compile( node.WhileBlock );

        m_BiteProgram.CurrentChunk.Code[toFix] = new ByteCode(
            BiteVmOpCodes.OpWhileLoop,
            jumpCodeWhileBegin,
            m_BiteProgram.CurrentChunk.SerializeToBytes().Length );

        EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );
        EmitByteCode( BiteVmOpCodes.OpNone, 0, 0 );

        return null;
    }

    public override object Visit( ReturnStatementNode node )
    {
        Compile( node.ExpressionStatement );
        EmitByteCode( BiteVmOpCodes.OpKeepLastItemOnStack );

        for ( int i = 0; i < m_CurrentEnterBlockCount; i++ )
        {
            EmitByteCode( BiteVmOpCodes.OpExitBlock );
        }

        EmitReturn();

        return null;
    }

    public override object Visit( BreakStatementNode node )
    {
        for ( int i = 0; i < m_CurrentEnterBlockCount - m_PreviousLoopBlockCount; i++ )
        {
            EmitByteCode( BiteVmOpCodes.OpExitBlock );
        }

        EmitByteCode( BiteVmOpCodes.OpBreak );

        return null;
    }

    public override object Visit( InitializerNode node )
    {
        Compile( node.Expression );

        return null;
    }

    public override object Visit( BinaryOperationNode node )
    {
        Compile( node.LeftOperand );
        Compile( node.RightOperand );

        switch ( node.Operator )
        {
            case BinaryOperationNode.BinaryOperatorType.Plus:
                EmitByteCode( BiteVmOpCodes.OpAdd );

                break;

            case BinaryOperationNode.BinaryOperatorType.Minus:
                EmitByteCode( BiteVmOpCodes.OpSubtract );

                break;

            case BinaryOperationNode.BinaryOperatorType.Mult:
                EmitByteCode( BiteVmOpCodes.OpMultiply );

                break;

            case BinaryOperationNode.BinaryOperatorType.Div:
                EmitByteCode( BiteVmOpCodes.OpDivide );

                break;

            case BinaryOperationNode.BinaryOperatorType.Modulo:
                EmitByteCode( BiteVmOpCodes.OpModulo );

                break;

            case BinaryOperationNode.BinaryOperatorType.Equal:
                EmitByteCode( BiteVmOpCodes.OpEqual );

                break;

            case BinaryOperationNode.BinaryOperatorType.NotEqual:
                EmitByteCode( BiteVmOpCodes.OpNotEqual );

                break;

            case BinaryOperationNode.BinaryOperatorType.Less:
                EmitByteCode( BiteVmOpCodes.OpLess );

                break;

            case BinaryOperationNode.BinaryOperatorType.LessOrEqual:
                EmitByteCode( BiteVmOpCodes.OpLessOrEqual );

                break;

            case BinaryOperationNode.BinaryOperatorType.Greater:
                EmitByteCode( BiteVmOpCodes.OpGreater );

                break;

            case BinaryOperationNode.BinaryOperatorType.GreaterOrEqual:
                EmitByteCode( BiteVmOpCodes.OpGreaterEqual );

                break;

            case BinaryOperationNode.BinaryOperatorType.And:
                EmitByteCode( BiteVmOpCodes.OpAnd );

                break;

            case BinaryOperationNode.BinaryOperatorType.Or:
                EmitByteCode( BiteVmOpCodes.OpOr );

                break;

            case BinaryOperationNode.BinaryOperatorType.BitwiseOr:
                EmitByteCode( BiteVmOpCodes.OpBitwiseOr );

                break;

            case BinaryOperationNode.BinaryOperatorType.BitwiseAnd:
                EmitByteCode( BiteVmOpCodes.OpBitwiseAnd );

                break;

            case BinaryOperationNode.BinaryOperatorType.BitwiseXor:
                EmitByteCode( BiteVmOpCodes.OpBitwiseXor );

                break;

            case BinaryOperationNode.BinaryOperatorType.ShiftLeft:
                EmitByteCode( BiteVmOpCodes.OpBitwiseLeftShift );

                break;

            case BinaryOperationNode.BinaryOperatorType.ShiftRight:
                EmitByteCode( BiteVmOpCodes.OpBitwiseRightShift );

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override object Visit( TernaryOperationNode node )
    {
        Compile( node.RightOperand );
        Compile( node.MidOperand );
        Compile( node.LeftOperand );
        EmitByteCode( BiteVmOpCodes.OpTernary );

        return null;
    }

    public override object Visit( PrimaryNode node )
    {
        switch ( node.PrimaryType )
        {
            case PrimaryNode.PrimaryTypes.Identifier:
            {
                int d = 0;
                Symbol symbol = node.AstScopeNode.resolve( node.PrimaryId.Id, out int moduleId, ref d );
                AddToConstuctingByteCodeInstruction( moduleId );
                AddToConstuctingByteCodeInstruction( d );

                if ( symbol == null )
                {
                    throw new CompilerException(
                        $"Failed to resolve symbol '{node.PrimaryId.Id}' in scope '{node.AstScopeNode.Name}'", node );
                }

                if ( symbol.SymbolScope is ClassSymbol s )
                {
                    AddToConstuctingByteCodeInstruction( s.InsertionOrderNumber );
                }
                else
                {
                    AddToConstuctingByteCodeInstruction( -1 );
                }

                AddToConstuctingByteCodeInstruction( symbol.InsertionOrderNumber );
                EndConstuctingByteCodeInstruction();
               
                return null;
            }

            case PrimaryNode.PrimaryTypes.ThisReference:
                int d2 = 0;
                Symbol thisSymbol = node.AstScopeNode.resolve( "this", out int moduleId2, ref d2 );
                AddToConstuctingByteCodeInstruction( moduleId2 );
                AddToConstuctingByteCodeInstruction( d2 );
                AddToConstuctingByteCodeInstruction( thisSymbol.InsertionOrderNumber );
                EndConstuctingByteCodeInstruction();

                return null;

            case PrimaryNode.PrimaryTypes.BooleanLiteral:
                EmitConstant( new ConstantValue( node.BooleanLiteral.Value ) );

                return null;

            case PrimaryNode.PrimaryTypes.IntegerLiteral:
                EmitConstant( new ConstantValue( node.IntegerLiteral.Value ) );

                return null;

            case PrimaryNode.PrimaryTypes.FloatLiteral:
                EmitConstant( new ConstantValue( node.FloatLiteral.Value ) );

                return null;

            case PrimaryNode.PrimaryTypes.StringLiteral:
                EmitConstant( new ConstantValue( node.StringLiteral ) );

                return null;
            
            case PrimaryNode.PrimaryTypes.InterpolatedString:
                int i = 0;
                foreach ( InterpolatedStringPart interpolatedStringStringPart in node.InterpolatedString.StringParts )
                {
                    EmitConstant( new ConstantValue( interpolatedStringStringPart.TextBeforeExpression ) );
                    if ( i > 0 )
                    {
                        EmitByteCode( BiteVmOpCodes.OpAdd );
                    }
                    Compile( interpolatedStringStringPart.ExpressionNode );
                    EmitByteCode( BiteVmOpCodes.OpAdd );
                    i++;
                }
                EmitConstant( new ConstantValue( node.InterpolatedString.TextAfterLastExpression ) );
                EmitByteCode( BiteVmOpCodes.OpAdd );
                return null;

            case PrimaryNode.PrimaryTypes.Expression:
                node.Expression.Accept( this );

                return null;

            case PrimaryNode.PrimaryTypes.NullReference:
                EmitConstant( new ConstantValue( null ) );

                return null;

            case PrimaryNode.PrimaryTypes.Default:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof( node.PrimaryType ),
                    node.PrimaryType,
                    null );
        }
    }

    public override object Visit( StructDeclarationNode node )
    {
        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        m_IsCompilingPostfixOperation = true;
        int toFix = -1;
        if ( m_PostfixInstructions.Count == 0 )
        {
            toFix = EmitByteCode( BiteVmOpCodes.OpNone );
        }
        
        Compile( node.Primary );
        
        if ( toFix >= 0 && m_BiteProgram.CurrentChunk.Code[toFix + 1].OpCode == BiteVmOpCodes.OpGetVar )
        {
            m_BiteProgram.CurrentChunk.Code[toFix] = new ByteCode(
                BiteVmOpCodes.OpGetNextVarByRef );
        }
        m_IsCompilingPostfixOperation = false;
        
        
        switch ( node.Operator )
        {
            case UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus:
                if ( m_PostfixInstructions.Count == 0 )
                {
                    EmitByteCode( BiteVmOpCodes.OpPostfixIncrement );
                }
                else
                {
                    if ( m_PostfixInstructions.Count > 0 )
                    {
                        m_PostfixInstructions.Peek().ByteCodes.Add( new ByteCode( BiteVmOpCodes.OpPostfixIncrement ) );
                    }
                }
                
                break;

            case UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus:
                if ( m_PostfixInstructions.Count == 0 )
                {
                    EmitByteCode( BiteVmOpCodes.OpPostfixDecrement );
                }
                else
                {
                    if ( m_PostfixInstructions.Count > 0 )
                    {
                        m_PostfixInstructions.Peek().ByteCodes.Add( new ByteCode( BiteVmOpCodes.OpPostfixDecrement ) );
                    }
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {

        Compile( node.Primary );

        switch ( node.Operator )
        {
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Plus:
                EmitByteCode( BiteVmOpCodes.OpAffirm );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment:
                EmitByteCode( BiteVmOpCodes.OpCompliment );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus:
                EmitByteCode( BiteVmOpCodes.OpPrefixIncrement );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus:
                EmitByteCode( BiteVmOpCodes.OpPrefixDecrement );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot:
                EmitByteCode( BiteVmOpCodes.OpNot );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.Negate:
                EmitByteCode( BiteVmOpCodes.OpNegate );

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

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
                return Visit( localVar );

            case LocalVariableInitializerNode initializer:
                return Visit( initializer );

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

            case BreakStatementNode returnStatement:
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

    private void AddToConstuctingByteCodeInstruction( int opCodeData )
    {
        m_ConstructingOpCodeData.Add( opCodeData );
    }

    private void BeginConstuctingByteCodeInstruction( BiteVmOpCodes byteCode, int line = 0 )
    {
        m_ConstructingOpCode = byteCode;
        m_ConstructingOpCodeData = new List < int >();
        m_ConstructingLine = line;
    }

    public void Compile( ProgramNode programNode )
    {
        programNode.Accept( this );
    }

    public void Compile( ModuleNode moduleNode )
    {
        moduleNode.Accept( this );
    }

    private object Compile( HeteroAstNode astNode )
    {
        return astNode.Accept( this );
    }

    private int EmitByteCode( BiteVmOpCodes byteCode, int line = 0 )
    {
        return m_BiteProgram.CurrentChunk.WriteToChunk( byteCode, line );
    }

    private int EmitByteCode( BiteVmOpCodes byteCode, int opCodeData, int line )
    {
        ByteCode byCode = new ByteCode( byteCode, opCodeData );

        return m_BiteProgram.CurrentChunk.WriteToChunk( byCode, line );
    }

    private int EmitByteCode( BiteVmOpCodes byteCode, int opCodeData, int opCodeData2, int line )
    {
        ByteCode byCode = new ByteCode( byteCode, opCodeData, opCodeData2 );

        return m_BiteProgram.CurrentChunk.WriteToChunk( byCode, line );
    }

    private int EmitByteCode( ByteCode byteCode, int line = 0 )
    {
        return m_BiteProgram.CurrentChunk.WriteToChunk( byteCode, line );
    }

    private int EmitByteCode( BiteVmOpCodes byteCode, ConstantValue constantValue, int line = 0 )
    {
        return m_BiteProgram.CurrentChunk.WriteToChunk( byteCode, constantValue, line );
    }

    private int EmitConstant( ConstantValue value, int line = 0 )
    {
        return EmitByteCode( BiteVmOpCodes.OpConstant, value, line );
    }

    private int EmitReturn( int line = 0 )
    {
        return EmitByteCode( BiteVmOpCodes.OpReturn, line );
    }

    private void EndConstuctingByteCodeInstruction()
    {
        ByteCode byCode = new ByteCode( m_ConstructingOpCode );
        byCode.OpCodeData = m_ConstructingOpCodeData.ToArray();
        m_BiteProgram.CurrentChunk.WriteToChunk( byCode, m_ConstructingLine );
        if (m_PostfixInstructions.Count > 0 && m_IsCompilingPostfixOperation)
        {
            if (m_ConstructingOpCode == BiteVmOpCodes.OpGetVar)
            {
                ByteCode byCodeAlt = new ByteCode( BiteVmOpCodes.OpGetNextVarByRef );
                m_PostfixInstructions.Peek().ByteCodes.Add( byCodeAlt );
            }
            m_PostfixInstructions.Peek().ByteCodes.Add( byCode );
        }
        m_ConstructingOpCodeData = null;
        m_ConstructingOpCode = BiteVmOpCodes.OpNone;
    }
        #endregion
    }

}
