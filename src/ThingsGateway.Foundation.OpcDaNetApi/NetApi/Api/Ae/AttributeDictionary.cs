

using System;
using System.Collections;


namespace Opc.Ae
{
  [Serializable]
  public class AttributeDictionary : WriteableDictionary
  {
    public AttributeCollection this[int categoryID]
    {
      get => (AttributeCollection) this[(object) categoryID];
      set
      {
        if (value != null)
          this[(object) categoryID] = (object) value;
        else
          this[(object) categoryID] = (object) new AttributeCollection();
      }
    }

    public virtual void Add(int key, int[] value)
    {
      if (value != null)
        this.Add((object) key, (object) new AttributeCollection(value));
      else
        this.Add((object) key, (object) new AttributeCollection());
    }

    public AttributeDictionary()
      : base((IDictionary) null, typeof (int), typeof (AttributeCollection))
    {
    }

    public AttributeDictionary(int[] categoryIDs)
      : base((IDictionary) null, typeof (int), typeof (AttributeCollection))
    {
      for (int index = 0; index < categoryIDs.Length; ++index)
        this.Add(categoryIDs[index], (int[]) null);
    }
  }
}
