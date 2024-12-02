

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;


namespace Opc
{
    [Serializable]
    public class Server : IServer, IDisposable, ISerializable, ICloneable
    {
        private bool m_disposed;
        protected IServer m_server;
        private URL m_url;
        protected IFactory m_factory;
        private ConnectData m_connectData;
        private string m_name;
        private string m_locale;
        private string[] m_supportedLocales;
        protected ResourceManager m_resourceManager;

        public Server(Factory factory, URL url)
        {
            m_factory = factory != null ? (IFactory)factory.Clone() : throw new ArgumentNullException(nameof(factory));
            m_server = (IServer)null;
            m_url = (URL)null;
            m_name = (string)null;
            m_supportedLocales = (string[])null;
            m_resourceManager = new ResourceManager("Opc.Resources.Strings", Assembly.GetExecutingAssembly());
            if (url == null)
                return;
            SetUrl(url);
        }

        ~Server() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            if (disposing)
            {
                if (m_factory != null)
                {
                    m_factory.Dispose();
                    m_factory = (IFactory)null;
                }
                if (m_server != null)
                {
                    try
                    {
                        Disconnect();
                    }
                    catch
                    {
                    }
                    m_server = (IServer)null;
                }
            }
            m_disposed = true;
        }

        protected Server(SerializationInfo info, StreamingContext context)
        {
            m_name = info.GetString(nameof(Name));
            m_url = (URL)info.GetValue(nameof(Url), typeof(URL));
            m_factory = (IFactory)info.GetValue("Factory", typeof(IFactory));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", (object)m_name);
            info.AddValue("Url", (object)m_url);
            info.AddValue("Factory", (object)m_factory);
        }

        public virtual object Clone()
        {
            Server server = (Server)MemberwiseClone();
            server.m_server = (IServer)null;
            server.m_supportedLocales = (string[])null;
            server.m_locale = (string)null;
            server.m_resourceManager = new ResourceManager("Opc.Resources.Strings", Assembly.GetExecutingAssembly());
            return (object)server;
        }

        public virtual string Name
        {
            get => m_name;
            set => m_name = value;
        }

        public virtual URL Url
        {
            get => m_url == null ? (URL)null : (URL)m_url.Clone();
            set => SetUrl(value);
        }

        public virtual string Locale => m_locale;

        public virtual string[] SupportedLocales
        {
            get
            {
                return m_supportedLocales == null ? (string[])null : (string[])m_supportedLocales.Clone();
            }
        }

        public virtual bool IsConnected => m_server != null;

        public virtual void Connect() => Connect(m_url, (ConnectData)null);

        public virtual void Connect(ConnectData connectData) => Connect(m_url, connectData);

        public virtual void Connect(URL url, ConnectData connectData)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (m_server != null)
                throw new AlreadyConnectedException();
            SetUrl(url);
            try
            {
                m_server = m_factory.CreateInstance(url, connectData);
                m_connectData = connectData;
                GetSupportedLocales();
                SetLocale(m_locale);
            }
            catch (Exception ex)
            {
                if (m_server != null)
                {
                    try
                    {
                        Disconnect();
                    }
                    catch
                    {
                    }
                }
                throw ex;
            }
        }

        public virtual void Disconnect()
        {
            if (m_server == null)
                throw new NotConnectedException();
            m_server.Dispose();
            m_server = (IServer)null;
        }

        public virtual Server Duplicate()
        {
            Server instance = (Server)Activator.CreateInstance(GetType(), (object)m_factory, (object)m_url);
            instance.m_connectData = m_connectData;
            instance.m_locale = m_locale;
            return instance;
        }

        public virtual event ServerShutdownEventHandler ServerShutdown
        {
            add => m_server.ServerShutdown += value;
            remove => m_server.ServerShutdown -= value;
        }

        public virtual string GetLocale()
        {
            m_locale = m_server != null ? m_server.GetLocale() : throw new NotConnectedException();
            return m_locale;
        }

        public virtual string SetLocale(string locale)
        {
            if (m_server == null)
                throw new NotConnectedException();
            try
            {
                m_locale = m_server.SetLocale(locale);
            }
            catch
            {
                string bestLocale = Server.FindBestLocale(locale, m_supportedLocales);
                if (bestLocale != locale)
                    m_server.SetLocale(bestLocale);
                m_locale = bestLocale;
            }
            return m_locale;
        }

        public virtual string[] GetSupportedLocales()
        {
            m_supportedLocales = m_server != null ? m_server.GetSupportedLocales() : throw new NotConnectedException();
            return SupportedLocales;
        }

        public virtual string GetErrorText(string locale, ResultID resultID)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return m_server.GetErrorText(locale == null ? m_locale : locale, resultID);
        }

        protected string GetString(string name)
        {
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(Locale);
            }
            catch
            {
                culture = new CultureInfo("");
            }
            try
            {
                return m_resourceManager.GetString(name, culture);
            }
            catch
            {
                return (string)null;
            }
        }

        protected void SetUrl(URL url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (m_server != null)
                throw new AlreadyConnectedException();
            m_url = (URL)url.Clone();
            string str1 = "";
            if (m_url.HostName != null)
            {
                str1 = m_url.HostName.ToLower();
                if (str1 == "localhost" || str1 == "127.0.0.1")
                    str1 = "";
            }
            if (m_url.Port != 0)
                str1 += string.Format(".{0}", (object)m_url.Port);
            if (!string.IsNullOrEmpty(str1))
                str1 += ".";
            string str2;
            if (m_url.Scheme != "http")
            {
                string str3 = m_url.Path;
                int length = str3.LastIndexOf('/');
                if (length != -1)
                    str3 = str3.Substring(0, length);
                str2 = str1 + str3;
            }
            else
            {
                string str4 = m_url.Path;
                int length = str4.LastIndexOf('.');
                if (length != -1)
                    str4 = str4.Substring(0, length);
                while (str4.IndexOf('/') != -1)
                    str4 = str4.Replace('/', '-');
                str2 = str1 + str4;
            }
            m_name = str2;
        }

        public static string FindBestLocale(string requestedLocale, string[] supportedLocales)
        {
            try
            {
                foreach (string supportedLocale in supportedLocales)
                {
                    if (supportedLocale == requestedLocale)
                        return requestedLocale;
                }
                CultureInfo cultureInfo1 = new CultureInfo(requestedLocale);
                foreach (string supportedLocale in supportedLocales)
                {
                    try
                    {
                        CultureInfo cultureInfo2 = new CultureInfo(supportedLocale);
                        if (cultureInfo1.Parent.Name == cultureInfo2.Name)
                            return cultureInfo2.Name;
                    }
                    catch
                    {
                    }
                }
                return supportedLocales == null || supportedLocales.Length == 0 ? "" : supportedLocales[0];
            }
            catch
            {
                return supportedLocales == null || supportedLocales.Length == 0 ? "" : supportedLocales[0];
            }
        }

        private sealed class Names
        {
            internal const string NAME = "Name";
            internal const string URL = "Url";
            internal const string FACTORY = "Factory";
        }
    }
}
