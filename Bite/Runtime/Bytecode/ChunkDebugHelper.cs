using System;
using System.Collections.Generic;

namespace Srsl.Runtime.Bytecode
{

    public class InstructionAnalysis
    {
        public static Dictionary<string, long> InstructionCounter = new Dictionary<string, long>();

    }
    public static class ChunkDebugHelper
    {
        public static Dictionary<string, long> InstructionCounter = new Dictionary<string, long>();

        public static Dictionary<string, InstructionAnalysis> InstructionAnalysis = new Dictionary<string, InstructionAnalysis>();

        public static void DissassembleChunk(this Chunk chunk, string name)
        {
            Console.WriteLine($"=== {name} ===");

            /*for ( int offset = 0; offset < chunk.Code.Count;)
            {
                offset = DissassembleInstruction( chunk, offset );
            }*/
        }

        public static int DissassembleInstruction(this BinaryChunk chunk, int offset)
        {
            Console.Write(offset + " ");

            SrslVmOpCodes instruction = (SrslVmOpCodes)chunk.Code[offset];
            offset++;
            switch (instruction)
            {
                /*  case SrslVmOpCodes.OpConstant:
                      return ConstantInstruction( "OpConstant", chunk, offset );

                  case SrslVmOpCodes.OpNegate:
                      return SimpleInstruction( "OpNegate", offset );

                  case SrslVmOpCodes.OpAdd:
                      return SimpleInstruction( "OpAdd", offset );

                  case SrslVmOpCodes.OpSubtract:
                      return SimpleInstruction( "OpSubtract", offset );

                  case SrslVmOpCodes.OpMultiply:
                      return SimpleInstruction( "OpMultiply", offset );

                  case SrslVmOpCodes.OpDivide:
                      return SimpleInstruction( "OpDivide", offset );

                 case SrslVmOpCodes.OpDefineLocalVar:
                     return SimpleInstruction( "OpDefineLocalVar", offset );

                  case SrslVmOpCodes.OpDefineModule:
                  {
                      string moduleName = ReadConstant(chunk, ref offset).StringConstantValue;
                      int numberOfMembers = chunk.Code[offset] | (chunk.Code[offset+1] << 8) | (chunk.Code[offset+2] << 16) | (chunk.Code[offset+3] << 24);offset += 4;
                      Console.WriteLine("OpDefineModule, '{0}' Number of Symbols'{1}'", moduleName, numberOfMembers);
                      return offset;
                  }

                  case SrslVmOpCodes.OpDefineClass:
                  {
                      string moduleName = ReadConstant(chunk, ref offset).StringConstantValue;
                      Console.WriteLine("OpDefineClass, '{0}'", moduleName);
                      return offset;
                  }

                  case SrslVmOpCodes.OpDefineMethod:
                  {
                      string moduleName = ReadConstant(chunk, ref offset).StringConstantValue;
                      Console.WriteLine("OpDefineMethod, '{0}'", moduleName);
                      return offset;
                  }

                 case SrslVmOpCodes.OpBindToFunction:
                  {
                      int numberOfArgs = chunk.Code[offset] | (chunk.Code[offset+1] << 8) | (chunk.Code[offset+2] << 16) | (chunk.Code[offset+3] << 24);offset += 4;
                      Console.WriteLine("OpBindToFunction, '{0}'", numberOfArgs);
                      return offset;
                  }
                 case SrslVmOpCodes.OpCallFunction:
                 {
                     string moduleName = ReadConstant(chunk, ref offset).StringConstantValue;
                     Console.WriteLine("OpCallFunction, '{0}'", moduleName);
                     return offset;
                 }

                  case SrslVmOpCodes.OpCallMemberFunction:
                  {
                      string moduleName = ReadConstant(chunk, ref offset).StringConstantValue;
                      Console.WriteLine("OpCallFunction, '{0}'", moduleName);
                      return offset;
                  }

                 case SrslVmOpCodes.OpReturn:
                 {
                     //object value = srslStack.Peek();
                     Console.WriteLine("OpReturn");
                     return offset + 1;
                 }


                  case SrslVmOpCodes.OpEnterBlock:
                  {
                      int numberOfMembers = chunk.Code[offset] | (chunk.Code[offset+1] << 8) | (chunk.Code[offset+2] << 16) | (chunk.Code[offset+3] << 24);offset += 4;

                      Console.WriteLine("OpEnterBlock {0}", numberOfMembers);
                      return offset + 1;
                  }

                  case SrslVmOpCodes.OpExitBlock:
                  {
                      Console.WriteLine("OpExitBlock");
                      return offset + 1;
                  }
                  */
                default:
                    string inst = instruction.ToString();

                    if (InstructionCounter.ContainsKey(inst))
                    {
                        InstructionCounter[inst]++;
                    }
                    else
                    {
                        InstructionCounter.Add(inst, 1);
                        //InstructionAnalysis.Add( inst, new InstructionAnalysis() );
                    }


                    Console.WriteLine(inst);
                    return offset + 1;
            }
        }

        /*private static ConstantValue ReadConstant(this BinaryChunk chunk, ref int offset )
        {
            ConstantValue instruction =
                chunk.Constants[chunk.Code[offset] | (chunk.Code[offset+1] << 8) | (chunk.Code[offset+2] << 16) | (chunk.Code[offset+3] << 24)];
            offset += 4;
            return instruction;
        }*/

        static int SimpleInstruction(string name, int offset)
        {
            Console.WriteLine(name);
            return offset + 1;
        }

        /*static int ConstantInstruction(string name, BinaryChunk chunk, int offset)
        {
            ConstantValue constantValue = ReadConstant(chunk, ref offset);

            Console.WriteLine("{0}, \"{1}\"", name, constantValue);
            return offset + 2;
        }*/
    }

}
