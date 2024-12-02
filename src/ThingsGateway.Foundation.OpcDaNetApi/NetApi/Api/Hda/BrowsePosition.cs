

using System;


namespace Opc.Hda
{
    [Serializable]
    public class BrowsePosition : IBrowsePosition, IDisposable, ICloneable
    {
        private bool m_disposed;

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
