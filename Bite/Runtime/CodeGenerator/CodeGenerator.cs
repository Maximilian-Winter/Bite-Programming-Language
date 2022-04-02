using System;
using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime.Bytecode;
using Bite.SymbolTable;

namespace Bite.Runtime.CodeGen
{
    public class CompilerException : Exception
    {
        public CompilerException(string message) : base(message)
        {

        }
    }

    public class CodeGenerator : HeteroAstVisitor<object>, IAstVisitor
    {
        private int m_CurrentEnterBlockCount = 0;
        private string m_CurrentModuleName = "";
        private string m_CurrentClassName = "";

        private bool m_IsCompilingAssignmentLhs = false;

        private SrslVmOpCodes m_CallNodeTypeForAssignment = SrslVmOpCodes.OpNone;

        private int m_MaxStackDepth = 0;

        private int PreviousLoopBlockCount = 0;
        private SrslVmOpCodes m_ConstructingOpCode;
        private List<int> m_ConstructingOpCodeData;
        private int m_ConstructingLine;

        private BiteProgram m_BiteProgram;

        private void BeginConstuctingByteCodeInstruction(SrslVmOpCodes byteCode, int line = 0)
        {
            m_ConstructingOpCode = byteCode;
            m_ConstructingOpCodeData = new List<int>();
            m_ConstructingLine = line;
        }

        private void AddToConstuctingByteCodeInstruction(int opCodeData)
        {
            m_ConstructingOpCodeData.Add(opCodeData);
        }

        private void EndConstuctingByteCodeInstruction()
        {
            ByteCode byCode = new ByteCode(m_ConstructingOpCode);
            byCode.OpCodeData = m_ConstructingOpCodeData.ToArray();
            m_BiteProgram.CurrentChunk.WriteToChunk(byCode, m_ConstructingLine);
            m_ConstructingOpCodeData = null;
            m_ConstructingOpCode = SrslVmOpCodes.OpNone;
        }

        private int EmitByteCode(SrslVmOpCodes byteCode, int line = 0)
        {
            return m_BiteProgram.CurrentChunk.WriteToChunk(byteCode, line);
        }

        private int EmitByteCode(SrslVmOpCodes byteCode, int opCodeData, int line)
        {
            ByteCode byCode = new ByteCode(byteCode, opCodeData);
            return m_BiteProgram.CurrentChunk.WriteToChunk(byCode, line);
        }

        private int EmitByteCode(SrslVmOpCodes byteCode, int opCodeData, int opCodeData2, int line)
        {
            ByteCode byCode = new ByteCode(byteCode, opCodeData, opCodeData2);
            return m_BiteProgram.CurrentChunk.WriteToChunk(byCode, line);
        }

        private int EmitByteCode(ByteCode byteCode, int line = 0)
        {
            return m_BiteProgram.CurrentChunk.WriteToChunk(byteCode, line);
        }

        private int EmitByteCode(SrslVmOpCodes byteCode, ConstantValue constantValue, int line = 0)
        {
            return m_BiteProgram.CurrentChunk.WriteToChunk(byteCode, constantValue, line);
        }

        private int EmitReturn(int line = 0)
        {
            return EmitByteCode(SrslVmOpCodes.OpReturn, line);
        }

        private int EmitConstant(ConstantValue value, int line = 0)
        {
            return EmitByteCode(SrslVmOpCodes.OpConstant, value, line);
        }

        private object Compile(HeteroAstNode astNode)
        {
            return astNode.Accept(this);
        }

        #region Public

        public CodeGenerator()
        {
            //m_CompilationContext = new CompilationContext();
        }


        public BiteProgram CompileProgram(ProgramNode programNode)
        {
            m_BiteProgram = new BiteProgram(programNode);

            programNode.Accept(this);
            
            m_BiteProgram.Build();

            return m_BiteProgram;
        }


        public BiteProgram CompileStatements(ModuleNode module, List<StatementNode> statements)
        {
            module.Statements = statements;

            m_BiteProgram = new BiteProgram(module);

            module.Accept(this);

            m_BiteProgram.Build();

            return m_BiteProgram;
        }
        public BiteProgram CompileStatements(IReadOnlyCollection<StatementNode> statements)
        {
            var module = new ModuleNode()
            {
                ModuleIdent = new ModuleIdentifier("MainModule"),
                //ImportedModules = new List<ModuleIdentifier>() { new ModuleIdentifier("System", null) },
                //UsedModules = new List<ModuleIdentifier>() { new ModuleIdentifier("System", null) },
                Statements = statements
            };

            m_BiteProgram = new BiteProgram(module);

            module.Accept(this);

            m_BiteProgram.Build();

            return m_BiteProgram;
        }

        public BiteProgram CompileExpression(ExpressionNode expression)
        {
            var module = new ModuleNode()
            {
                ModuleIdent = new ModuleIdentifier("MainModule"),
                //ImportedModules = new List<ModuleIdentifier>() { new ModuleIdentifier("System", null) },
                //UsedModules = new List<ModuleIdentifier>() { new ModuleIdentifier("System", null) },
                Statements = new List<StatementNode>()
                {
                    new ExpressionStatementNode()
                    {
                        Expression = expression
                    }
                }
            };

            m_BiteProgram = new BiteProgram(module);

            module.Accept(this);

            //foreach (var statement in statements)
            //{
            //    statement.Accept(this);
            //}

            m_BiteProgram.Build();

            return m_BiteProgram;
        }



