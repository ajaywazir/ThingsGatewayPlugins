

using System;


namespace Opc.Da
{
  [Flags]
  public enum StateMask
  {
    Name = 1,
    ClientHandle = 2,
    Locale = 4,
    Active = 8,
    UpdateRate = 16, // 0x00000010
    KeepAlive = 32, // 0x00000020
    ReqType = 64, // 0x00000040
    Deadband = 128, // 0x00000080
    SamplingRate = 256, // 0x00000100
    EnableBuffering = 512, // 0x00000200
    All = 65535, // 0x0000FFFF
  }
}
