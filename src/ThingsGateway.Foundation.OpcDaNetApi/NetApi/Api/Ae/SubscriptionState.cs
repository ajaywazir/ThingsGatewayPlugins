

using System;


namespace Opc.Ae
{
  [Serializable]
  public class SubscriptionState : ICloneable
  {
    private string m_name;
    private object m_clientHandle;
    private bool m_active = true;
    private int m_bufferTime;
    private int m_maxSize;
    private int m_keepAlive;

    public string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public object ClientHandle
    {
      get => this.m_clientHandle;
      set => this.m_clientHandle = value;
    }

    public bool Active
    {
      get => this.m_active;
      set => this.m_active = value;
    }

    public int BufferTime
    {
      get => this.m_bufferTime;
      set => this.m_bufferTime = value;
    }

    public int MaxSize
    {
      get => this.m_maxSize;
      set => this.m_maxSize = value;
    }

    public int KeepAlive
    {
      get => this.m_keepAlive;
      set => this.m_keepAlive = value;
    }

    public virtual object Clone() => this.MemberwiseClone();
  }
}
