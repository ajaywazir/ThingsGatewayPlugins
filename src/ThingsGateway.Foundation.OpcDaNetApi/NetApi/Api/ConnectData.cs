

using System;
using System.Net;
using System.Runtime.Serialization;


namespace Opc
{
    [Serializable]
    public class ConnectData : ISerializable, ICredentials
    {
        private NetworkCredential m_credentials;
        private WebProxy m_proxy;
        private string m_licenseKey;

        public NetworkCredential Credentials
        {
            get => m_credentials;
            set => m_credentials = value;
        }

        public string LicenseKey
        {
            get => m_licenseKey;
            set => m_licenseKey = value;
        }

        public bool AlwaysUseDA20 { get; set; }

        public NetworkCredential GetCredential(Uri uri, string authenticationType)
        {
            return m_credentials != null ? new NetworkCredential(m_credentials.UserName, m_credentials.Password, m_credentials.Domain) : (NetworkCredential)null;
        }

        public IWebProxy GetProxy()
        {
            return m_proxy != null ? (IWebProxy)m_proxy : (IWebProxy)new WebProxy();
        }

        public void SetProxy(WebProxy proxy) => m_proxy = proxy;

        public ConnectData(NetworkCredential credentials)
        {
            m_credentials = credentials;
            m_proxy = (WebProxy)null;
        }

        public ConnectData(NetworkCredential credentials, WebProxy proxy)
        {
            m_credentials = credentials;
            m_proxy = proxy;
        }

        protected ConnectData(SerializationInfo info, StreamingContext context)
        {
            string userName = info.GetString("UN");
            string password = info.GetString("PW");
            string domain = info.GetString("DO");
            string Address = info.GetString("PU");
            info.GetString("LK");
            m_credentials = domain == null ? new NetworkCredential(userName, password) : new NetworkCredential(userName, password, domain);
            if (Address != null)
                m_proxy = new WebProxy(Address);
            else
                m_proxy = (WebProxy)null;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (m_credentials != null)
            {
                info.AddValue("UN", (object)m_credentials.UserName);
                info.AddValue("PW", (object)m_credentials.Password);
                info.AddValue("DO", (object)m_credentials.Domain);
            }
            else
            {
                info.AddValue("UN", (object)null);
                info.AddValue("PW", (object)null);
                info.AddValue("DO", (object)null);
            }
            if (m_proxy != null)
                info.AddValue("PU", (object)m_proxy.Address);
            else
                info.AddValue("PU", (object)null);
        }

        private sealed class Names
        {
            internal const string USER_NAME = "UN";
            internal const string PASSWORD = "PW";
            internal const string DOMAIN = "DO";
            internal const string PROXY_URI = "PU";
            internal const string LICENSE_KEY = "LK";
        }
    }
}
