

using Opc;
using Opc.Hda;

using OpcRcw.Comn;
using OpcRcw.Hda;

using System;
using System.Collections;


namespace OpcCom.Hda
{
    public class Browser : IBrowser, IDisposable
    {
        private bool m_disposed;
        private Server m_server;
        private IOPCHDA_Browser m_browser;
        private BrowseFilterCollection m_filters = new BrowseFilterCollection();
        private const int BLOCK_SIZE = 10;

        internal Browser(
          Server server,
          IOPCHDA_Browser browser,
          BrowseFilter[] filters,
          ResultID[] results)
        {
            if (browser == null)
                throw new ArgumentNullException(nameof(browser));
            m_server = server;
            m_browser = browser;
            if (filters == null)
                return;
            ArrayList arrayList = new ArrayList();
            for (int index = 0; index < filters.Length; ++index)
            {
                if (results[index].Succeeded())
                    arrayList.Add((object)filters[index]);
            }
            m_filters = new BrowseFilterCollection((ICollection)arrayList);
        }

        ~Browser() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            int num = disposing ? 1 : 0;
            lock (this)
            {
                m_server = (Server)null;
                OpcCom.Interop.ReleaseServer((object)m_browser);
                m_browser = (IOPCHDA_Browser)null;
            }
            m_disposed = true;
        }

        public BrowseFilterCollection Filters
        {
            get
            {
                lock (this)
                    return (BrowseFilterCollection)m_filters.Clone();
            }
        }

        public BrowseElement[] Browse(ItemIdentifier itemID)
        {
            IBrowsePosition position = (IBrowsePosition)null;
            BrowseElement[] browseElementArray = Browse(itemID, 0, out position);
            if (position == null)
                return browseElementArray;
            position.Dispose();
            return browseElementArray;
        }

        public BrowseElement[] Browse(
          ItemIdentifier itemID,
          int maxElements,
          out IBrowsePosition position)
        {
            position = (IBrowsePosition)null;
            if (maxElements <= 0)
                maxElements = int.MaxValue;
            lock (this)
            {
                string itemName = itemID == null || itemID.ItemName == null ? "" : itemID.ItemName;
                try
                {
                    m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_DIRECT, itemName);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCHDA_Browser.ChangeBrowsePosition", ex);
                }
                EnumString enumerator1 = GetEnumerator(true);
                ArrayList arrayList = FetchElements(enumerator1, maxElements, true);
                if (arrayList.Count >= maxElements)
                {
                    position = (IBrowsePosition)new BrowsePosition(itemName, enumerator1, false);
                    return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
                }
                enumerator1.Dispose();
                EnumString enumerator2 = GetEnumerator(false);
                ArrayList c = FetchElements(enumerator2, maxElements - arrayList.Count, false);
                if (c != null)
                    arrayList.AddRange((ICollection)c);
                if (arrayList.Count >= maxElements)
                {
                    position = (IBrowsePosition)new BrowsePosition(itemName, enumerator2, true);
                    return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
                }
                enumerator2.Dispose();
                return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
            }
        }

        public BrowseElement[] BrowseNext(int maxElements, ref IBrowsePosition position)
        {
            if (position == null || position.GetType() != typeof(BrowsePosition))
                throw new ArgumentException("Not a valid browse position object.", nameof(position));
            if (maxElements <= 0)
                maxElements = int.MaxValue;
            lock (this)
            {
                BrowsePosition browsePosition = (BrowsePosition)position;
                ArrayList arrayList = new ArrayList();
                if (!browsePosition.FetchingItems)
                {
                    arrayList = FetchElements(browsePosition.Enumerator, maxElements, true);
                    if (arrayList.Count >= maxElements)
                        return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
                    browsePosition.Enumerator.Dispose();
                    browsePosition.Enumerator = (EnumString)null;
                    browsePosition.FetchingItems = true;
                    try
                    {
                        m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_DIRECT, browsePosition.BranchPath);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCHDA_Browser.ChangeBrowsePosition", ex);
                    }
                    browsePosition.Enumerator = GetEnumerator(false);
                }
                ArrayList c = FetchElements(browsePosition.Enumerator, maxElements - arrayList.Count, false);
                if (c != null)
                    arrayList.AddRange((ICollection)c);
                if (arrayList.Count >= maxElements)
                    return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
                position.Dispose();
                position = (IBrowsePosition)null;
                return (BrowseElement[])arrayList.ToArray(typeof(BrowseElement));
            }
        }

        private EnumString GetEnumerator(bool isBranch)
        {
            try
            {
                OPCHDA_BROWSETYPE dwBrowseType = isBranch ? OPCHDA_BROWSETYPE.OPCHDA_BRANCH : OPCHDA_BROWSETYPE.OPCHDA_LEAF;
                IEnumString ppIEnumString = (IEnumString)null;
                m_browser.GetEnum(dwBrowseType, out ppIEnumString);
                return new EnumString((object)ppIEnumString);
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCHDA_Browser.GetEnum", ex);
            }
        }

        private string GetFullBranchName(string name)
        {
            string pszBranchPos = (string)null;
            try
            {
                m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_DOWN, name);
            }
            catch
            {
                return (string)null;
            }
            try
            {
                m_browser.GetBranchPosition(out pszBranchPos);
            }
            catch
            {
            }
            m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_UP, "");
            return pszBranchPos;
        }

        private ArrayList FetchElements(EnumString enumerator, int maxElements, bool isBranch)
        {
            ArrayList arrayList = new ArrayList();
            while (arrayList.Count < maxElements)
            {
                int count = 10;
                if (arrayList.Count + count > maxElements)
                    count = maxElements - arrayList.Count;
                string[] strArray = enumerator.Next(count);
                if (strArray != null && strArray.Length != 0)
                {
                    foreach (string str in strArray)
                    {
                        BrowseElement browseElement = new BrowseElement();
                        browseElement.Name = str;
                        browseElement.ItemPath = (string)null;
                        browseElement.HasChildren = isBranch;
                        string pszItemID = (string)null;
                        try
                        {
                            if (isBranch)
                                pszItemID = GetFullBranchName(str);
                            else
                                m_browser.GetItemID(str, out pszItemID);
                        }
                        catch
                        {
                            pszItemID = (string)null;
                        }
                        browseElement.ItemName = pszItemID;
                        arrayList.Add((object)browseElement);
                    }
                }
                else
                    break;
            }
            IdentifiedResult[] identifiedResultArray = m_server.ValidateItems((ItemIdentifier[])arrayList.ToArray(typeof(ItemIdentifier)));
            if (identifiedResultArray != null)
            {
                for (int index = 0; index < identifiedResultArray.Length; ++index)
                {
                    if (identifiedResultArray[index].ResultID.Succeeded())
                        ((BrowseElement)arrayList[index]).IsItem = true;
                }
            }
            return arrayList;
        }
    }
}
