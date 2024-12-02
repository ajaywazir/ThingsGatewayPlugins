

using Opc;
using Opc.Da;

using System;


namespace OpcCom.Da20
{
    [Serializable]
    internal sealed class BrowsePosition : Opc.Da.BrowsePosition
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
            Enumerator = enumerator;
            IsBranch = isBranch;
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing && Enumerator != null)
                {
                    Enumerator.Dispose();
                    Enumerator = (EnumString)null;
                }
                m_disposed = true;
            }
            base.Dispose(disposing);
        }

        public override object Clone()
        {
            BrowsePosition browsePosition = (BrowsePosition)MemberwiseClone();
            browsePosition.Enumerator = Enumerator.Clone();
            return (object)browsePosition;
        }
    }
}
