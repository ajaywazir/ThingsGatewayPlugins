

using System;


namespace Opc.Dx
{
    [Serializable]
    public class ItemIdentifier : Opc.ItemIdentifier
    {
        private string m_version;

        public string Version
        {
            get => m_version;
            set => m_version = value;
        }

        public ItemIdentifier()
        {
        }

        public ItemIdentifier(string itemName)
          : base(itemName)
        {
        }

        public ItemIdentifier(string itemPath, string itemName)
          : base(itemPath, itemName)
        {
        }

        public ItemIdentifier(Opc.ItemIdentifier item)
          : base(item)
        {
        }

        public ItemIdentifier(ItemIdentifier item)
          : base((Opc.ItemIdentifier)item)
        {
            if (item == null)
                return;
            m_version = item.m_version;
        }

        public override bool Equals(object target)
        {
            if (!typeof(ItemIdentifier).IsInstanceOfType(target))
                return false;
            ItemIdentifier itemIdentifier = (ItemIdentifier)target;
            return itemIdentifier.ItemName == ItemName && itemIdentifier.ItemPath == ItemPath && itemIdentifier.Version == Version;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
