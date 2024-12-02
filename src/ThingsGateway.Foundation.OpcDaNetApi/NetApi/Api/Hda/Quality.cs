

using System;


namespace Opc.Hda
{
    [Flags]
    public enum Quality
    {
        ExtraData = 65536, // 0x00010000
        Interpolated = 131072, // 0x00020000
        Raw = 262144, // 0x00040000
        Calculated = 524288, // 0x00080000
        NoBound = 1048576, // 0x00100000
        NoData = 2097152, // 0x00200000
        DataLost = 4194304, // 0x00400000
        Conversion = 8388608, // 0x00800000
        Partial = 16777216, // 0x01000000
    }
}