        public override object Visit(ProgramNode node)
        {
            //Chunk saveChunk1 = m_CompilationContext.m_CompilingChunk;
            m_BiteProgram.PushChunk();

            /*int d = 0;
            ModuleSymbol m = node.AstScopeNode.resolve( "System", out int moduleId, ref d ) as ModuleSymbol;
            EmitByteCode( SrslVmOpCodes.OpDefineModule,  new ConstantValue("System"));
            //EmitByteCode( mod.InsertionOrderNumber);
            m_CompilingChunk = new Chunk();
            CompilingChunks.Add( "System", m_CompilingChunk);
            Chunk saveChunk2 = m_CompilingChunk;

            int d2 = 0;
            ClassSymbol c = m.resolve( "Object", out int moduleId2, ref d2 ) as ClassSymbol;
            EmitByteCode( SrslVmOpCodes.OpDefineClass, new ConstantValue("System.Object") );
            //EmitByteCode( symbol.InsertionOrderNumber );
            m_CompilingChunk = new Chunk();
            CompilingChunks.Add( "System.Object", m_CompilingChunk);
            m_CompilingChunk = saveChunk2;

            int d4 = 0;
            MethodSymbol method = m.resolve( "Print", out int moduleId4, ref d4 ) as MethodSymbol;
            EmitByteCode( SrslVmOpCodes.OpDefineMethod, new ConstantValue("System.Print") );
            //EmitByteCode( symbol.InsertionOrderNumber );
            m_CompilingChunk = new Chunk();
            CompilingChunks.Add( "System.Print", m_CompilingChunk);
            m_CompilingChunk = saveChunk2;

            int d5 = 0;
            MethodSymbol method2 = m.resolve( "CSharpInterfaceCall", out int moduleId5, ref d5 ) as MethodSymbol;
            EmitByteCode( SrslVmOpCodes.OpDefineMethod, new ConstantValue("System.CSharpInterfaceCall") );
            //EmitByteCode( symbol.InsertionOrderNumber );
            m_CompilingChunk = new Chunk();
            CompilingChunks.Add( "System.CSharpInterfaceCall", m_CompilingChunk);
            m_CompilingChunk = saveChunk2;

            int d3 = 0;
            ClassSymbol c2 = m.resolve( "CSharpInterface", out int moduleId3, ref d3 ) as ClassSymbol;
            EmitByteCode( SrslVmOpCodes.OpDefineClass, new ConstantValue("System.CSharpInterface") );*/
            //EmitByteCode( symbol.InsertionOrderNumber );
            int d = 0;
            ModuleSymbol m = node.AstScopeNode.resolve("System", out int moduleId, ref d) as ModuleSymbol;

            int d2 = 0;
            ClassSymbol c = m.resolve("Object", out int moduleId2, ref d2) as ClassSymbol;

            //m_CompilingChunk = new Chunk();

            m_BiteProgram.NewChunk();

            ByteCode byteCode = new ByteCode(SrslVmOpCodes.OpDefineLocalInstance, moduleId2, d2, c.InsertionOrderNumber);
            EmitByteCode(byteCode);
            EmitByteCode(byteCode);
            EmitByteCode(byteCode);
            EmitByteCode(byteCode);

            //m_CompilationContext.CompilingChunks.Add("System.CSharpInterface", m_CompilationContext.CurrentChunk);
            m_BiteProgram.SaveCurrentChunk("System.CSharpInterface");
            //m_CompilationContext.CompilingChunks.Add("System.CSharpInterface", m_CompilingChunk);

            //m_CompilationContext.RestoreChunk(saveChunk1);
            m_BiteProgram.PopChunk();


            foreach (KeyValuePair<string, ModuleNode> module in node.ModuleNodes)
            {
                //ModuleSymbol moduleSymbol = m_SymbolTableBuilder.CurrentScope.resolve( module.Key ) as ModuleSymbol;

                if (module.Key != node.MainModule)
                {
                    Compile(module.Value);
                }
            }

            if (node.ModuleNodes.ContainsKey(node.MainModule))
            {
                Compile(node.ModuleNodes[node.MainModule]);
            }
            else
            {
                throw new Exception("Main Module: " + node.MainModule + " not found!");
            }

            return null;
        }

        public override object Visit(ModuleNode node)
        {
            m_CurrentModuleName = node.ModuleIdent.ToString();
            //Chunk saveChunk = m_CompilingChunk;
            m_BiteProgram.PushChunk();

            int d = 0;
            ModuleSymbol mod = m_BiteProgram.SymbolTableBuilder.CurrentScope.resolve(m_CurrentModuleName, out int moduleId, ref d) as ModuleSymbol;
            if (m_BiteProgram.HasChunk(m_CurrentModuleName))
            {
                //m_CompilingChunk = CompilingChunks[m_CurrentModuleName];
                m_BiteProgram.RestoreChunk(m_CurrentModuleName);
            }
            else
            {
                m_BiteProgram.CurrentChunk.
                    WriteToChunk(
                        SrslVmOpCodes.OpDefineModule,
                        new ConstantValue(m_CurrentModuleName),
                        mod.NumberOfSymbols,
                        0);
                //m_CompilingChunk = new Chunk();
                m_BiteProgram.NewChunk();
                //CompilingChunks.Add(m_CurrentModuleName, m_CompilingChunk);
                m_BiteProgram.SaveCurrentChunk(m_CurrentModuleName);
            }

            /*foreach ( ModuleIdentifier importedModule in node.ImportedModules )
            {
                EmitByteCode( SrslVmOpCodes.OpImportModule, importedModule.ToString(), importedModule.DebugInfoAstNode.LineNumber );
            }*/

            foreach (StatementNode statement in node.Statements)
            {
                switch (statement)
                {
                    case ClassDeclarationNode classDeclarationNode:
                        Compile(classDeclarationNode);
                        break;
                    case StructDeclarationNode structDeclaration:
                        Compile(structDeclaration);
                        break;
                    case FunctionDeclarationNode functionDeclarationNode:
                        Compile(functionDeclarationNode);
                        break;
                    case VariableDeclarationNode variable:
                        Compile(variable);
                        break;
                    case ClassInstanceDeclarationNode classInstance:
                        Compile(classInstance);
                        break;
                    case StatementNode stat:
                        Compile(stat);
                        break;
                }
            }

            m_BiteProgram.PopChunk(); //m_CompilingChunk = saveChunk;

            return null;
        }

