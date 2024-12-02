

using System;


namespace Opc.Ae
{
    [Serializable]
    public class ItemUrl : ItemIdentifier
    {
        private URL m_url = new URL();

        public URL Url
        {
            get => m_url;
            set => m_url = value;
        }

        public ItemUrl()
        {
        }

        public ItemUrl(ItemIdentifier item)
          : base(item)
        {
        }

        public ItemUrl(ItemIdentifier item, URL url)
          : base(item)
        {
            Url = url;
        }

        public ItemUrl(ItemUrl item)
          : base((ItemIdentifier)item)
        {
            if (item == null)
                return;
            Url = item.Url;
        }

        public override object Clone() => (object)new ItemUrl(this);
    }
}
