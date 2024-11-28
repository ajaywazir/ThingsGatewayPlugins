

using System;


namespace Opc.Ae
{
  public class ItemUrlCollection : ReadOnlyCollection
  {
    public new ItemUrl this[int index] => (ItemUrl) this.Array.GetValue(index);

    public new ItemUrl[] ToArray() => (ItemUrl[]) Opc.Convert.Clone((object) this.Array);

    public ItemUrlCollection()
      : base((Array) new ItemUrl[0])
    {
    }

    public ItemUrlCollection(ItemUrl[] itemUrls)
      : base((Array) itemUrls)
    {
    }
  }
}
