using System;
using System.Collections.Generic;

namespace Bite.Runtime.Bytecode
{

public class ByteCode
{
    public BiteVmOpCodes OpCode;
    public int[] OpCodeData;

    #region Public

    public ByteCode( BiteVmOpCodes opCode )
    {
        OpCode = opCode;
    }

    public ByteCode( BiteVmOpCodes opCode, int opCodeData )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData };
    }

    public ByteCode( BiteVmOpCodes opCode, int opCodeData1, int opCodeData2 )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2 };
    }

    public ByteCode( BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3 )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3 };
    }

    public ByteCode( BiteVmOpCodes opCode, int opCodeData1, int opCodeData2, int opCodeData3, int opCodeData4 )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4 };
    }

    public ByteCode(
        BiteVmOpCodes opCode,
        int opCodeData1,
        int opCodeData2,
        int opCodeData3,
        int opCodeData4,
        int opCodeData5 )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5 };
    }

    public ByteCode(
        BiteVmOpCodes opCode,
        int opCodeData1,
        int opCodeData2,
        int opCodeData3,
        int opCodeData4,
        int opCodeData5,
        int opCodeData6 )
    {
        OpCode = opCode;
        OpCodeData = new[] { opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6 };
    }

    public ByteCode(
        BiteVmOpCodes opCode,
        int opCodeData1,
        int opCodeData2,
        int opCodeData3,
        int opCodeData4,
        int opCodeData5,
        int opCodeData6,
        int opCodeData7 )
    {
        OpCode = opCode;

        OpCodeData = new[]
        {
            opCodeData1, opCodeData2, opCodeData3, opCodeData4, opCodeData5, opCodeData6, opCodeData7
        };
    }

    public override string ToString()
    {
        return $"{OpCode.ToString()}";
    }

    #endregion
}

public class Chunk
{
    public List < ByteCode > Code { get; set; }

    public List < ConstantValue > Constants { get; set; }

    public List < int > Lines { get; set; }

    #region Public

    public Chunk()
    {
        Code = new List < ByteCode >();
        Constants = new List < ConstantValue >();
        Lines = new List < int >();
    }

    #endregion
}

public class BinaryChunk
{
    public byte[] Code;
    public List < ConstantValue > Constants;
    public List < int > Lines;

    #region Public

    public BinaryChunk()
    {
        Code = Array.Empty < byte >();
        Constants = new List < ConstantValue >();
        Lines = new List < int >();
    }

    public BinaryChunk( byte[] code, List < ConstantValue > constants, List < int > lines )
    {
        Code = code;
        Constants = constants;
        Lines = lines;
    }

    #endregion
}

}
