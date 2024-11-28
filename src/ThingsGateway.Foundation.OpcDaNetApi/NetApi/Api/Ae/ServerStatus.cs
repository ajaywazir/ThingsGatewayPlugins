

using System;


namespace Opc.Ae
{
  [Serializable]
  public class ServerStatus : ICloneable
  {
    private string m_vendorInfo;
    private string m_productVersion;
    private ServerState m_serverState;
    private string m_statusInfo;
    private DateTime m_startTime = DateTime.MinValue;
    private DateTime m_currentTime = DateTime.MinValue;
    private DateTime m_lastUpdateTime = DateTime.MinValue;

    public string VendorInfo
    {
      get => this.m_vendorInfo;
      set => this.m_vendorInfo = value;
    }

    public string ProductVersion
    {
      get => this.m_productVersion;
      set => this.m_productVersion = value;
    }

    public ServerState ServerState
    {
      get => this.m_serverState;
      set => this.m_serverState = value;
    }

    public string StatusInfo
    {
      get => this.m_statusInfo;
      set => this.m_statusInfo = value;
    }

    public DateTime StartTime
    {
      get => this.m_startTime;
      set => this.m_startTime = value;
    }

    public DateTime CurrentTime
    {
      get => this.m_currentTime;
      set => this.m_currentTime = value;
    }

    public DateTime LastUpdateTime
    {
      get => this.m_lastUpdateTime;
      set => this.m_lastUpdateTime = value;
    }

    public virtual object Clone() => this.MemberwiseClone();
  }
}
