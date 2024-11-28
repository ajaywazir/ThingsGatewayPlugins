

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

    public string Scheme
    {
      get => this.m_scheme;
      set => this.m_scheme = value;
    }

    public string HostName
    {
      get => this.m_hostName;
      set => this.m_hostName = value;
    }

    public int Port
    {
      get => this.m_port;
      set => this.m_port = value;
    }

    public string Path
    {
      get => this.m_path;
      set => this.m_path = value;
    }

    public URL()
    {
      this.Scheme = "http";
      this.HostName = "localhost";
      this.Port = 0;
      this.Path = (string) null;
    }

    public URL(string url)
    {
      this.Scheme = "http";
      this.HostName = "localhost";
      this.Port = 0;
      this.Path = (string) null;
      string str1 = url;
      int length1 = str1.IndexOf("://");
      if (length1 >= 0)
      {
        this.Scheme = str1.Substring(0, length1);
        str1 = str1.Substring(length1 + 3);
      }
      int length2 = str1.IndexOfAny(new char[1]{ '/' });
      if (length2 < 0)
      {
        this.Path = str1;
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
          ipAddress = (IPAddress) null;
        }
        if (ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
          if (ipString.Contains("]"))
          {
            this.HostName = ipString.Substring(0, ipString.IndexOf("]") + 1);
            if (ipString.Substring(ipString.IndexOf(']')).Contains(":"))
            {
              string str2 = ipString.Substring(ipString.LastIndexOf(':') + 1);
              if (str2 != "")
              {
                try
                {
                  this.Port = (int) System.Convert.ToUInt16(str2);
                }
                catch
                {
                  this.Port = 0;
                }
              }
              else
                this.Port = 0;
            }
            else
              this.Port = 0;
            this.Path = str1.Substring(length2 + 1);
          }
          else
          {
            this.HostName = "[" + ipString + "]";
            this.Port = 0;
          }
          this.Path = str1.Substring(length2 + 1);
        }
        else
        {
          int num = str1.IndexOfAny(new char[2]{ ':', '/' });
          if (num < 0)
          {
            this.Path = str1;
          }
          else
          {
            this.HostName = str1.Substring(0, num);
            string str3;
            if (str1[num] == ':')
            {
              string str4 = str1.Substring(num + 1);
              int length3 = str4.IndexOf("/");
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
                this.Port = (int) System.Convert.ToUInt16(str5);
              }
              catch
              {
                this.Port = 0;
              }
            }
            else
              str3 = str1.Substring(num + 1);
            this.Path = str3;
          }
        }
      }
    }

    public override string ToString()
    {
      string str = this.HostName == null || this.HostName == "" ? "localhost" : this.HostName;
      return this.Port > 0 ? string.Format("{0}://{1}:{2}/{3}", (object) this.Scheme, (object) str, (object) this.Port, (object) this.Path) : string.Format("{0}://{1}/{2}", new object[3]
      {
        (object) this.Scheme,
        (object) str,
        (object) this.Path
      });
    }

    public override bool Equals(object target)
    {
      URL url = (URL) null;
      if (target != null && target.GetType() == typeof (URL))
        url = (URL) target;
      if (target != null && target.GetType() == typeof (string))
        url = new URL((string) target);
      return url != null && !(url.Path != this.Path) && !(url.Scheme != this.Scheme) && !(url.HostName != this.HostName) && url.Port == this.Port;
    }

    public override int GetHashCode() => this.ToString().GetHashCode();

    public virtual object Clone() => this.MemberwiseClone();
  }
}
