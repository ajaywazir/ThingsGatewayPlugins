

using System;
using System.Runtime.Serialization;


namespace Opc
{
    [Serializable]
    public class Factory : IFactory, IDisposable, ISerializable, ICloneable
    {
        private bool m_disposed;
        private System.Type m_systemType;
        private bool m_useRemoting;

        public Factory(System.Type systemType, bool useRemoting)
        {
            m_systemType = systemType;
            m_useRemoting = useRemoting;
        }

        ~Factory() => Dispose(false);

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

        protected Factory(SerializationInfo info, StreamingContext context)
        {
            m_useRemoting = info.GetBoolean(nameof(UseRemoting));
            m_systemType = (System.Type)info.GetValue(nameof(SystemType), typeof(System.Type));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UseRemoting", m_useRemoting);
            info.AddValue("SystemType", (object)m_systemType);
        }

        public virtual object Clone() => MemberwiseClone();

        public virtual IServer CreateInstance(URL url, ConnectData connectData)
        {
            if (!m_useRemoting)
            {
                IServer instance = (IServer)Activator.CreateInstance(m_systemType, (object)url, (object)connectData);
            }
            throw new NotSupportedException(".NET remoting not supported in .NET Core.");
        }

        protected System.Type SystemType
        {
            get => m_systemType;
            set => m_systemType = value;
        }

        protected bool UseRemoting
        {
            get => m_useRemoting;
            set => m_useRemoting = value;
        }

        private sealed class Names
        {
            internal const string USE_REMOTING = "UseRemoting";
            internal const string SYSTEM_TYPE = "SystemType";
        }
    }
}
