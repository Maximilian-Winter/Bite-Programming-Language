using System;
using System.Collections.Generic;

namespace Srsl_Parser.Runtime
{

public class ByteCode
{
    public ByteCode(SrslVmOpCodes opCode)
    {
        OpCode = opCode;
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2 };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3 };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4 };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5 };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5, int opCodeData6)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6 };
    }
    
    public ByteCode(SrslVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4, int opCodeData5, int opCodeData6, int opCodeData7)
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6, opCodeData7 };
    }
    
    public SrslVmOpCodes OpCode;
    public int[] OpCodeData;

   /* public SrslVmOpCodes OpCode
    {
        get => m_OpCode;
        set => m_OpCode = value;
    }

    public int[] OpCodeData
    {
        get => m_OpCodeData;
        set => m_OpCodeData = value;
    }*/

    public override string ToString()
    {
        return $"{OpCode.ToString()}";
    }
}
public class Chunk
{
    public Chunk()
    {
        m_Code = new List < ByteCode >();
        m_Constants = new List < DynamicSrslVariable >();
        m_Lines = new List < int >();
    }
    private List < ByteCode > m_Code;
    private List < DynamicSrslVariable > m_Constants;
    private List < int > m_Lines;
    public List < ByteCode > Code
    {
        get => m_Code;
        set => m_Code = value;
    }
    
    public List < DynamicSrslVariable > Constants
    {
        get => m_Constants;
        set => m_Constants = value;
    }

    public List < int > Lines
    {
        get => m_Lines;
        set => m_Lines = value;
    }
}

public class BinaryChunk
{
    public BinaryChunk()
    {
        Code = Array.Empty < byte >();
        Constants = new List < DynamicSrslVariable >();
        Lines = new List < int >();
    }
    
    public BinaryChunk(byte[] code, List <DynamicSrslVariable> constants, List <int> lines )
    {
        Code = code;
        Constants = constants;
        Lines = lines;
    }
    public byte[] Code;
    public List < DynamicSrslVariable > Constants;
    public List < int > Lines;
}

}