        public override object Visit(ModifiersNode node)
        {
            return null;
        }

        public override object Visit(DeclarationNode node)
        {
            return null;
        }

        public override object Visit(UsingStatementNode node)
        {
            EmitByteCode(SrslVmOpCodes.OpEnterBlock, (node.AstScopeNode as BaseScope).NestedSymbolCount, 0);
            Compile(node.UsingNode);
            EmitByteCode(SrslVmOpCodes.OpUsingStatmentHead);
            Compile(node.UsingBlock);
            EmitByteCode(SrslVmOpCodes.OpUsingStatmentEnd);
            EmitByteCode(SrslVmOpCodes.OpExitBlock);
            return null;
        }

        public override object Visit(DeclarationsNode node)
        {
            EmitByteCode(SrslVmOpCodes.OpEnterBlock, (node.AstScopeNode as BaseScope).NestedSymbolCount, 0);

            m_CurrentEnterBlockCount++;

            if (node.Classes != null)
            {
                foreach (ClassDeclarationNode declaration in node.Classes)
                {
                    Compile(declaration);
                }
            }

            if (node.Structs != null)
            {
                foreach (StructDeclarationNode declaration in node.Structs)
                {
                    Compile(declaration);
                }
            }

            if (node.Functions != null)
            {
                foreach (FunctionDeclarationNode declaration in node.Functions)
                {
                    Compile(declaration);
                }
            }

            if (node.ClassInstances != null)
            {
                foreach (ClassInstanceDeclarationNode declaration in node.ClassInstances)
                {
                    Compile(declaration);
                }
            }

            if (node.Variables != null)
            {
                foreach (VariableDeclarationNode declaration in node.Variables)
                {
                    Compile(declaration);
                }
            }

            if (node.Statements != null)
            {
                foreach (StatementNode declaration in node.Statements)
                {
                    Compile(declaration);
                }
            }

            EmitByteCode(SrslVmOpCodes.OpExitBlock);
            m_CurrentEnterBlockCount--;
            
            return null;
        }

        public override object Visit(ClassDeclarationNode node)
        {
            int d = 0;
            ClassSymbol symbol = (ClassSymbol)node.AstScopeNode.resolve(node.ClassId.Id, out int moduleId, ref d);
            m_CurrentClassName = symbol.QualifiedName;
            //Chunk saveChunk = m_CompilingChunk;

            m_BiteProgram.PushChunk();

            if (m_BiteProgram.HasChunk(m_CurrentClassName))
            {
                //m_CompilingChunk = CompilingChunks[m_CurrentClassName];
                m_BiteProgram.RestoreChunk(m_CurrentClassName);
            }
            else
            {
                EmitByteCode(SrslVmOpCodes.OpDefineClass, new ConstantValue(m_CurrentClassName));
                //EmitByteCode( symbol.InsertionOrderNumber );
                m_BiteProgram.NewChunk(); //m_CompilingChunk = new Chunk();
                                                 // CompilingChunks.Add(m_CurrentClassName, m_CompilingChunk);
                m_BiteProgram.SaveCurrentChunk(m_CurrentClassName);
            }

            foreach (FieldSymbol field in symbol.Fields)
            {
                if (field.DefinitionNode != null && field.DefinitionNode is VariableDeclarationNode variableDeclarationNode)
                {
                    Compile(variableDeclarationNode);
                }
                else if (field.DefinitionNode != null && field.DefinitionNode is ClassInstanceDeclarationNode classInstance)
                {
                    Compile(classInstance);
                }
                else
                {

                    EmitByteCode(SrslVmOpCodes.OpDefineLocalVar);

                    //EmitByteCode( field.Type );
                }
            }

            foreach (MethodSymbol method in symbol.DefinedMethods)
            {
                if (method.DefNode != null)
                {
                    Compile(method.DefNode);
                }
                else
                {
                    EmitByteCode(SrslVmOpCodes.OpDefineMethod, new ConstantValue(method.QualifiedName));
                    //EmitByteCode( method.InsertionOrderNumber );
                }
            }
            m_BiteProgram.PopChunk(); //m_CompilingChunk = saveChunk;
            return null;
        }

        public override object Visit(FunctionDeclarationNode node)
        {
            m_BiteProgram.PushChunk(); //Chunk saveChunk = m_CompilingChunk;
            int d = 0;
            FunctionSymbol symbol = node.AstScopeNode.resolve(node.FunctionId.Id, out int moduleId, ref d) as FunctionSymbol;
            if (m_BiteProgram.HasChunk(symbol.QualifiedName))
            {
                //m_CompilingChunk = CompilingChunks[symbol.QualifiedName];
                m_BiteProgram.RestoreChunk(symbol.QualifiedName);
            }
            else
            {
                EmitByteCode(SrslVmOpCodes.OpDefineMethod, new ConstantValue(symbol.QualifiedName));
                //EmitByteCode( symbol.InsertionOrderNumber );
                //m_CompilingChunk = new Chunk();
                m_BiteProgram.NewChunk();
                //CompilingChunks.Add(symbol.QualifiedName, m_CompilingChunk);
                m_BiteProgram.SaveCurrentChunk(symbol.QualifiedName);
            }
            /*if ( node.Parameters != null )
            {
                foreach ( Identifier parametersIdentifier in node.Parameters.Identifiers )
                {
                    EmitByteCode( SrslVmOpCodes.OpDefineLocalVar, parametersIdentifier.Id );
                }
            }*/

            if (node.FunctionBlock != null)
            {
                Compile(node.FunctionBlock);
            }

            m_BiteProgram.PopChunk(); //m_CompilingChunk = saveChunk;
            return null;
        }

