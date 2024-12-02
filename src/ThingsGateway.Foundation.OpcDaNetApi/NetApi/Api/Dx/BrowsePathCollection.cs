

using System;
using System.Collections;


namespace Opc.Dx
{
    [Serializable]
    public class BrowsePathCollection : ArrayList
    {
        public new string this[int index]
        {
            get => (string)base[index];
            set => base[index] = value;
        }

        public new string[] ToArray() => (string[])ToArray(typeof(string));

        public int Add(string browsePath) => Add((object)browsePath);

        public void Insert(int index, string browsePath)
        {
            if (browsePath == null)
                throw new ArgumentNullException(nameof(browsePath));
            Insert(index, (object)browsePath);
        }

        public BrowsePathCollection()
        {
        }

        public BrowsePathCollection(ICollection browsePaths)
        {
            if (browsePaths == null)
                return;
            foreach (string browsePath in (IEnumerable)browsePaths)
                Add(browsePath);
        }
    }
}
