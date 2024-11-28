

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
      this.m_systemType = systemType;
      this.m_useRemoting = useRemoting;
    }

    ~Factory() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      int num = disposing ? 1 : 0;
      this.m_disposed = true;
    }

    protected Factory(SerializationInfo info, StreamingContext context)
    {
      this.m_useRemoting = info.GetBoolean(nameof (UseRemoting));
      this.m_systemType = (System.Type) info.GetValue(nameof (SystemType), typeof (System.Type));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("UseRemoting", this.m_useRemoting);
      info.AddValue("SystemType", (object) this.m_systemType);
    }

    public virtual object Clone() => this.MemberwiseClone();

    public virtual IServer CreateInstance(URL url, ConnectData connectData)
    {
      if (!this.m_useRemoting)
      {
        IServer instance = (IServer) Activator.CreateInstance(this.m_systemType, (object) url, (object) connectData);
      }
      throw new NotSupportedException(".NET remoting not supported in .NET Core.");
    }

    protected System.Type SystemType
    {
      get => this.m_systemType;
      set => this.m_systemType = value;
    }

    protected bool UseRemoting
    {
      get => this.m_useRemoting;
      set => this.m_useRemoting = value;
    }

    private class Names
    {
      internal const string USE_REMOTING = "UseRemoting";
      internal const string SYSTEM_TYPE = "SystemType";
    }
  }
}
