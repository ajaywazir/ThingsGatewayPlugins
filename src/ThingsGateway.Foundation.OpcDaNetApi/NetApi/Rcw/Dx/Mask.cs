


namespace OpcRcw.Dx
{
  public enum Mask
  {
    None = 0,
    ItemPath = 1,
    ItemName = 2,
    Version = 4,
    BrowsePaths = 8,
    Name = 16, // 0x00000010
    Description = 32, // 0x00000020
    Keyword = 64, // 0x00000040
    DefaultSourceItemConnected = 128, // 0x00000080
    DefaultTargetItemConnected = 256, // 0x00000100
    DefaultOverridden = 512, // 0x00000200
    DefaultOverrideValue = 1024, // 0x00000400
    SubstituteValue = 2048, // 0x00000800
    EnableSubstituteValue = 4096, // 0x00001000
    TargetItemPath = 8192, // 0x00002000
    TargetItemName = 16384, // 0x00004000
    SourceServerName = 32768, // 0x00008000
    SourceItemPath = 65536, // 0x00010000
    SourceItemName = 131072, // 0x00020000
    SourceItemQueueSize = 262144, // 0x00040000
    UpdateRate = 524288, // 0x00080000
    DeadBand = 1048576, // 0x00100000
    VendorData = 2097152, // 0x00200000
    ServerType = 4194304, // 0x00400000
    ServerURL = 8388608, // 0x00800000
    DefaultSourceServerConnected = 16777216, // 0x01000000
    All = 2147483647, // 0x7FFFFFFF
  }
}
