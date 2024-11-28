

using Opc;
using Opc.Da;
using System;


namespace OpcCom.Da20
{
  [Serializable]
  internal class BrowsePosition : Opc.Da.BrowsePosition
  {
    internal EnumString Enumerator;
    internal bool IsBranch = true;
    internal string[] Names;
    internal int Index;
    private bool m_disposed;

    internal BrowsePosition(
      ItemIdentifier itemID,
      BrowseFilters filters,
      EnumString enumerator,
      bool isBranch)
      : base(itemID, filters)
    {
      this.Enumerator = enumerator;
      this.IsBranch = isBranch;
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        if (disposing && this.Enumerator != null)
        {
          this.Enumerator.Dispose();
          this.Enumerator = (EnumString) null;
        }
        this.m_disposed = true;
      }
      base.Dispose(disposing);
    }

    public override object Clone()
    {
      BrowsePosition browsePosition = (BrowsePosition) this.MemberwiseClone();
      browsePosition.Enumerator = this.Enumerator.Clone();
      return (object) browsePosition;
    }
  }
}
