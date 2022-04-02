using System;
using System.Collections.Generic;

namespace Bite.Runtime.Bytecode
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

            BiteVmOpCodes instruction = (BiteVmOpCodes)chunk.Code[offset];
            offset++;
            switch (instruction)
            {
                
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