        public override object Visit(VariableDeclarationNode node)
        {
            int d = 0;
            DynamicVariable variableSymbol = node.AstScopeNode.resolve(node.VarId.Id, out int moduleId, ref d) as DynamicVariable;
            
            if (node.Initializer != null)
            {
                Compile(node.Initializer);
                EmitByteCode(SrslVmOpCodes.OpDefineLocalVar);

            }
            else
            {
                EmitByteCode(SrslVmOpCodes.OpDeclareLocalVar);
            }
            return null;
        }

        public override object Visit(ClassInstanceDeclarationNode node)
        {
            int d = 0;
            DynamicVariable variableSymbol = node.AstScopeNode.resolve(node.InstanceId.Id, out int moduleId, ref d) as DynamicVariable;
            int d2 = 0;
            ClassSymbol classSymbol = node.AstScopeNode.resolve(node.ClassName.Id, out int moduleId2, ref d2) as ClassSymbol;
            if (node.IsVariableRedeclaration)
            {
                ByteCode byteCode = new ByteCode(
                    SrslVmOpCodes.OpSetLocalInstance,
                    moduleId,
                    d,
                    variableSymbol.InsertionOrderNumber,
                    moduleId2,
                    d2,
                    classSymbol.InsertionOrderNumber,
                classSymbol.NumberOfSymbols);

                EmitByteCode(byteCode);
            }
            else
            {
                ByteCode byteCode = new ByteCode(
                    SrslVmOpCodes.OpDefineLocalInstance,
                    moduleId2,
                    d2,
                    classSymbol.InsertionOrderNumber,
                    classSymbol.NumberOfSymbols);

                EmitByteCode(byteCode);
            }

            /*foreach ( var methodSymbol in classSymbol.Methods )
            {
                if ( methodSymbol.IsConstructor )
                {
                    ByteCode byteCode = new ByteCode(
                        SrslVmOpCodes.OpGetLocalInstance,
                        moduleId,
                        d,
                        variableSymbol.InsertionOrderNumber);
                    
                    EmitByteCode(byteCode);
                    EmitByteCode( SrslVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.QualifiedName ) );
                }
            }*/
            return null;
        }

