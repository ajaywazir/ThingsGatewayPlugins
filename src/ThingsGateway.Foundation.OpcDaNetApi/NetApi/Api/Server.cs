

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
      this.m_factory = factory != null ? (IFactory) factory.Clone() : throw new ArgumentNullException(nameof (factory));
      this.m_server = (IServer) null;
      this.m_url = (URL) null;
      this.m_name = (string) null;
      this.m_supportedLocales = (string[]) null;
      this.m_resourceManager = new ResourceManager("Opc.Resources.Strings", Assembly.GetExecutingAssembly());
      if (url == null)
        return;
      this.SetUrl(url);
    }

    ~Server() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      if (disposing)
      {
        if (this.m_factory != null)
        {
          this.m_factory.Dispose();
          this.m_factory = (IFactory) null;
        }
        if (this.m_server != null)
        {
          try
          {
            this.Disconnect();
          }
          catch
          {
          }
          this.m_server = (IServer) null;
        }
      }
      this.m_disposed = true;
    }

    protected Server(SerializationInfo info, StreamingContext context)
    {
      this.m_name = info.GetString(nameof (Name));
      this.m_url = (URL) info.GetValue(nameof (Url), typeof (URL));
      this.m_factory = (IFactory) info.GetValue("Factory", typeof (IFactory));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Name", (object) this.m_name);
      info.AddValue("Url", (object) this.m_url);
      info.AddValue("Factory", (object) this.m_factory);
    }

    public virtual object Clone()
    {
      Server server = (Server) this.MemberwiseClone();
      server.m_server = (IServer) null;
      server.m_supportedLocales = (string[]) null;
      server.m_locale = (string) null;
      server.m_resourceManager = new ResourceManager("Opc.Resources.Strings", Assembly.GetExecutingAssembly());
      return (object) server;
    }

    public virtual string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public virtual URL Url
    {
      get => this.m_url == null ? (URL) null : (URL) this.m_url.Clone();
      set => this.SetUrl(value);
    }

    public virtual string Locale => this.m_locale;

    public virtual string[] SupportedLocales
    {
      get
      {
        return this.m_supportedLocales == null ? (string[]) null : (string[]) this.m_supportedLocales.Clone();
      }
    }

    public virtual bool IsConnected => this.m_server != null;

    public virtual void Connect() => this.Connect(this.m_url, (ConnectData) null);

    public virtual void Connect(ConnectData connectData) => this.Connect(this.m_url, connectData);

    public virtual void Connect(URL url, ConnectData connectData)
    {
      if (url == null)
        throw new ArgumentNullException(nameof (url));
      if (this.m_server != null)
        throw new AlreadyConnectedException();
      this.SetUrl(url);
      try
      {
        this.m_server = this.m_factory.CreateInstance(url, connectData);
        this.m_connectData = connectData;
        this.GetSupportedLocales();
        this.SetLocale(this.m_locale);
      }
      catch (Exception ex)
      {
        if (this.m_server != null)
        {
          try
          {
            this.Disconnect();
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
      if (this.m_server == null)
        throw new NotConnectedException();
      this.m_server.Dispose();
      this.m_server = (IServer) null;
    }

    public virtual Server Duplicate()
    {
      Server instance = (Server) Activator.CreateInstance(this.GetType(), (object) this.m_factory, (object) this.m_url);
      instance.m_connectData = this.m_connectData;
      instance.m_locale = this.m_locale;
      return instance;
    }

    public virtual event ServerShutdownEventHandler ServerShutdown
    {
      add => this.m_server.ServerShutdown += value;
      remove => this.m_server.ServerShutdown -= value;
    }

    public virtual string GetLocale()
    {
      this.m_locale = this.m_server != null ? this.m_server.GetLocale() : throw new NotConnectedException();
      return this.m_locale;
    }

    public virtual string SetLocale(string locale)
    {
      if (this.m_server == null)
        throw new NotConnectedException();
      try
      {
        this.m_locale = this.m_server.SetLocale(locale);
      }
      catch
      {
        string bestLocale = Server.FindBestLocale(locale, this.m_supportedLocales);
        if (bestLocale != locale)
          this.m_server.SetLocale(bestLocale);
        this.m_locale = bestLocale;
      }
      return this.m_locale;
    }

    public virtual string[] GetSupportedLocales()
    {
      this.m_supportedLocales = this.m_server != null ? this.m_server.GetSupportedLocales() : throw new NotConnectedException();
      return this.SupportedLocales;
    }

    public virtual string GetErrorText(string locale, ResultID resultID)
    {
      if (this.m_server == null)
        throw new NotConnectedException();
      return this.m_server.GetErrorText(locale == null ? this.m_locale : locale, resultID);
    }

    protected string GetString(string name)
    {
      CultureInfo culture;
      try
      {
        culture = new CultureInfo(this.Locale);
      }
      catch
      {
        culture = new CultureInfo("");
      }
      try
      {
        return this.m_resourceManager.GetString(name, culture);
      }
      catch
      {
        return (string) null;
      }
    }

    protected void SetUrl(URL url)
    {
      if (url == null)
        throw new ArgumentNullException(nameof (url));
      if (this.m_server != null)
        throw new AlreadyConnectedException();
      this.m_url = (URL) url.Clone();
      string str1 = "";
      if (this.m_url.HostName != null)
      {
        str1 = this.m_url.HostName.ToLower();
        if (str1 == "localhost" || str1 == "127.0.0.1")
          str1 = "";
      }
      if (this.m_url.Port != 0)
        str1 += string.Format(".{0}", (object) this.m_url.Port);
      if (str1 != "")
        str1 += ".";
      string str2;
      if (this.m_url.Scheme != "http")
      {
        string str3 = this.m_url.Path;
        int length = str3.LastIndexOf('/');
        if (length != -1)
          str3 = str3.Substring(0, length);
        str2 = str1 + str3;
      }
      else
      {
        string str4 = this.m_url.Path;
        int length = str4.LastIndexOf('.');
        if (length != -1)
          str4 = str4.Substring(0, length);
        while (str4.IndexOf('/') != -1)
          str4 = str4.Replace('/', '-');
        str2 = str1 + str4;
      }
      this.m_name = str2;
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

    private class Names
    {
      internal const string NAME = "Name";
      internal const string URL = "Url";
      internal const string FACTORY = "Factory";
    }
  }
}
