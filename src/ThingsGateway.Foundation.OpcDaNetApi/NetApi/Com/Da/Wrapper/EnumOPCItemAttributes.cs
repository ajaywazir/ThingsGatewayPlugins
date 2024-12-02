

using Opc.Da;

using OpcRcw.Da;

using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
    public class EnumOPCItemAttributes : IEnumOPCItemAttributes
    {
        private ArrayList m_items = new ArrayList();
        private int m_index;

        internal EnumOPCItemAttributes(ICollection items)
        {
            if (items == null)
                return;
            foreach (EnumOPCItemAttributes.ItemAttributes itemAttributes in (IEnumerable)items)
                m_items.Add((object)itemAttributes);
        }

        public void Skip(int celt)
        {
            lock (this)
            {
                try
                {
                    m_index += celt;
                    if (m_index <= m_items.Count)
                        return;
                    m_index = m_items.Count;
                }
                catch (Exception ex)
                {
                    throw Server.CreateException(ex);
                }
            }
        }

        public void Clone(out IEnumOPCItemAttributes ppEnumItemAttributes)
        {
            lock (this)
            {
                try
                {
                    ppEnumItemAttributes = (IEnumOPCItemAttributes)new EnumOPCItemAttributes((ICollection)m_items);
                }
                catch (Exception ex)
                {
                    throw Server.CreateException(ex);
                }
            }
        }

        public void Reset()
        {
            lock (this)
            {
                try
                {
                    m_index = 0;
                }
                catch (Exception ex)
                {
                    throw Server.CreateException(ex);
                }
            }
        }

        public void Next(int celt, out IntPtr ppItemArray, out int pceltFetched)
        {
            lock (this)
            {
                try
                {
                    pceltFetched = 0;
                    ppItemArray = IntPtr.Zero;
                    if (m_index >= m_items.Count)
                        return;
                    pceltFetched = m_items.Count - m_index;
                    if (pceltFetched > celt)
                        pceltFetched = celt;
                    ppItemArray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCITEMATTRIBUTES)) * pceltFetched);
                    IntPtr ptr = ppItemArray;
                    for (int index = 0; index < pceltFetched; ++index)
                    {
                        EnumOPCItemAttributes.ItemAttributes itemAttributes = (EnumOPCItemAttributes.ItemAttributes)m_items[m_index + index];
                        OPCITEMATTRIBUTES structure = new OPCITEMATTRIBUTES();
                        structure.szItemID = itemAttributes.ItemID;
                        structure.szAccessPath = itemAttributes.AccessPath;
                        structure.hClient = itemAttributes.ClientHandle;
                        structure.hServer = itemAttributes.ServerHandle;
                        structure.bActive = itemAttributes.Active ? 1 : 0;
                        structure.vtCanonicalDataType = (short)OpcCom.Interop.GetType(itemAttributes.CanonicalDataType);
                        structure.vtRequestedDataType = (short)OpcCom.Interop.GetType(itemAttributes.RequestedDataType);
                        structure.dwAccessRights = (int)OpcCom.Da.Interop.MarshalPropertyValue(Property.ACCESSRIGHTS, (object)itemAttributes.AccessRights);
                        structure.dwBlobSize = 0;
                        structure.pBlob = IntPtr.Zero;
                        structure.dwEUType = (OPCEUTYPE)OpcCom.Da.Interop.MarshalPropertyValue(Property.EUTYPE, (object)itemAttributes.EuType);
                        structure.vEUInfo = (object)null;
                        switch (itemAttributes.EuType)
                        {
                            case euType.analog:
                                structure.vEUInfo = (object)new double[2]
                                {
                  itemAttributes.MinValue,
                  itemAttributes.MaxValue
                                };
                                break;
                            case euType.enumerated:
                                structure.vEUInfo = (object)itemAttributes.EuInfo;
                                break;
                        }
                        Marshal.StructureToPtr<OPCITEMATTRIBUTES>(structure, ptr, false);
                        ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMATTRIBUTES)));
                    }
                    m_index += pceltFetched;
                }
                catch (Exception ex)
                {
                    throw Server.CreateException(ex);
                }
            }
        }

        public class ItemAttributes
        {
            public string ItemID;
            public string AccessPath;
            public int ClientHandle = -1;
            public int ServerHandle = -1;
            public bool Active;
            public Type CanonicalDataType;
            public Type RequestedDataType;
            public accessRights AccessRights = accessRights.readWritable;
            public euType EuType = euType.noEnum;
            public double MaxValue;
            public double MinValue;
            public string[] EuInfo;
        }
    }
}