        public override object Visit(CallNode node)
        {
            if (node.Arguments != null && node.Arguments.Expressions != null)
            {
                foreach (ExpressionNode argumentsExpression in node.Arguments.Expressions)
                {
                    Compile(argumentsExpression);
                }
                ByteCode byteCode = new ByteCode(
                    SrslVmOpCodes.OpBindToFunction,
                    node.Arguments.Expressions.Count);

                EmitByteCode(byteCode);
            }
            
            if (node.IsFunctionCall)
            {
                //EmitByteCode( SrslVmOpCodes.OpCallFunction );
                int d = 0;
                FunctionSymbol functionSymbol = node.AstScopeNode.resolve(node.Primary.PrimaryId.Id, out int moduleId, ref d) as FunctionSymbol;
                EmitByteCode(SrslVmOpCodes.OpCallFunction, new ConstantValue(functionSymbol.QualifiedName));
            }
            else
            {
                if (node.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier)
                {
                    if (m_IsCompilingAssignmentLhs && (node.CallEntries == null || node.CallEntries.Count == 0))
                    {
                        BeginConstuctingByteCodeInstruction(SrslVmOpCodes.OpSetLocalVar);
                        Compile(node.Primary);
                    }
                    else
                    {
                        int d = 0;
                        Symbol var = node.AstScopeNode.resolve(node.Primary.PrimaryId.Id, out int moduleId, ref d);
                        if (var is ModuleSymbol m)
                        {
                            BeginConstuctingByteCodeInstruction(SrslVmOpCodes.OpGetModule);
                            AddToConstuctingByteCodeInstruction(m.InsertionOrderNumber);
                            EndConstuctingByteCodeInstruction();
                        }
                        else
                        {
                            BeginConstuctingByteCodeInstruction(SrslVmOpCodes.OpGetLocalVar);
                            Compile(node.Primary);
                        }

                    }

                }
                else
                {
                    Compile(node.Primary);
                }

            }
            if (node.ElementAccess != null)
            {
                foreach (CallElementEntry callElementEntry in node.ElementAccess)
                {
                    if (callElementEntry.CallElementType == CallElementTypes.Call)
                    {
                        Compile(callElementEntry.Call);
                    }
                    else
                    {
                        EmitConstant(new ConstantValue(callElementEntry.Identifier));
                    }
                }
                if (m_IsCompilingAssignmentLhs && (node.CallEntries == null || node.CallEntries.Count == 0))
                {
                    ByteCode byteCode = new ByteCode(
                        SrslVmOpCodes.OpSetElement,
                        node.ElementAccess.Count);
                    EmitByteCode(byteCode);
                }
                else
                {
                    ByteCode byteCode = new ByteCode(
                        SrslVmOpCodes.OpGetElement,
                        node.ElementAccess.Count);
                    EmitByteCode(byteCode);
                }
            }

           

            if (node.CallEntries != null)
            {
                int i = 0;
                int d = 0;
                int d2 = 0;
                bool isModuleSymbol = false;
                Symbol var = node.AstScopeNode.resolve(node.Primary.PrimaryId.Id, out int moduleId, ref d);
                ModuleSymbol moduleSymbol = null;
                ClassSymbol classSymbol = null;
                if (var is ModuleSymbol m)
                {
                    moduleSymbol = m;
                    isModuleSymbol = true;
                }
                else
                {
                    classSymbol = node.AstScopeNode.resolve((var as DynamicVariable).Type.Name, out int moduleId2, ref d2) as ClassSymbol;
                }

                //DynamicVariable dynamicVariable = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d) as DynamicVariable;

                foreach (CallEntry terminalNode in node.CallEntries)
                {

                    if (terminalNode.Arguments != null && terminalNode.Arguments.Expressions != null)
                    {
                        foreach (ExpressionNode argumentsExpression in terminalNode.Arguments.Expressions)
                        {
                            Compile(argumentsExpression);
                        }
                        ByteCode byteCode = new ByteCode(
                            SrslVmOpCodes.OpBindToFunction,
                            terminalNode.Arguments.Expressions.Count);

                        EmitByteCode(byteCode);
                    }

                    if (terminalNode.ElementAccess != null)
                    {
                        foreach (CallElementEntry callElementEntry in terminalNode.ElementAccess)
                        {
                            if (callElementEntry.CallElementType == CallElementTypes.Call)
                            {
                                Compile(callElementEntry.Call);
                            }
                            else
                            {
                                EmitConstant(new ConstantValue(callElementEntry.Identifier));
                            }
                        }

                        if ( m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1 )
                        {
                            ByteCode byteCode = new ByteCode(
                                SrslVmOpCodes.OpSetElement,
                                terminalNode.ElementAccess.Count);
                            
                            EmitByteCode(byteCode);
                        }
                        else
                        {
                            ByteCode byteCode = new ByteCode(
                                SrslVmOpCodes.OpGetElement,
                                terminalNode.ElementAccess.Count);
                            
                            EmitByteCode(byteCode);
                        }
                      

                       
                    }

                    if (terminalNode.IsFunctionCall)
                    {
                        if (isModuleSymbol)
                        {
                            int d4 = 0;
                            FunctionSymbol memberSymbol = moduleSymbol.resolve(
                                terminalNode.Primary.PrimaryId.Id,
                                out int moduleId4,
                                ref d4) as FunctionSymbol;

                            if (memberSymbol == null)
                            {
                                EmitByteCode(SrslVmOpCodes.OpCallMemberFunction, new ConstantValue(terminalNode.Primary.PrimaryId.Id));
                            }
                            else
                            {
                                EmitByteCode(SrslVmOpCodes.OpCallMemberFunction, new ConstantValue(memberSymbol.QualifiedName));
                            }

                        }
                        else
                        {
                            int d4 = 0;
                            FunctionSymbol memberSymbol = classSymbol.resolve(
                                terminalNode.Primary.PrimaryId.Id,
                                out int moduleId4,
                                ref d4) as FunctionSymbol;

                            if (memberSymbol == null)
                            {
                                EmitByteCode(SrslVmOpCodes.OpCallMemberFunction, new ConstantValue(terminalNode.Primary.PrimaryId.Id));
                            }
                            else
                            {
                                EmitByteCode(SrslVmOpCodes.OpCallMemberFunction, new ConstantValue(memberSymbol.QualifiedName));
                            }

                        }

                    }
                    else
                    {
                        if (terminalNode.Primary.PrimaryType == PrimaryNode.PrimaryTypes.Identifier)
                        {
                            if (m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1)
                            {
                                if (isModuleSymbol)
                                {
                                    int d4 = 0;

                                    Symbol memberSymbol = moduleSymbol.resolve(
                                        terminalNode.Primary.PrimaryId.Id,
                                        out int moduleId4,
                                        ref d4);

                                    ByteCode byteCode = new ByteCode(
                                        SrslVmOpCodes.OpSetMember,
                                        memberSymbol.InsertionOrderNumber);
                                    EmitByteCode(byteCode);

                                }
                                else
                                {
                                    int d4 = 0;

                                    Symbol memberSymbol = classSymbol.resolve(
                                        terminalNode.Primary.PrimaryId.Id,
                                        out int moduleId4,
                                        ref d4);

                                    ByteCode byteCode = new ByteCode(
                                        SrslVmOpCodes.OpSetMember,
                                        memberSymbol.InsertionOrderNumber);
                                    EmitByteCode(byteCode);
                                }
                            }
                            else
                            {
                                if (isModuleSymbol)
                                {
                                    int d4 = 0;

                                    Symbol memberSymbol = moduleSymbol.resolve(
                                        terminalNode.Primary.PrimaryId.Id,
                                        out int moduleId4,
                                        ref d4);

                                    ByteCode byteCode = new ByteCode(
                                        SrslVmOpCodes.OpGetMember,
                                        memberSymbol.InsertionOrderNumber);
                                    EmitByteCode(byteCode);

                                }
                                else
                                {
                                    int d4 = 0;

                                    Symbol memberSymbol = classSymbol.resolve(
                                        terminalNode.Primary.PrimaryId.Id,
                                        out int moduleId4,
                                        ref d4);

                                    ByteCode byteCode = new ByteCode(
                                        SrslVmOpCodes.OpGetMember,
                                        memberSymbol.InsertionOrderNumber);
                                    EmitByteCode(byteCode);
                                }

                            }
                        }
                    }
                    i++;
                }
            }

            return null;
        }
        public override object Visit(ArgumentsNode node)
        {

            return null;
        }

