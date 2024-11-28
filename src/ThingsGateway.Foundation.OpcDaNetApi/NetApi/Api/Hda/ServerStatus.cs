

using System;


namespace Opc.Hda
{
  [Serializable]
  public class ServerStatus : ICloneable
  {
    private string m_vendorInfo;
    private string m_productVersion;
    private DateTime m_currentTime = DateTime.MinValue;
    private DateTime m_startTime = DateTime.MinValue;
    private ServerState m_serverState = ServerState.Indeterminate;
    private string m_statusInfo;
    private int m_maxReturnValues;

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

    public int MaxReturnValues
    {
      get => this.m_maxReturnValues;
      set => this.m_maxReturnValues = value;
    }

    public virtual object Clone() => this.MemberwiseClone();
  }
}
