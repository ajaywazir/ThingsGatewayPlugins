

using System;


namespace Opc.Ae
{
  [Serializable]
  public class ItemUrl : ItemIdentifier
  {
    private URL m_url = new URL();

    public URL Url
    {
      get => this.m_url;
      set => this.m_url = value;
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
      this.Url = url;
    }

    public ItemUrl(ItemUrl item)
      : base((ItemIdentifier) item)
    {
      if (item == null)
        return;
      this.Url = item.Url;
    }

    public override object Clone() => (object) new ItemUrl(this);
  }
}
