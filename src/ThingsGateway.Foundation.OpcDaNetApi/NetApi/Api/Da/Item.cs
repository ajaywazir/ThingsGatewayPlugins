

using System;


namespace Opc.Da
{
  [Serializable]
  public class Item : ItemIdentifier
  {
    private System.Type m_reqType;
    private int m_maxAge;
    private bool m_maxAgeSpecified;
    private bool m_active = true;
    private bool m_activeSpecified;
    private float m_deadband;
    private bool m_deadbandSpecified;
    private int m_samplingRate;
    private bool m_samplingRateSpecified;
    private bool m_enableBuffering;
    private bool m_enableBufferingSpecified;

    public System.Type ReqType
    {
      get => this.m_reqType;
      set => this.m_reqType = value;
    }

    public int MaxAge
    {
      get => this.m_maxAge;
      set => this.m_maxAge = value;
    }

    public bool MaxAgeSpecified
    {
      get => this.m_maxAgeSpecified;
      set => this.m_maxAgeSpecified = value;
    }

    public bool Active
    {
      get => this.m_active;
      set => this.m_active = value;
    }

    public bool ActiveSpecified
    {
      get => this.m_activeSpecified;
      set => this.m_activeSpecified = value;
    }

    public float Deadband
    {
      get => this.m_deadband;
      set => this.m_deadband = value;
    }

    public bool DeadbandSpecified
    {
      get => this.m_deadbandSpecified;
      set => this.m_deadbandSpecified = value;
    }

    public int SamplingRate
    {
      get => this.m_samplingRate;
      set => this.m_samplingRate = value;
    }

    public bool SamplingRateSpecified
    {
      get => this.m_samplingRateSpecified;
      set => this.m_samplingRateSpecified = value;
    }

    public bool EnableBuffering
    {
      get => this.m_enableBuffering;
      set => this.m_enableBuffering = value;
    }

    public bool EnableBufferingSpecified
    {
      get => this.m_enableBufferingSpecified;
      set => this.m_enableBufferingSpecified = value;
    }

    public Item()
    {
    }

    public Item(ItemIdentifier item)
    {
      if (item == null)
        return;
      this.ItemName = item.ItemName;
      this.ItemPath = item.ItemPath;
      this.ClientHandle = item.ClientHandle;
      this.ServerHandle = item.ServerHandle;
    }

    public Item(Item item)
      : base((ItemIdentifier) item)
    {
      if (item == null)
        return;
      this.ReqType = item.ReqType;
      this.MaxAge = item.MaxAge;
      this.MaxAgeSpecified = item.MaxAgeSpecified;
      this.Active = item.Active;
      this.ActiveSpecified = item.ActiveSpecified;
      this.Deadband = item.Deadband;
      this.DeadbandSpecified = item.DeadbandSpecified;
      this.SamplingRate = item.SamplingRate;
      this.SamplingRateSpecified = item.SamplingRateSpecified;
      this.EnableBuffering = item.EnableBuffering;
      this.EnableBufferingSpecified = item.EnableBufferingSpecified;
    }
  }
}
