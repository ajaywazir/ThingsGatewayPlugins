

using System;


namespace Opc.Da
{
  [Serializable]
  public class ItemValue : ItemIdentifier
  {
    private object m_value;
    private Quality m_quality = Quality.Bad;
    private bool m_qualitySpecified;
    private DateTime m_timestamp = DateTime.MinValue;
    private bool m_timestampSpecified;

    public object Value
    {
      get => this.m_value;
      set => this.m_value = value;
    }

    public Quality Quality
    {
      get => this.m_quality;
      set => this.m_quality = value;
    }

    public bool QualitySpecified
    {
      get => this.m_qualitySpecified;
      set => this.m_qualitySpecified = value;
    }

    public DateTime Timestamp
    {
      get => this.m_timestamp;
      set => this.m_timestamp = value;
    }

    public bool TimestampSpecified
    {
      get => this.m_timestampSpecified;
      set => this.m_timestampSpecified = value;
    }

    public ItemValue()
    {
    }

    public ItemValue(ItemIdentifier item)
    {
      if (item == null)
        return;
      this.ItemName = item.ItemName;
      this.ItemPath = item.ItemPath;
      this.ClientHandle = item.ClientHandle;
      this.ServerHandle = item.ServerHandle;
    }

    public ItemValue(string itemName)
      : base(itemName)
    {
    }

    public ItemValue(ItemValue item)
      : base((ItemIdentifier) item)
    {
      if (item == null)
        return;
      this.Value = Opc.Convert.Clone(item.Value);
      this.Quality = item.Quality;
      this.QualitySpecified = item.QualitySpecified;
      this.Timestamp = item.Timestamp;
      this.TimestampSpecified = item.TimestampSpecified;
    }

    public override object Clone()
    {
      ItemValue itemValue = (ItemValue) this.MemberwiseClone();
      itemValue.Value = Opc.Convert.Clone(this.Value);
      return (object) itemValue;
    }
  }
}
