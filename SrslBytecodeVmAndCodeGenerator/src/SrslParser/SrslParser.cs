using System;
using System.Collections.Generic;
using System.IO;
using MemoizeSharp;

namespace Srsl_Parser
{

public class SrslParser
{
    public ProgramNode Program;
    public virtual void ParseSrslProgram(string mainModule, string pathToFolderWithSrslProgram)
    {
        Program = new ProgramNode();
        Program.ModuleNodes = new Dictionary < string, ModuleNode >();
        Program.MainModule = mainModule;
        
        foreach (string file in Directory.EnumerateFiles(pathToFolderWithSrslProgram, "*.srsl", SearchOption.AllDirectories))
        {
            string readText = File.ReadAllText( file );
            SrslLexer lexer = new SrslLexer( readText );
            
            SrslModuleParser moduleParser = new SrslModuleParser( lexer );

            ModuleNode module = moduleParser.module();

            if ( !Program.ModuleNodes.ContainsKey( module.ModuleIdent.ToString() ) )
            {
                Program.ModuleNodes.Add( module.ModuleIdent.ToString(), module );
            }
            else
            {
                Program.ModuleNodes[module.ModuleIdent.ToString()].AddStatements( module.Statements );
            }
        }

    }
    
    public virtual List < StatementNode > ParseSrslString(string srslStatments)
    {
        SrslLexer lexer = new SrslLexer( srslStatments );
        SrslModuleParser statmentParser = new SrslModuleParser( lexer );
        List < StatementNode > statementNodes = statmentParser.statements();

        return statementNodes;
    }
}

}
