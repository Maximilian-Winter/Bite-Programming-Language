using System.Runtime.InteropServices;

namespace Bite.Runtime
{

[StructLayout( LayoutKind.Explicit )]
internal struct IntByteStruct
{
    [FieldOffset( 0 )]
    public byte byte0;

    [FieldOffset( 1 )]
    public byte byte1;

    [FieldOffset( 2 )]
    public byte byte2;

    [FieldOffset( 3 )]
    public byte byte3;

    [FieldOffset( 0 )]
    public int integer;
}

}
