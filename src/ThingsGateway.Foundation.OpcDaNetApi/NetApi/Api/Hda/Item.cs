

using System;


namespace Opc.Hda
{
    [Serializable]
    public class Item : ItemIdentifier
    {
        private int m_aggregateID;

        public int AggregateID
        {
            get => m_aggregateID;
            set => m_aggregateID = value;
        }

        public Item()
        {
        }

        public Item(ItemIdentifier item)
          : base(item)
        {
        }

        public Item(Item item)
          : base((ItemIdentifier)item)
        {
            if (item == null)
                return;
            AggregateID = item.AggregateID;
        }
    }
}
