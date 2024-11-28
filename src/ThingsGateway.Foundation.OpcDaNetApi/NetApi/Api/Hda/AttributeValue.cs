

using System;


namespace Opc.Hda
{
  [Serializable]
  public class AttributeValue : ICloneable
  {
    private object m_value;
    private DateTime m_timestamp = DateTime.MinValue;

    public object Value
    {
      get => this.m_value;
      set => this.m_value = value;
    }

    public DateTime Timestamp
    {
      get => this.m_timestamp;
      set => this.m_timestamp = value;
    }

    public virtual object Clone()
    {
      AttributeValue attributeValue = (AttributeValue) this.MemberwiseClone();
      attributeValue.m_value = Opc.Convert.Clone(this.m_value);
      return (object) attributeValue;
    }
  }
}
