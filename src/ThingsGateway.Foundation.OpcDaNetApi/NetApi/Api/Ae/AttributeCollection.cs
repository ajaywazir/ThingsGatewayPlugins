

using System;
using System.Collections;


namespace Opc.Ae
{
    [Serializable]
    public class AttributeCollection : WriteableCollection
    {
        public new int this[int index] => (int)Array[index];

        public new int[] ToArray() => (int[])Array.ToArray(typeof(int));

        internal AttributeCollection()
          : base((ICollection)null, typeof(int))
        {
        }

        internal AttributeCollection(int[] attributeIDs)
          : base((ICollection)attributeIDs, typeof(int))
        {
        }
    }
}
