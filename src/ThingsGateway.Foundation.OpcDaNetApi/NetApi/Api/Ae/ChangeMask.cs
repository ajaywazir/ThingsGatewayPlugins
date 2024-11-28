

using System;


namespace Opc.Ae
{
  [Flags]
  public enum ChangeMask
  {
    ActiveState = 1,
    AcknowledgeState = 2,
    EnableState = 4,
    Quality = 8,
    Severity = 16, // 0x00000010
    SubCondition = 32, // 0x00000020
    Message = 64, // 0x00000040
    Attribute = 128, // 0x00000080
  }
}
