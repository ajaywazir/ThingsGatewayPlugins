

using System;
using System.Net;
using System.Net.Sockets;


namespace Opc
{
    [Serializable]
    public class URL : ICloneable
    {
        private string m_scheme;
        private string m_hostName;
        private int m_port;
        private string m_path;

        private static readonly char[] PathSeparator = new char[] { '/' };
        private static readonly char[] HostPortSeparator = new char[] { ':', '/' };

        public string Scheme
        {
            get => m_scheme;
            set => m_scheme = value;
        }

        public string HostName
        {
            get => m_hostName;
            set => m_hostName = value;
        }

        public int Port
        {
            get => m_port;
            set => m_port = value;
        }

        public string Path
        {
            get => m_path;
            set => m_path = value;
        }

        public URL()
        {
            Scheme = "http";
            HostName = "localhost";
            Port = 0;
            Path = (string)null;
        }

        public URL(string url)
        {
            Scheme = "http";
            HostName = "localhost";
            Port = 0;
            Path = (string)null;
            string str1 = url;
            int length1 = str1.IndexOf("://");
            if (length1 >= 0)
            {
                Scheme = str1.Substring(0, length1);
                str1 = str1.Substring(length1 + 3);
            }
            int length2 = str1.IndexOfAny(PathSeparator);
            if (length2 < 0)
            {
                Path = str1;
            }
            else
            {
                string ipString = str1.Substring(0, length2);
                IPAddress ipAddress;
                try
                {
                    ipAddress = IPAddress.Parse(ipString);
                }
                catch
                {
                    ipAddress = (IPAddress)null;
                }
                if (ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (ipString.Contains(']'))
                    {
                        HostName = ipString.Substring(0, ipString.IndexOf(']') + 1);
                        if (ipString.Substring(ipString.IndexOf(']')).Contains(':'))
                        {
                            string str2 = ipString.Substring(ipString.LastIndexOf(':') + 1);
                            if (!string.IsNullOrEmpty(str2))
                            {
                                try
                                {
                                    Port = (int)System.Convert.ToUInt16(str2);
                                }
                                catch
                                {
                                    Port = 0;
                                }
                            }
                            else
                                Port = 0;
                        }
                        else
                            Port = 0;
                        Path = str1.Substring(length2 + 1);
                    }
                    else
                    {
                        HostName = "[" + ipString + "]";
                        Port = 0;
                    }
                    Path = str1.Substring(length2 + 1);
                }
                else
                {
                    int num = str1.IndexOfAny(HostPortSeparator);
                    if (num < 0)
                    {
                        Path = str1;
                    }
                    else
                    {
                        HostName = str1.Substring(0, num);
                        string str3;
                        if (str1[num] == ':')
                        {
                            string str4 = str1.Substring(num + 1);
                            int length3 = str4.IndexOf('/');
                            string str5;
                            if (length3 >= 0)
                            {
                                str5 = str4.Substring(0, length3);
                                str3 = str4.Substring(length3 + 1);
                            }
                            else
                            {
                                str5 = str4;
                                str3 = "";
                            }
                            try
                            {
                                Port = (int)System.Convert.ToUInt16(str5);
                            }
                            catch
                            {
                                Port = 0;
                            }
                        }
                        else
                            str3 = str1.Substring(num + 1);
                        Path = str3;
                    }
                }
            }
        }

        public override string ToString()
        {
            string str = string.IsNullOrEmpty(HostName) ? "localhost" : HostName;
            return Port > 0 ? string.Format("{0}://{1}:{2}/{3}", (object)Scheme, (object)str, (object)Port, (object)Path) : string.Format("{0}://{1}/{2}", new object[3]
            {
          (object) Scheme,
          (object) str,
          (object) Path
            });
        }

        public override bool Equals(object target)
        {
            URL url = (URL)null;
            if (target != null && target.GetType() == typeof(URL))
                url = (URL)target;
            if (target != null && target.GetType() == typeof(string))
                url = new URL((string)target);
            return url != null && !(url.Path != Path) && !(url.Scheme != Scheme) && !(url.HostName != HostName) && url.Port == Port;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public virtual object Clone() => MemberwiseClone();
    }
}
