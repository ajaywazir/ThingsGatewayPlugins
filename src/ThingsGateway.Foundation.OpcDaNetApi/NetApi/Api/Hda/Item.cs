

using System;


namespace Opc.Hda
{
  [Serializable]
  public class Item : ItemIdentifier
  {
    private int m_aggregateID;

    public int AggregateID
    {
      get => this.m_aggregateID;
      set => this.m_aggregateID = value;
    }

    public Item()
    {
    }

    public Item(ItemIdentifier item)
      : base(item)
    {
    }

    public Item(Item item)
      : base((ItemIdentifier) item)
    {
      if (item == null)
        return;
      this.AggregateID = item.AggregateID;
    }
  }
}
