

using System;


namespace Opc.Ae
{
    [Serializable]
    public class BrowsePosition : IBrowsePosition, IDisposable, ICloneable
    {
        private bool m_disposed;
        private string m_areaID;
        private BrowseType m_browseType;
        private string m_browseFilter;

        public BrowsePosition(string areaID, BrowseType browseType, string browseFilter)
        {
            m_areaID = areaID;
            m_browseType = browseType;
            m_browseFilter = browseFilter;
        }

        public string AreaID => m_areaID;

        public BrowseType BrowseType => m_browseType;

        public string BrowseFilter => m_browseFilter;

        ~BrowsePosition() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            int num = disposing ? 1 : 0;
            m_disposed = true;
        }

        public virtual object Clone() => (object)(BrowsePosition)MemberwiseClone();
    }
}
