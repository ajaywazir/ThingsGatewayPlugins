

using System;


namespace Opc.Ae
{
  [Serializable]
  public class Category : ICloneable
  {
    private int m_id;
    private string m_name;

    public int ID
    {
      get => this.m_id;
      set => this.m_id = value;
    }

    public string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public override string ToString() => this.Name;

    public virtual object Clone() => this.MemberwiseClone();
  }
}
