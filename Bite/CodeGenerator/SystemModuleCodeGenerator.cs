using Bite.Ast;
using Bite.Runtime.Bytecode;
using Bite.Symbols;

namespace Bite.Runtime.CodeGen
{



public class SystemModuleCodeGenerator
{
    private string decl = @"
module System;

extern class Object() {
}

class CSharpInterface() {
    var Type;
    var Method;
    var Arguments;
    var ConstructorArguments;
    var ConstructorArgumentsTypes;
    var ObjectInstance';
}

extern callable function CSharpInterfaceCall ( object ) {
}

extern callable function Print ( object ) {
}

extern callable function PrintLine ( object ) {
}
";

    public static Chunk GenerateFrom( ProgramBaseNode node )
    {
        int d = 0;
        ModuleSymbol moduleSymbol = node.AstScopeNode.resolve( "System", out int moduleId, ref d ) as ModuleSymbol;

        int d2 = 0;
        ClassSymbol classSymbol = moduleSymbol.resolve( "Object", out int moduleId2, ref d2 ) as ClassSymbol;

        ByteCode byteCode = new ByteCode(
            BiteVmOpCodes.OpDefineInstance,
            moduleId2,
            d2,
            classSymbol.InsertionOrderNumber,
            5 );

        Chunk chunk = new Chunk();

        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "Type" ), 0 );
        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "Method" ), 0 );
        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "Arguments" ), 0 );
        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "ConstructorArguments" ), 0 );
        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "ConstructorArgumentsTypes" ), 0 );
        chunk.WriteToChunk( byteCode, 0 );
        chunk.WriteToChunk( BiteVmOpCodes.OpNone, new ConstantValue( "ObjectInstance" ), 0 );

        return chunk;
    }
}

}