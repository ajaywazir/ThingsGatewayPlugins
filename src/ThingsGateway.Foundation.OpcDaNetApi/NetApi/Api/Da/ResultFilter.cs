

using System;


namespace Opc.Da
{
  [Flags]
  public enum ResultFilter
  {
    ItemName = 1,
    ItemPath = 2,
    ClientHandle = 4,
    ItemTime = 8,
    ErrorText = 16, // 0x00000010
    DiagnosticInfo = 32, // 0x00000020
    Minimal = ItemTime | ItemName, // 0x00000009
    All = Minimal | DiagnosticInfo | ErrorText | ClientHandle | ItemPath, // 0x0000003F
  }
}
