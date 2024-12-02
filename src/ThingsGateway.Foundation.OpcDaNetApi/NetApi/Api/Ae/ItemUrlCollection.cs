

using System;


namespace Opc.Ae
{
    public class ItemUrlCollection : ReadOnlyCollection
    {
        public new ItemUrl this[int index] => (ItemUrl)Array.GetValue(index);

        public new ItemUrl[] ToArray() => (ItemUrl[])Opc.Convert.Clone((object)Array);

        public ItemUrlCollection()
          : base(Array.Empty<ItemUrl>())
        {
        }

        public ItemUrlCollection(ItemUrl[] itemUrls)
          : base(itemUrls)
        {
        }
    }
}