        public override object Visit(ParametersNode node)
        {

            return null;
        }

        public override object Visit(AssignmentNode node)
        {
            switch (node.Type)
            {
                case AssignmentTypes.Assignment:
                    Compile(node.Assignment);
                    m_IsCompilingAssignmentLhs = true;
                    Compile(node.Call);
                    m_IsCompilingAssignmentLhs = false;
                    switch (node.OperatorType)
                    {
                        case AssignmentOperatorTypes.Assign:
                            EmitByteCode(SrslVmOpCodes.OpAssign);
                            break;

                        case AssignmentOperatorTypes.DivAssign:
                            EmitByteCode(SrslVmOpCodes.OpDivideAssign);
                            break;

                        case AssignmentOperatorTypes.MultAssign:
                            EmitByteCode(SrslVmOpCodes.OpMultiplyAssign);
                            break;

                        case AssignmentOperatorTypes.PlusAssign:
                            EmitByteCode(SrslVmOpCodes.OpPlusAssign);
                            break;

                        case AssignmentOperatorTypes.MinusAssign:
                            EmitByteCode(SrslVmOpCodes.OpMinusAssign);
                            break;

                        case AssignmentOperatorTypes.ModuloAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpModuloAssign);
                            break;

                        case AssignmentOperatorTypes.BitwiseAndAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpBitwiseAndAssign);
                            break;

                        case AssignmentOperatorTypes.BitwiseOrAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpBitwiseOrAssign);
                            break;

                        case AssignmentOperatorTypes.BitwiseXorAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpBitwiseXorAssign);
                            break;

