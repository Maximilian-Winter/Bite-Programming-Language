using System;
using System.Collections.Generic;

namespace Bite.Runtime.Bytecode
{

    public class ByteCode
    {
        public ByteCode(BiteVmOpCodes opCode)
        {
            OpCode = opCode;
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2 };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3 };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4 };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5 };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5, int opCodeData6)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6 };
        }

        public ByteCode(BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5, int opCodeData6, int opCodeData7)
        {
            OpCode = opCode;
            OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6, opCodeData7 };
        }

        public BiteVmOpCodes OpCode;
        public int[] OpCodeData;

        public override string ToString()
        {
            return $"{OpCode.ToString()}";
        }
    }
    public class Chunk
    {
        public Chunk()
        {
            m_Code = new List<ByteCode>();
            m_Constants = new List<ConstantValue>();
            m_Lines = new List<int>();
        }
        private List<ByteCode> m_Code;
        private List<ConstantValue> m_Constants;
        private List<int> m_Lines;
        public List<ByteCode> Code
        {
            get => m_Code;
            set => m_Code = value;
        }

        public List<ConstantValue> Constants
        {
            get => m_Constants;
            set => m_Constants = value;
        }

        public List<int> Lines
        {
            get => m_Lines;
            set => m_Lines = value;
        }
    }

    public class BinaryChunk
    {
        public BinaryChunk()
        {
            Code = Array.Empty<byte>();
            Constants = new List<ConstantValue>();
            Lines = new List<int>();
        }

        public BinaryChunk(byte[] code, List<ConstantValue> constants, List<int> lines)
        {
            Code = code;
            Constants = constants;
            Lines = lines;
        }
        public byte[] Code;
        public List<ConstantValue> Constants;
        public List<int> Lines;
    }

}
