

using Opc;
using Opc.Da;

using OpcRcw.Da;

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;


namespace OpcCom.Da
{
    public class Interop
    {
        private static readonly char[] dotSeparator = new char[] { '.' };
        internal static OpcRcw.Da.FILETIME Convert(OpcCom.FILETIME input)
        {
            return new OpcRcw.Da.FILETIME()
            {
                dwLowDateTime = input.dwLowDateTime,
                dwHighDateTime = input.dwHighDateTime
            };
        }

        internal static OpcCom.FILETIME Convert(OpcRcw.Da.FILETIME input)
        {
            return new OpcCom.FILETIME()
            {
                dwLowDateTime = input.dwLowDateTime,
                dwHighDateTime = input.dwHighDateTime
            };
        }

        internal static OPCSERVERSTATUS GetServerStatus(ServerStatus input, int groupCount)
        {
            OPCSERVERSTATUS serverStatus = new OPCSERVERSTATUS();
            if (input != null)
            {
                serverStatus.szVendorInfo = input.VendorInfo;
                serverStatus.wMajorVersion = (short)0;
                serverStatus.wMinorVersion = (short)0;
                serverStatus.wBuildNumber = (short)0;
                serverStatus.dwServerState = (OPCSERVERSTATE)input.ServerState;
                serverStatus.ftStartTime = Interop.Convert(OpcCom.Interop.GetFILETIME(input.StartTime));
                serverStatus.ftCurrentTime = Interop.Convert(OpcCom.Interop.GetFILETIME(input.CurrentTime));
                serverStatus.ftLastUpdateTime = Interop.Convert(OpcCom.Interop.GetFILETIME(input.LastUpdateTime));
                serverStatus.dwBandWidth = -1;
                serverStatus.dwGroupCount = groupCount;
                serverStatus.wReserved = (short)0;
                if (input.ProductVersion != null)
                {
                    string[] strArray = input.ProductVersion.Split(dotSeparator);
                    if (strArray.Length != 0)
                    {
                        try
                        {
                            serverStatus.wMajorVersion = System.Convert.ToInt16(strArray[0]);
                        }
                        catch
                        {
                            serverStatus.wMajorVersion = (short)0;
                        }
                    }
                    if (strArray.Length > 1)
                    {
                        try
                        {
                            serverStatus.wMinorVersion = System.Convert.ToInt16(strArray[1]);
                        }
                        catch
                        {
                            serverStatus.wMinorVersion = (short)0;
                        }
                    }
                    serverStatus.wBuildNumber = (short)0;
                    for (int index = 2; index < strArray.Length; ++index)
                    {
                        try
                        {
                            serverStatus.wBuildNumber = (short)((int)serverStatus.wBuildNumber * 100 + (int)System.Convert.ToInt16(strArray[index]));
                        }
                        catch
                        {
                            serverStatus.wBuildNumber = (short)0;
                            break;
                        }
                    }
                }
            }
            return serverStatus;
        }