                        case AssignmentOperatorTypes.BitwiseLeftShiftAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpBitwiseLeftShiftAssign);
                            break;

                        case AssignmentOperatorTypes.BitwiseRightShiftAssignOperator:
                            EmitByteCode(SrslVmOpCodes.OpBitwiseRightShiftAssign);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                    break;

                case AssignmentTypes.Binary:
                    Compile(node.Binary);

                    break;

                case AssignmentTypes.Ternary:
                    Compile(node.Ternary);

                    break;

                case AssignmentTypes.Call:
                    Compile(node.Call);

                    break;

                case AssignmentTypes.Primary:
                    Compile(node.PrimaryNode);

                    break;

                case AssignmentTypes.UnaryPostfix:
                    Compile(node.UnaryPostfix);

                    break;

                case AssignmentTypes.UnaryPrefix:
                    Compile(node.UnaryPrefix);

                    break;

                default:
                    throw new Exception("Invalid Type");
            }

            return null;
        }

        public override object Visit(ExpressionNode node)
        {
            Compile(node.Assignment);

            return null;
        }

        public override object Visit(BlockStatementNode node)
        {
            Compile(node.Declarations);
            return null;
        }

        public override object Visit(StatementNode node)
        {
            if (node is ExpressionStatementNode expressionStatementNode)
            {
                Compile(expressionStatementNode);

                return null;
            }

            if (node is ForStatementNode forStatementNode)
            {
                Compile(forStatementNode);

                return null;
            }

            if (node is IfStatementNode ifStatementNode)
            {
                Compile(ifStatementNode);

                return null;
            }

            if (node is WhileStatementNode whileStatement)
            {
                Compile(whileStatement);

                return null;
            }

            if (node is ReturnStatementNode returnStatement)
            {
                Compile(returnStatement);

                return null;
            }

            if (node is BlockStatementNode blockStatementNode)
            {
                Compile(blockStatementNode);

                return null;
            }

            return null;
        }

        public override object Visit(ExpressionStatementNode node)
        {
            Compile(node.Expression);

            return null;
        }

        public override object Visit(IfStatementNode node)
        {
            Compile(node.Expression);
            int thenJump = EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            Compile(node.ThenBlock);
            int overElseJump = EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            m_BiteProgram.CurrentChunk.Code[thenJump] = new ByteCode(SrslVmOpCodes.OpJumpIfFalse, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);

            foreach ( IfStatementEntry nodeIfStatementEntry in node.IfStatementEntries )
            {
                if ( nodeIfStatementEntry.IfStatementType == IfStatementEntryType.Else )
                {
                    Compile( nodeIfStatementEntry.ElseBlock );
                    m_BiteProgram.CurrentChunk.Code[overElseJump] = new ByteCode(SrslVmOpCodes.OpJump, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);
                }
                if ( nodeIfStatementEntry.IfStatementType == IfStatementEntryType.ElseIf )
                {
                    Compile( nodeIfStatementEntry.ExpressionElseIf );
                    int elseJump = EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
                    Compile( nodeIfStatementEntry.ElseBlock );
                    m_BiteProgram.CurrentChunk.Code[elseJump] = new ByteCode(SrslVmOpCodes.OpJumpIfFalse, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);
                    m_BiteProgram.CurrentChunk.Code[overElseJump] = new ByteCode(SrslVmOpCodes.OpJump, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);
                }
            }
            
            return null;
        }

        public override object Visit(ForStatementNode node)
        {
           
            EmitByteCode(SrslVmOpCodes.OpEnterBlock, (node.AstScopeNode as BaseScope).NestedSymbolCount, 0);
            m_CurrentEnterBlockCount++;
            PreviousLoopBlockCount = m_CurrentEnterBlockCount;
            if (node.VariableDeclaration != null)
            {
                Compile(node.VariableDeclaration);
            }
            else if (node.ExpressionStatement != null)
            {
                Compile(node.ExpressionStatement);
            }
            
            int jumpCodeWhileBegin = m_BiteProgram.CurrentChunk.SerializeToBytes().Length;
            if (node.Expression1 != null)
            {
                Compile(node.Expression1);
            }
            
         
            int toFix = EmitByteCode(SrslVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, 0);
            Compile(node.Block);
            if (node.Expression2 != null)
            {
                Compile(node.Expression2);
            }
            m_BiteProgram.CurrentChunk.Code[toFix] = new ByteCode(SrslVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);
            EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            EmitByteCode(SrslVmOpCodes.OpExitBlock, 0);
            m_CurrentEnterBlockCount--;
            return null;
        }

        public override object Visit(WhileStatementNode node)
        {
            PreviousLoopBlockCount = m_CurrentEnterBlockCount;
            
            int jumpCodeWhileBegin = m_BiteProgram.CurrentChunk.SerializeToBytes().Length;
            Compile(node.Expression);
            int toFix = EmitByteCode(SrslVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, 0);
            Compile(node.WhileBlock);
            m_BiteProgram.CurrentChunk.Code[toFix] = new ByteCode(SrslVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, m_BiteProgram.CurrentChunk.SerializeToBytes().Length);
            EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            EmitByteCode(SrslVmOpCodes.OpNone, 0, 0);
            return null;
        }

        public override object Visit(ReturnStatementNode node)
        {
            Compile(node.ExpressionStatement);
            EmitByteCode(SrslVmOpCodes.OpKeepLastItemOnStack);
            for (int i = 0; i < m_CurrentEnterBlockCount; i++)
            {
                EmitByteCode(SrslVmOpCodes.OpExitBlock);
            }
            EmitReturn();

            return null;
        }
        
        public override object Visit(BreakStatementNode node)
        {
            for ( int i = 0; i < (m_CurrentEnterBlockCount - PreviousLoopBlockCount); i++ )
            {
                EmitByteCode(SrslVmOpCodes.OpExitBlock);
            }

            EmitByteCode( SrslVmOpCodes.OpBreak );
            return null;
        }

        public override object Visit(InitializerNode node)
        {
            Compile(node.Expression);

            return null;
        }

        public override object Visit(BinaryOperationNode node)
        {
            Compile(node.LeftOperand);
            Compile(node.RightOperand);
            
            switch (node.Operator)
            {
                case BinaryOperationNode.BinaryOperatorType.Plus:
                    EmitByteCode(SrslVmOpCodes.OpAdd);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Minus:
                    EmitByteCode(SrslVmOpCodes.OpSubtract);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Mult:
                    EmitByteCode(SrslVmOpCodes.OpMultiply);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Div:
                    EmitByteCode(SrslVmOpCodes.OpDivide);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Modulo:
                    EmitByteCode(SrslVmOpCodes.OpModulo);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Equal:
                    EmitByteCode(SrslVmOpCodes.OpEqual);
                    break;

                case BinaryOperationNode.BinaryOperatorType.NotEqual:
                    EmitByteCode(SrslVmOpCodes.OpNotEqual);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Less:
                    EmitByteCode(SrslVmOpCodes.OpSmaller);
                    break;

                case BinaryOperationNode.BinaryOperatorType.LessOrEqual:
                    EmitByteCode(SrslVmOpCodes.OpSmallerEqual);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Greater:
                    EmitByteCode(SrslVmOpCodes.OpGreater);
                    break;

                case BinaryOperationNode.BinaryOperatorType.GreaterOrEqual:
                    EmitByteCode(SrslVmOpCodes.OpGreaterEqual);
                    break;

                case BinaryOperationNode.BinaryOperatorType.And:
                    EmitByteCode(SrslVmOpCodes.OpAnd);
                    break;

                case BinaryOperationNode.BinaryOperatorType.Or:
                    EmitByteCode(SrslVmOpCodes.OpOr);
                    break;
                
                case BinaryOperationNode.BinaryOperatorType.BitwiseOr:
                    EmitByteCode(SrslVmOpCodes.OpBitwiseOr);
                    break;
                
                case BinaryOperationNode.BinaryOperatorType.BitwiseAnd:
                    EmitByteCode(SrslVmOpCodes.OpBitwiseAnd);
                    break;
                
                case BinaryOperationNode.BinaryOperatorType.BitwiseXor:
                    EmitByteCode(SrslVmOpCodes.OpBitwiseXor);
                    break;
                
                case BinaryOperationNode.BinaryOperatorType.ShiftLeft:
                    EmitByteCode(SrslVmOpCodes.OpBitwiseLeftShift);
                    break;

                case BinaryOperationNode.BinaryOperatorType.ShiftRight:
                    EmitByteCode(SrslVmOpCodes.OpBitwiseRightShift);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public override object Visit(TernaryOperationNode node)
        {
            Compile( node.RightOperand );
            Compile( node.MidOperand );
            Compile( node.LeftOperand );
            EmitByteCode( SrslVmOpCodes.OpTernary );
            return null;
        }

        public override object Visit(PrimaryNode node)
        {
            switch (node.PrimaryType)
            {
                case PrimaryNode.PrimaryTypes.Identifier:
                    {
                        int d = 0;
                        Symbol symbol = node.AstScopeNode.resolve(node.PrimaryId.Id, out int moduleId, ref d);
                        AddToConstuctingByteCodeInstruction(moduleId);
                        AddToConstuctingByteCodeInstruction(d);

                        if (symbol == null)
                        {
                            throw new CompilerException($"Failed to resolve symbol '{node.PrimaryId.Id}' in scope '{node.AstScopeNode.Name}'");
                        }

                        if (symbol.SymbolScope is ClassSymbol s)
                        {
                            AddToConstuctingByteCodeInstruction(s.InsertionOrderNumber);
                        }
                        else
                        {
                            AddToConstuctingByteCodeInstruction(-1);
                        }
                        AddToConstuctingByteCodeInstruction(symbol.InsertionOrderNumber);
                        EndConstuctingByteCodeInstruction();
                        return null;
                    }


                case PrimaryNode.PrimaryTypes.ThisReference:
                    int d2 = 0;
                    Symbol thisSymbol = node.AstScopeNode.resolve("this", out int moduleId2, ref d2);
                    AddToConstuctingByteCodeInstruction(moduleId2);
                    AddToConstuctingByteCodeInstruction(d2);
                    AddToConstuctingByteCodeInstruction(thisSymbol.InsertionOrderNumber);
                    EndConstuctingByteCodeInstruction();
                    return null;

                case PrimaryNode.PrimaryTypes.BooleanLiteral:
                    EmitConstant(new ConstantValue(node.BooleanLiteral.Value));
                    return null;

                case PrimaryNode.PrimaryTypes.IntegerLiteral:
                    EmitConstant(new ConstantValue(node.IntegerLiteral.Value));
                    return null;

                case PrimaryNode.PrimaryTypes.FloatLiteral:
                    EmitConstant(new ConstantValue(node.FloatLiteral.Value));
                    return null;

                case PrimaryNode.PrimaryTypes.StringLiteral:
                    EmitConstant(new ConstantValue(node.StringLiteral));
                    return null;

                case PrimaryNode.PrimaryTypes.Expression:
                    node.Expression.Accept(this);
                    return null;

                case PrimaryNode.PrimaryTypes.NullReference:
                    EmitConstant(new ConstantValue(null));
                    return null;

                case PrimaryNode.PrimaryTypes.Default:
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(node.PrimaryType),
                        node.PrimaryType,
                        null);
            }
        }

        public override object Visit(StructDeclarationNode node)
        {
            return null;
        }

        public override object Visit(UnaryPostfixOperation node)
        {
            Compile(node.Primary);
            switch (node.Operator)
            {
                case UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus:
                    EmitByteCode(SrslVmOpCodes.OpPostfixIncrement);
                    break;

                case UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus:
                    EmitByteCode(SrslVmOpCodes.OpPostfixDecrement);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public override object Visit(UnaryPrefixOperation node)
        {
            Compile(node.Primary);
            switch (node.Operator)
            {
                case UnaryPrefixOperation.UnaryPrefixOperatorType.Plus:
                    EmitByteCode(SrslVmOpCodes.OpAffirm);
                    break;

                case UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment:
                    EmitByteCode(SrslVmOpCodes.OpCompliment);
                    break;

                case UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus:
                    EmitByteCode(SrslVmOpCodes.OpPrefixIncrement);
                    break;

                case UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus:
                    EmitByteCode(SrslVmOpCodes.OpPrefixDecrement);
                    break;

                case UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot:
                    EmitByteCode(SrslVmOpCodes.OpNot);
                    break;

                case UnaryPrefixOperation.UnaryPrefixOperatorType.Negate:
                    EmitByteCode(SrslVmOpCodes.OpNegate);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }

        public override object Visit(HeteroAstNode node)
        {
            switch (node)
            {
                case ProgramNode program:
                    return Visit(program);

                case ModuleNode module:
                    return Visit(module);

                case ClassDeclarationNode classDeclarationNode:
                    return Visit(classDeclarationNode);

                case StructDeclarationNode structDeclaration:
                    return Visit(structDeclaration);

                case FunctionDeclarationNode functionDeclarationNode:
                    return Visit(functionDeclarationNode);

                case VariableDeclarationNode variable:
                    return Visit(variable);

                case ClassInstanceDeclarationNode classInstance:
                    return Visit(classInstance);

                case UsingStatementNode usingStatementNode:
                    return Visit(usingStatementNode);

                case ExpressionStatementNode expressionStatementNode:
                    return Visit(expressionStatementNode);

                case ForStatementNode forStatementNode:
                    return Visit(forStatementNode);

                case WhileStatementNode whileStatement:
                    return Visit(whileStatement);

                case IfStatementNode ifStatementNode:
                    return Visit(ifStatementNode);

                case ReturnStatementNode returnStatement:
                    return Visit(returnStatement);
                
                case BreakStatementNode returnStatement:
                    return Visit(returnStatement);

                case BlockStatementNode blockStatementNode:
                    return Visit(blockStatementNode);

                case AssignmentNode assignmentNode:
                    return Visit(assignmentNode);

                case CallNode callNode:
                    return Visit(callNode);

                case BinaryOperationNode binaryOperation:
                    return Visit(binaryOperation);

                case TernaryOperationNode ternaryOperationNode:
                    return Visit(ternaryOperationNode);

                case PrimaryNode primaryNode:
                    return Visit(primaryNode);

                case DeclarationsNode declarationsNode:
                    return Visit(declarationsNode);

                case UnaryPostfixOperation postfixOperation:
                    return Visit(postfixOperation);

                case UnaryPrefixOperation prefixOperation:
                    return Visit(prefixOperation);

                case StatementNode stat:
                    return Visit(stat);

                case ExpressionNode expression:
                    return Visit(expression);

                case InitializerNode initializerNode:
                    return Visit(initializerNode.Expression);

                default:
                    return null;
            }
        }

        #endregion

    }

}