        internal static ServerStatus GetServerStatus(ref IntPtr pInput, bool deallocate)
        {
            ServerStatus serverStatus = (ServerStatus)null;
            if (pInput != IntPtr.Zero)
            {
                OPCSERVERSTATUS structure = (OPCSERVERSTATUS)Marshal.PtrToStructure(pInput, typeof(OPCSERVERSTATUS));
                serverStatus = new ServerStatus();
                serverStatus.VendorInfo = structure.szVendorInfo;
                serverStatus.ProductVersion = string.Format("{0}.{1}.{2}", (object)structure.wMajorVersion, (object)structure.wMinorVersion, (object)structure.wBuildNumber);
                serverStatus.ServerState = (serverState)structure.dwServerState;
                serverStatus.StatusInfo = (string)null;
                serverStatus.StartTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftStartTime));
                serverStatus.CurrentTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftCurrentTime));
                serverStatus.LastUpdateTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftLastUpdateTime));
                if (deallocate)
                {
                    Marshal.DestroyStructure(pInput, typeof(OPCSERVERSTATUS));
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }
            return serverStatus;
        }

        internal static OPCBROWSEFILTER GetBrowseFilter(browseFilter input)
        {
            switch (input)
            {
                case browseFilter.all:
                    return OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL;
                case browseFilter.branch:
                    return OPCBROWSEFILTER.OPC_BROWSE_FILTER_BRANCHES;
                case browseFilter.item:
                    return OPCBROWSEFILTER.OPC_BROWSE_FILTER_ITEMS;
                default:
                    return OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL;
            }
        }

        internal static browseFilter GetBrowseFilter(OPCBROWSEFILTER input)
        {
            switch (input)
            {
                case OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL:
                    return browseFilter.all;
                case OPCBROWSEFILTER.OPC_BROWSE_FILTER_BRANCHES:
                    return browseFilter.branch;
                case OPCBROWSEFILTER.OPC_BROWSE_FILTER_ITEMS:
                    return browseFilter.item;
                default:
                    return browseFilter.all;
            }
        }

        internal static IntPtr GetHRESULTs(IResult[] results)
        {
            int[] source = new int[results.Length];
            for (int index = 0; index < results.Length; ++index)
                source[index] = results[index] == null ? -1073479679 : OpcCom.Interop.GetResultID(results[index].ResultID);
            IntPtr destination = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * results.Length);
            Marshal.Copy(source, 0, destination, results.Length);
            return destination;
        }

        internal static BrowseElement[] GetBrowseElements(
          ref IntPtr pInput,
          int count,
          bool deallocate)
        {
            BrowseElement[] browseElements = (BrowseElement[])null;
            if (pInput != IntPtr.Zero && count > 0)
            {
                browseElements = new BrowseElement[count];
                IntPtr pInput1 = pInput;
                for (int index = 0; index < count; ++index)
                {
                    browseElements[index] = Interop.GetBrowseElement(pInput1, deallocate);
                    pInput1 = (nint)(pInput1.ToInt64() + (long)Marshal.SizeOf(typeof(OPCBROWSEELEMENT)));
                }
                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }
            return browseElements;
        }

        internal static IntPtr GetBrowseElements(BrowseElement[] input, bool propertiesRequested)
        {
            IntPtr browseElements = IntPtr.Zero;
            if (input != null && input.Length != 0)
            {
                browseElements = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCBROWSEELEMENT)) * input.Length);
                IntPtr ptr = browseElements;
                for (int index = 0; index < input.Length; ++index)
                {
                    Marshal.StructureToPtr<OPCBROWSEELEMENT>(Interop.GetBrowseElement(input[index], propertiesRequested), ptr, false);
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCBROWSEELEMENT)));
                }
            }
            return browseElements;
        }

        internal static BrowseElement GetBrowseElement(IntPtr pInput, bool deallocate)
        {
            BrowseElement browseElement = (BrowseElement)null;
            if (pInput != IntPtr.Zero)
            {
                OPCBROWSEELEMENT structure = (OPCBROWSEELEMENT)Marshal.PtrToStructure(pInput, typeof(OPCBROWSEELEMENT));
                browseElement = new BrowseElement();
                browseElement.Name = structure.szName;
                browseElement.ItemPath = (string)null;
                browseElement.ItemName = structure.szItemID;
                browseElement.IsItem = (structure.dwFlagValue & 2) != 0;
                browseElement.HasChildren = (structure.dwFlagValue & 1) != 0;
                browseElement.Properties = Interop.GetItemProperties(ref structure.ItemProperties, deallocate);
                if (deallocate)
                    Marshal.DestroyStructure(pInput, typeof(OPCBROWSEELEMENT));
            }
            return browseElement;
        }

        internal static OPCBROWSEELEMENT GetBrowseElement(BrowseElement input, bool propertiesRequested)
        {
            OPCBROWSEELEMENT browseElement = new OPCBROWSEELEMENT();
            if (input != null)
            {
                browseElement.szName = input.Name;
                browseElement.szItemID = input.ItemName;
                browseElement.dwFlagValue = 0;
                browseElement.ItemProperties = Interop.GetItemProperties(input.Properties);
                if (input.IsItem)
                    browseElement.dwFlagValue |= 2;
                if (input.HasChildren)
                    browseElement.dwFlagValue |= 1;
            }
            return browseElement;
        }

        internal static int[] GetPropertyIDs(PropertyID[] propertyIDs)
        {
            ArrayList arrayList = new ArrayList();
            if (propertyIDs != null)
            {
                foreach (PropertyID propertyId in propertyIDs)
                    arrayList.Add((object)propertyId.Code);
            }
            return (int[])arrayList.ToArray(typeof(int));
        }

        internal static PropertyID[] GetPropertyIDs(int[] propertyIDs)
        {
            ArrayList arrayList = new ArrayList();
            if (propertyIDs != null)
            {
                foreach (int propertyId in propertyIDs)
                    arrayList.Add((object)Interop.GetPropertyID(propertyId));
            }
            return (PropertyID[])arrayList.ToArray(typeof(PropertyID));
        }

        internal static ItemPropertyCollection[] GetItemPropertyCollections(
          ref IntPtr pInput,
          int count,
          bool deallocate)
        {
            ItemPropertyCollection[] propertyCollections = (ItemPropertyCollection[])null;
            if (pInput != IntPtr.Zero && count > 0)
            {
                propertyCollections = new ItemPropertyCollection[count];
                IntPtr ptr = pInput;
                for (int index = 0; index < count; ++index)
                {
                    OPCITEMPROPERTIES structure = (OPCITEMPROPERTIES)Marshal.PtrToStructure(ptr, typeof(OPCITEMPROPERTIES));
                    propertyCollections[index] = new ItemPropertyCollection();
                    propertyCollections[index].ItemPath = (string)null;
                    propertyCollections[index].ItemName = (string)null;
                    propertyCollections[index].ResultID = OpcCom.Interop.GetResultID(structure.hrErrorID);
                    ItemProperty[] itemProperties = Interop.GetItemProperties(ref structure, deallocate);
                    if (itemProperties != null)
                        propertyCollections[index].AddRange((ICollection)itemProperties);
                    if (deallocate)
                        Marshal.DestroyStructure(ptr, typeof(OPCITEMPROPERTIES));
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMPROPERTIES)));
                }
                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }
            return propertyCollections;
        }

        internal static IntPtr GetItemPropertyCollections(ItemPropertyCollection[] input)
        {
            IntPtr propertyCollections = IntPtr.Zero;
            if (input != null && input.Length != 0)
            {
                propertyCollections = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCITEMPROPERTIES)) * input.Length);
                IntPtr ptr = propertyCollections;
                for (int index = 0; index < input.Length; ++index)
                {
                    OPCITEMPROPERTIES structure = new OPCITEMPROPERTIES();
                    if (input[index].Count > 0)
                        structure = Interop.GetItemProperties((ItemProperty[])input[index].ToArray(typeof(ItemProperty)));
                    structure.hrErrorID = OpcCom.Interop.GetResultID(input[index].ResultID);
                    Marshal.StructureToPtr<OPCITEMPROPERTIES>(structure, ptr, false);
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMPROPERTIES)));
                }
            }
            return propertyCollections;
        }

        internal static ItemProperty[] GetItemProperties(ref OPCITEMPROPERTIES input, bool deallocate)
        {
            ItemProperty[] itemProperties = (ItemProperty[])null;
            if (input.dwNumProperties > 0)
            {
                itemProperties = new ItemProperty[input.dwNumProperties];
                IntPtr pInput = input.pItemProperties;
                for (int index = 0; index < itemProperties.Length; ++index)
                {
                    try
                    {
                        itemProperties[index] = Interop.GetItemProperty(pInput, deallocate);
                    }
                    catch (Exception ex)
                    {
                        itemProperties[index] = new ItemProperty();
                        itemProperties[index].Description = ex.Message;
                        itemProperties[index].ResultID = ResultID.E_FAIL;
                    }
                    pInput = (nint)(pInput.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMPROPERTY)));
                }
                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(input.pItemProperties);
                    input.pItemProperties = IntPtr.Zero;
                }
            }
            return itemProperties;
        }

        internal static OPCITEMPROPERTIES GetItemProperties(ItemProperty[] input)
        {
            OPCITEMPROPERTIES itemProperties = new OPCITEMPROPERTIES();
            if (input != null && input.Length != 0)
            {
                itemProperties.hrErrorID = 0;
                itemProperties.dwReserved = 0;
                itemProperties.dwNumProperties = input.Length;
                itemProperties.pItemProperties = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCITEMPROPERTY)) * input.Length);
                bool flag = false;
                IntPtr ptr = itemProperties.pItemProperties;
                for (int index = 0; index < input.Length; ++index)
                {
                    Marshal.StructureToPtr<OPCITEMPROPERTY>(Interop.GetItemProperty(input[index]), ptr, false);
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMPROPERTY)));
                    if (input[index].ResultID.Failed())
                        flag = true;
                }
                if (flag)
                    itemProperties.hrErrorID = 1;
            }
            return itemProperties;
        }

        internal static ItemProperty GetItemProperty(IntPtr pInput, bool deallocate)
        {
            ItemProperty itemProperty = (ItemProperty)null;
            if (pInput != IntPtr.Zero)
            {
                OPCITEMPROPERTY structure = (OPCITEMPROPERTY)Marshal.PtrToStructure(pInput, typeof(OPCITEMPROPERTY));
                itemProperty = new ItemProperty()
                {
                    ID = Interop.GetPropertyID(structure.dwPropertyID),
                    Description = structure.szDescription,
                    DataType = OpcCom.Interop.GetType((VarEnum)structure.vtDataType),
                    ItemPath = (string)null,
                    ItemName = structure.szItemID
                };
                itemProperty.Value = Interop.UnmarshalPropertyValue(itemProperty.ID, structure.vValue);
                itemProperty.ResultID = OpcCom.Interop.GetResultID(structure.hrErrorID);
                if (structure.hrErrorID == -1073479674)
                    itemProperty.ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
                if (deallocate)
                    Marshal.DestroyStructure(pInput, typeof(OPCITEMPROPERTY));
            }
            return itemProperty;
        }

        internal static OPCITEMPROPERTY GetItemProperty(ItemProperty input)
        {
            OPCITEMPROPERTY itemProperty = new OPCITEMPROPERTY();
            if (input != null)
            {
                itemProperty.dwPropertyID = input.ID.Code;
                itemProperty.szDescription = input.Description;
                itemProperty.vtDataType = (short)OpcCom.Interop.GetType(input.DataType);
                itemProperty.vValue = Interop.MarshalPropertyValue(input.ID, input.Value);
                itemProperty.wReserved = (short)0;
                itemProperty.hrErrorID = OpcCom.Interop.GetResultID(input.ResultID);
                PropertyDescription propertyDescription = PropertyDescription.Find(input.ID);
                if (propertyDescription != null)
                    itemProperty.vtDataType = (short)OpcCom.Interop.GetType(propertyDescription.Type);
                if (input.ResultID == ResultID.Da.E_WRITEONLY)
                    itemProperty.hrErrorID = -1073479674;
            }
            return itemProperty;
        }

        public static PropertyID GetPropertyID(int input)
        {
            foreach (FieldInfo field in typeof(Property).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                PropertyID propertyId = (PropertyID)field.GetValue((object)typeof(PropertyID));
                if (input == propertyId.Code)
                    return propertyId;
            }
            return new PropertyID(input);
        }

        internal static object UnmarshalPropertyValue(PropertyID propertyID, object input)
        {
            if (input == null)
                return (object)null;
            try
            {
                if (propertyID == Property.DATATYPE)
                    return (object)OpcCom.Interop.GetType((VarEnum)System.Convert.ToUInt16(input));
                if (propertyID == Property.ACCESSRIGHTS)
                {
                    switch (System.Convert.ToInt32(input))
                    {
                        case 1:
                            return (object)accessRights.readable;
                        case 2:
                            return (object)accessRights.writable;
                        case 3:
                            return (object)accessRights.readWritable;
                        default:
                            return (object)null;
                    }
                }
                else if (propertyID == Property.EUTYPE)
                {
                    switch ((OPCEUTYPE)input)
                    {
                        case OPCEUTYPE.OPC_NOENUM:
                            return (object)euType.noEnum;
                        case OPCEUTYPE.OPC_ANALOG:
                            return (object)euType.analog;
                        case OPCEUTYPE.OPC_ENUMERATED:
                            return (object)euType.enumerated;
                        default:
                            return (object)null;
                    }
                }
                else
                {
                    if (propertyID == Property.QUALITY)
                        return (object)new Quality(System.Convert.ToInt16(input));
                    if (propertyID == Property.TIMESTAMP)
                    {
                        if (input.GetType() == typeof(DateTime))
                        {
                            DateTime dateTime = (DateTime)input;
                            return dateTime != DateTime.MinValue ? (object)dateTime.ToLocalTime() : (object)dateTime;
                        }
                    }
                }
            }
            catch
            {
            }
            return input;
        }

        internal static object MarshalPropertyValue(PropertyID propertyID, object input)
        {
            if (input == null)
                return (object)null;
            try
            {
                if (propertyID == Property.DATATYPE)
                    return (object)(short)OpcCom.Interop.GetType((System.Type)input);
                if (propertyID == Property.ACCESSRIGHTS)
                {
                    switch ((accessRights)input)
                    {
                        case accessRights.readable:
                            return (object)1;
                        case accessRights.writable:
                            return (object)2;
                        case accessRights.readWritable:
                            return (object)3;
                        default:
                            return (object)null;
                    }
                }
                else if (propertyID == Property.EUTYPE)
                {
                    switch ((euType)input)
                    {
                        case euType.noEnum:
                            return (object)OPCEUTYPE.OPC_NOENUM;
                        case euType.analog:
                            return (object)OPCEUTYPE.OPC_ANALOG;
                        case euType.enumerated:
                            return (object)OPCEUTYPE.OPC_ENUMERATED;
                        default:
                            return (object)null;
                    }
                }
                else
                {
                    if (propertyID == Property.QUALITY)
                        return (object)((Quality)input).GetCode();
                    if (propertyID == Property.TIMESTAMP)
                    {
                        if (input.GetType() == typeof(DateTime))
                        {
                            DateTime dateTime = (DateTime)input;
                            return dateTime != DateTime.MinValue ? (object)dateTime.ToUniversalTime() : (object)dateTime;
                        }
                    }
                }
            }
            catch
            {
            }
            return input;
        }

        internal static OPCITEMVQT[] GetOPCITEMVQTs(ItemValue[] input)
        {
            OPCITEMVQT[] opcitemvqTs = (OPCITEMVQT[])null;
            if (input != null)
            {
                opcitemvqTs = new OPCITEMVQT[input.Length];
                for (int index = 0; index < input.Length; ++index)
                {
                    opcitemvqTs[index] = new OPCITEMVQT();
                    DateTime datetime = input[index].TimestampSpecified ? input[index].Timestamp : DateTime.MinValue;
                    opcitemvqTs[index].vDataValue = OpcCom.Interop.GetVARIANT(input[index].Value);
                    opcitemvqTs[index].bQualitySpecified = input[index].QualitySpecified ? 1 : 0;
                    opcitemvqTs[index].wQuality = input[index].QualitySpecified ? input[index].Quality.GetCode() : (short)0;
                    opcitemvqTs[index].bTimeStampSpecified = input[index].TimestampSpecified ? 1 : 0;
                    opcitemvqTs[index].ftTimeStamp = Interop.Convert(OpcCom.Interop.GetFILETIME(datetime));
                }
            }
            return opcitemvqTs;
        }

        internal static OPCITEMDEF[] GetOPCITEMDEFs(Item[] input)
        {
            OPCITEMDEF[] opcitemdeFs = (OPCITEMDEF[])null;
            if (input != null)
            {
                opcitemdeFs = new OPCITEMDEF[input.Length];
                for (int index = 0; index < input.Length; ++index)
                {
                    opcitemdeFs[index] = new OPCITEMDEF();
                    opcitemdeFs[index].szItemID = input[index].ItemName;
                    opcitemdeFs[index].szAccessPath = input[index].ItemPath == null ? string.Empty : input[index].ItemPath;
                    opcitemdeFs[index].bActive = input[index].ActiveSpecified ? (input[index].Active ? 1 : 0) : 1;
                    opcitemdeFs[index].vtRequestedDataType = (short)OpcCom.Interop.GetType(input[index].ReqType);
                    opcitemdeFs[index].hClient = 0;
                    opcitemdeFs[index].dwBlobSize = 0;
                    opcitemdeFs[index].pBlob = IntPtr.Zero;
                }
            }
            return opcitemdeFs;
        }

        internal static ItemValue[] GetItemValues(ref IntPtr pInput, int count, bool deallocate)
        {
            ItemValue[] itemValues = (ItemValue[])null;
            if (pInput != IntPtr.Zero && count > 0)
            {
                itemValues = new ItemValue[count];
                IntPtr ptr = pInput;
                for (int index = 0; index < count; ++index)
                {
                    OPCITEMSTATE structure = (OPCITEMSTATE)Marshal.PtrToStructure(ptr, typeof(OPCITEMSTATE));
                    itemValues[index] = new ItemValue();
                    itemValues[index].ClientHandle = (object)structure.hClient;
                    itemValues[index].Value = structure.vDataValue;
                    itemValues[index].Quality = new Quality(structure.wQuality);
                    itemValues[index].QualitySpecified = true;
                    itemValues[index].Timestamp = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftTimeStamp));
                    itemValues[index].TimestampSpecified = itemValues[index].Timestamp != DateTime.MinValue;
                    if (deallocate)
                        Marshal.DestroyStructure(ptr, typeof(OPCITEMSTATE));
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMSTATE)));
                }
                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }
            return itemValues;
        }

        internal static int[] GetItemResults(ref IntPtr pInput, int count, bool deallocate)
        {
            int[] itemResults = (int[])null;
            if (pInput != IntPtr.Zero && count > 0)
            {
                itemResults = new int[count];
                IntPtr ptr = pInput;
                for (int index = 0; index < count; ++index)
                {
                    OPCITEMRESULT structure = (OPCITEMRESULT)Marshal.PtrToStructure(ptr, typeof(OPCITEMRESULT));
                    itemResults[index] = structure.hServer;
                    if (deallocate)
                    {
                        Marshal.FreeCoTaskMem(structure.pBlob);
                        structure.pBlob = IntPtr.Zero;
                        Marshal.DestroyStructure(ptr, typeof(OPCITEMRESULT));
                    }
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMRESULT)));
                }
                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }
            return itemResults;
        }

        internal static IntPtr GetItemStates(ItemValueResult[] input)
        {
            IntPtr itemStates = IntPtr.Zero;
            if (input != null && input.Length != 0)
            {
                itemStates = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCITEMSTATE)) * input.Length);
                IntPtr ptr = itemStates;
                for (int index = 0; index < input.Length; ++index)
                {
                    Marshal.StructureToPtr<OPCITEMSTATE>(new OPCITEMSTATE()
                    {
                        hClient = System.Convert.ToInt32(input[index].ClientHandle),
                        vDataValue = input[index].Value,
                        wQuality = input[index].QualitySpecified ? input[index].Quality.GetCode() : (short)0,
                        ftTimeStamp = Interop.Convert(OpcCom.Interop.GetFILETIME(input[index].Timestamp)),
                        wReserved = (short)0
                    }, ptr, false);
                    ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(OPCITEMSTATE)));
                }
            }
            return itemStates;
        }
    }
}
