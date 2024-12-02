

using Opc;
using Opc.Ae;

using OpcRcw.Ae;

using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Ae
{
    [Serializable]
    public class Server : OpcCom.Server, Opc.Ae.IServer, Opc.IServer, IDisposable
    {
        private bool m_disposed;
        private bool m_supportsAE11 = true;
        private object m_browser;
        private int m_handles = 1;
        private Hashtable m_subscriptions = new Hashtable();

        public Server(URL url, object server)
          : base(url, server)
        {
            m_supportsAE11 = true;
            try
            {
            }
            catch
            {
                m_supportsAE11 = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                lock (this)
                {
                    if (disposing && m_server != null)
                    {
                        foreach (Subscription subscription in (IEnumerable)m_subscriptions.Values)
                            subscription.Dispose();
                        m_subscriptions.Clear();
                    }
                    if (m_browser != null)
                    {
                        OpcCom.Interop.ReleaseServer(m_browser);
                        m_browser = (object)null;
                    }
                    if (m_server != null)
                    {
                        OpcCom.Interop.ReleaseServer(m_server);
                        m_server = (object)null;
                    }
                }
                m_disposed = true;
            }
            base.Dispose(disposing);
        }

        public ServerStatus GetStatus()
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                IntPtr ppEventServerStatus = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).GetStatus(out ppEventServerStatus);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.GetStatus", ex);
                }
                return Interop.GetServerStatus(ref ppEventServerStatus, true);
            }
        }

        public ISubscription CreateSubscription(SubscriptionState state)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (state == null)
                    throw new ArgumentNullException(nameof(state));
                object ppUnk = (object)null;
                Guid guid = typeof(IOPCEventSubscriptionMgt).GUID;
                int pdwRevisedBufferTime = 0;
                int pdwRevisedMaxSize = 0;
                try
                {
                    ((IOPCEventServer)m_server).CreateEventSubscription(state.Active ? 1 : 0, state.BufferTime, state.MaxSize, ++m_handles, ref guid, out ppUnk, out pdwRevisedBufferTime, out pdwRevisedMaxSize);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.CreateEventSubscription", ex);
                }
                state.BufferTime = pdwRevisedBufferTime;
                state.MaxSize = pdwRevisedMaxSize;
                Subscription subscription = new Subscription(state, ppUnk);
                subscription.ModifyState(32, state);
                m_subscriptions.Add((object)m_handles, (object)subscription);
                return (ISubscription)subscription;
            }
        }

        public int QueryAvailableFilters()
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwFilterMask = 0;
                try
                {
                    ((IOPCEventServer)m_server).QueryAvailableFilters(out pdwFilterMask);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QueryAvailableFilters", ex);
                }
                return pdwFilterMask;
            }
        }

        public Category[] QueryEventCategories(int eventType)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppdwEventCategories = IntPtr.Zero;
                IntPtr ppszEventCategoryDescs = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).QueryEventCategories(eventType, out pdwCount, out ppdwEventCategories, out ppszEventCategoryDescs);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QueryEventCategories", ex);
                }
                if (pdwCount == 0)
                    return Array.Empty<Category>();
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppdwEventCategories, pdwCount, true);
                string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref ppszEventCategoryDescs, pdwCount, true);
                Category[] categoryArray = new Category[pdwCount];
                for (int index = 0; index < pdwCount; ++index)
                {
                    categoryArray[index] = new Category();
                    categoryArray[index].ID = int32s[index];
                    categoryArray[index].Name = unicodeStrings[index];
                }
                return categoryArray;
            }
        }

        public string[] QueryConditionNames(int eventCategory)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppszConditionNames = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).QueryConditionNames(eventCategory, out pdwCount, out ppszConditionNames);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QueryConditionNames", ex);
                }
                if (pdwCount == 0)
                    return Array.Empty<string>();
                return OpcCom.Interop.GetUnicodeStrings(ref ppszConditionNames, pdwCount, true);
            }
        }

        public string[] QuerySubConditionNames(string conditionName)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppszSubConditionNames = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).QuerySubConditionNames(conditionName, out pdwCount, out ppszSubConditionNames);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QuerySubConditionNames", ex);
                }
                if (pdwCount == 0)
                    return Array.Empty<string>();
                return OpcCom.Interop.GetUnicodeStrings(ref ppszSubConditionNames, pdwCount, true);
            }
        }

        public string[] QueryConditionNames(string sourceName)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppszConditionNames = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).QuerySourceConditions(sourceName, out pdwCount, out ppszConditionNames);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QuerySourceConditions", ex);
                }
                if (pdwCount == 0)
                    return Array.Empty<string>();
                return OpcCom.Interop.GetUnicodeStrings(ref ppszConditionNames, pdwCount, true);
            }
        }

        public Opc.Ae.Attribute[] QueryEventAttributes(int eventCategory)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                int pdwCount = 0;
                IntPtr ppdwAttrIDs = IntPtr.Zero;
                IntPtr ppszAttrDescs = IntPtr.Zero;
                IntPtr ppvtAttrTypes = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).QueryEventAttributes(eventCategory, out pdwCount, out ppdwAttrIDs, out ppszAttrDescs, out ppvtAttrTypes);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.QueryEventAttributes", ex);
                }
                if (pdwCount == 0)
                    return Array.Empty<Opc.Ae.Attribute>();
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppdwAttrIDs, pdwCount, true);
                string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref ppszAttrDescs, pdwCount, true);
                short[] int16s = OpcCom.Interop.GetInt16s(ref ppvtAttrTypes, pdwCount, true);
                Opc.Ae.Attribute[] attributeArray = new Opc.Ae.Attribute[pdwCount];
                for (int index = 0; index < pdwCount; ++index)
                {
                    attributeArray[index] = new Opc.Ae.Attribute();
                    attributeArray[index].ID = int32s[index];
                    attributeArray[index].Name = unicodeStrings[index];
                    attributeArray[index].DataType = OpcCom.Interop.GetType((VarEnum)int16s[index]);
                }
                return attributeArray;
            }
        }

        public ItemUrl[] TranslateToItemIDs(
          string sourceName,
          int eventCategory,
          string conditionName,
          string subConditionName,
          int[] attributeIDs)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                IntPtr ppszAttrItemIDs = IntPtr.Zero;
                IntPtr ppszNodeNames = IntPtr.Zero;
                IntPtr ppCLSIDs = IntPtr.Zero;
                int length = attributeIDs != null ? attributeIDs.Length : 0;
                try
                {
                    ((IOPCEventServer)m_server).TranslateToItemIDs(sourceName != null ? sourceName : "", eventCategory, conditionName != null ? conditionName : "", subConditionName != null ? subConditionName : "", length, length > 0 ? attributeIDs : Array.Empty<int>(), out ppszAttrItemIDs, out ppszNodeNames, out ppCLSIDs);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.TranslateToItemIDs", ex);
                }
                string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref ppszAttrItemIDs, length, true);
                string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref ppszNodeNames, length, true);
                Guid[] guiDs = OpcCom.Interop.GetGUIDs(ref ppCLSIDs, length, true);
                ItemUrl[] itemIds = new ItemUrl[length];
                for (int index = 0; index < length; ++index)
                {
                    itemIds[index] = new ItemUrl();
                    itemIds[index].ItemName = unicodeStrings1[index];
                    itemIds[index].ItemPath = (string)null;
                    itemIds[index].Url.Scheme = "opcda";
                    itemIds[index].Url.HostName = unicodeStrings2[index];
                    itemIds[index].Url.Path = string.Format("{{{0}}}", (object)guiDs[index]);
                }
                return itemIds;
            }
        }

        public Condition GetConditionState(string sourceName, string conditionName, int[] attributeIDs)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                IntPtr ppConditionState = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).GetConditionState(sourceName != null ? sourceName : "", conditionName != null ? conditionName : "", attributeIDs != null ? attributeIDs.Length : 0, attributeIDs != null ? attributeIDs : Array.Empty<int>(), out ppConditionState);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.GetConditionState", ex);
                }
                Condition[] conditions = Interop.GetConditions(ref ppConditionState, 1, true);
                for (int index = 0; index < conditions[0].Attributes.Count; ++index)
                    conditions[0].Attributes[index].ID = attributeIDs[index];
                return conditions[0];
            }
        }

        public ResultID[] EnableConditionByArea(string[] areas)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (areas == null || areas.Length == 0)
                    return Array.Empty<ResultID>();
                IntPtr ppErrors = IntPtr.Zero;
                int[] numArray;
                if (m_supportsAE11)
                {
                    try
                    {
                        ((IOPCEventServer2)m_server).EnableConditionByArea2(areas.Length, areas, out ppErrors);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer2.EnableConditionByArea2", ex);
                    }
                    numArray = OpcCom.Interop.GetInt32s(ref ppErrors, areas.Length, true);
                }
                else
                {
                    try
                    {
                        ((IOPCEventServer)m_server).EnableConditionByArea(areas.Length, areas);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer.EnableConditionByArea", ex);
                    }
                    numArray = new int[areas.Length];
                }
                ResultID[] resultIdArray = new ResultID[numArray.Length];
                for (int index = 0; index < numArray.Length; ++index)
                    resultIdArray[index] = Interop.GetResultID(numArray[index]);
                return resultIdArray;
            }
        }

        public ResultID[] DisableConditionByArea(string[] areas)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (areas == null || areas.Length == 0)
                    return Array.Empty<ResultID>();
                IntPtr ppErrors = IntPtr.Zero;
                int[] numArray;
                if (m_supportsAE11)
                {
                    try
                    {
                        ((IOPCEventServer2)m_server).DisableConditionByArea2(areas.Length, areas, out ppErrors);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer2.DisableConditionByArea2", ex);
                    }
                    numArray = OpcCom.Interop.GetInt32s(ref ppErrors, areas.Length, true);
                }
                else
                {
                    try
                    {
                        ((IOPCEventServer)m_server).DisableConditionByArea(areas.Length, areas);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer.DisableConditionByArea", ex);
                    }
                    numArray = new int[areas.Length];
                }
                ResultID[] resultIdArray = new ResultID[numArray.Length];
                for (int index = 0; index < numArray.Length; ++index)
                    resultIdArray[index] = Interop.GetResultID(numArray[index]);
                return resultIdArray;
            }
        }

        public ResultID[] EnableConditionBySource(string[] sources)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (sources == null || sources.Length == 0)
                    return Array.Empty<ResultID>();
                IntPtr ppErrors = IntPtr.Zero;
                int[] numArray;
                if (m_supportsAE11)
                {
                    try
                    {
                        ((IOPCEventServer2)m_server).EnableConditionBySource2(sources.Length, sources, out ppErrors);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer2.EnableConditionBySource2", ex);
                    }
                    numArray = OpcCom.Interop.GetInt32s(ref ppErrors, sources.Length, true);
                }
                else
                {
                    try
                    {
                        ((IOPCEventServer)m_server).EnableConditionBySource(sources.Length, sources);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer.EnableConditionBySource", ex);
                    }
                    numArray = new int[sources.Length];
                }
                ResultID[] resultIdArray = new ResultID[numArray.Length];
                for (int index = 0; index < numArray.Length; ++index)
                    resultIdArray[index] = Interop.GetResultID(numArray[index]);
                return resultIdArray;
            }
        }

        public ResultID[] DisableConditionBySource(string[] sources)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (sources == null || sources.Length == 0)
                    return Array.Empty<ResultID>();
                IntPtr ppErrors = IntPtr.Zero;
                int[] numArray;
                if (m_supportsAE11)
                {
                    try
                    {
                        ((IOPCEventServer2)m_server).DisableConditionBySource2(sources.Length, sources, out ppErrors);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer2.DisableConditionBySource2", ex);
                    }
                    numArray = OpcCom.Interop.GetInt32s(ref ppErrors, sources.Length, true);
                }
                else
                {
                    try
                    {
                        ((IOPCEventServer)m_server).DisableConditionBySource(sources.Length, sources);
                    }
                    catch (Exception ex)
                    {
                        throw OpcCom.Interop.CreateException("IOPCEventServer.DisableConditionBySource", ex);
                    }
                    numArray = new int[sources.Length];
                }
                ResultID[] resultIdArray = new ResultID[numArray.Length];
                for (int index = 0; index < numArray.Length; ++index)
                    resultIdArray[index] = Interop.GetResultID(numArray[index]);
                return resultIdArray;
            }
        }

        public EnabledStateResult[] GetEnableStateByArea(string[] areas)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (areas == null || areas.Length == 0)
                    return Array.Empty<EnabledStateResult>();
                if (!m_supportsAE11)
                {
                    EnabledStateResult[] enableStateByArea = new EnabledStateResult[areas.Length];
                    for (int index = 0; index < enableStateByArea.Length; ++index)
                    {
                        enableStateByArea[index] = new EnabledStateResult();
                        enableStateByArea[index].Enabled = false;
                        enableStateByArea[index].EffectivelyEnabled = false;
                        enableStateByArea[index].ResultID = ResultID.E_FAIL;
                    }
                    return enableStateByArea;
                }
                IntPtr pbEnabled = IntPtr.Zero;
                IntPtr pbEffectivelyEnabled = IntPtr.Zero;
                IntPtr ppErrors = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer2)m_server).GetEnableStateByArea(areas.Length, areas, out pbEnabled, out pbEffectivelyEnabled, out ppErrors);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer2.GetEnableStateByArea", ex);
                }
                int[] int32s1 = OpcCom.Interop.GetInt32s(ref pbEnabled, areas.Length, true);
                int[] int32s2 = OpcCom.Interop.GetInt32s(ref pbEffectivelyEnabled, areas.Length, true);
                int[] int32s3 = OpcCom.Interop.GetInt32s(ref ppErrors, areas.Length, true);
                EnabledStateResult[] enableStateByArea1 = new EnabledStateResult[int32s3.Length];
                for (int index = 0; index < int32s3.Length; ++index)
                {
                    enableStateByArea1[index] = new EnabledStateResult();
                    enableStateByArea1[index].Enabled = int32s1[index] != 0;
                    enableStateByArea1[index].EffectivelyEnabled = int32s2[index] != 0;
                    enableStateByArea1[index].ResultID = Interop.GetResultID(int32s3[index]);
                }
                return enableStateByArea1;
            }
        }

        public EnabledStateResult[] GetEnableStateBySource(string[] sources)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (sources == null || sources.Length == 0)
                    return Array.Empty<EnabledStateResult>();
                if (!m_supportsAE11)
                {
                    EnabledStateResult[] enableStateBySource = new EnabledStateResult[sources.Length];
                    for (int index = 0; index < enableStateBySource.Length; ++index)
                    {
                        enableStateBySource[index] = new EnabledStateResult();
                        enableStateBySource[index].Enabled = false;
                        enableStateBySource[index].EffectivelyEnabled = false;
                        enableStateBySource[index].ResultID = ResultID.E_FAIL;
                    }
                    return enableStateBySource;
                }
                IntPtr pbEnabled = IntPtr.Zero;
                IntPtr pbEffectivelyEnabled = IntPtr.Zero;
                IntPtr ppErrors = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer2)m_server).GetEnableStateBySource(sources.Length, sources, out pbEnabled, out pbEffectivelyEnabled, out ppErrors);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer2.GetEnableStateBySource", ex);
                }
                int[] int32s1 = OpcCom.Interop.GetInt32s(ref pbEnabled, sources.Length, true);
                int[] int32s2 = OpcCom.Interop.GetInt32s(ref pbEffectivelyEnabled, sources.Length, true);
                int[] int32s3 = OpcCom.Interop.GetInt32s(ref ppErrors, sources.Length, true);
                EnabledStateResult[] enableStateBySource1 = new EnabledStateResult[int32s3.Length];
                for (int index = 0; index < int32s3.Length; ++index)
                {
                    enableStateBySource1[index] = new EnabledStateResult();
                    enableStateBySource1[index].Enabled = int32s1[index] != 0;
                    enableStateBySource1[index].EffectivelyEnabled = int32s2[index] != 0;
                    enableStateBySource1[index].ResultID = Interop.GetResultID(int32s3[index]);
                }
                return enableStateBySource1;
            }
        }

        public ResultID[] AcknowledgeCondition(
          string acknowledgerID,
          string comment,
          EventAcknowledgement[] conditions)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (conditions == null || conditions.Length == 0)
                    return Array.Empty<ResultID>();
                int length = conditions.Length;
                string[] pszSource = new string[length];
                string[] szConditionName = new string[length];
                OpcRcw.Ae.FILETIME[] pftActiveTime = new OpcRcw.Ae.FILETIME[length];
                int[] pdwCookie = new int[length];
                for (int index = 0; index < length; ++index)
                {
                    pszSource[index] = conditions[index].SourceName;
                    szConditionName[index] = conditions[index].ConditionName;
                    pftActiveTime[index] = Interop.Convert(OpcCom.Interop.GetFILETIME(conditions[index].ActiveTime));
                    pdwCookie[index] = conditions[index].Cookie;
                }
                IntPtr ppErrors = IntPtr.Zero;
                try
                {
                    ((IOPCEventServer)m_server).AckCondition(conditions.Length, acknowledgerID, comment, pszSource, szConditionName, pftActiveTime, pdwCookie, out ppErrors);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventServer.AckCondition", ex);
                }
                int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, length, true);
                ResultID[] resultIdArray = new ResultID[length];
                for (int index = 0; index < length; ++index)
                    resultIdArray[index] = Interop.GetResultID(int32s[index]);
                return resultIdArray;
            }
        }

        public BrowseElement[] Browse(string areaID, BrowseType browseType, string browseFilter)
        {
            lock (this)
            {
                IBrowsePosition position = (IBrowsePosition)null;
                BrowseElement[] browseElementArray = Browse(areaID, browseType, browseFilter, 0, out position);
                position?.Dispose();
                return browseElementArray;
            }
        }

        public BrowseElement[] Browse(
          string areaID,
          BrowseType browseType,
          string browseFilter,
          int maxElements,
          out IBrowsePosition position)
        {
            position = (IBrowsePosition)null;
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                InitializeBrowser();
                ChangeBrowsePosition(areaID);
                System.Runtime.InteropServices.ComTypes.IEnumString enumerator = (System.Runtime.InteropServices.ComTypes.IEnumString)CreateEnumerator(browseType, browseFilter);
                ArrayList elements = new ArrayList();
                if (FetchElements(browseType, maxElements, enumerator, elements) != 0)
                    OpcCom.Interop.ReleaseServer((object)enumerator);
                else
                    position = (IBrowsePosition)new BrowsePosition(areaID, browseType, browseFilter, enumerator);
                return (BrowseElement[])elements.ToArray(typeof(BrowseElement));
            }
        }

        public BrowseElement[] BrowseNext(int maxElements, ref IBrowsePosition position)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                if (position == null)
                    throw new ArgumentNullException(nameof(position));
                InitializeBrowser();
                ChangeBrowsePosition(((Opc.Ae.BrowsePosition)position).AreaID);
                System.Runtime.InteropServices.ComTypes.IEnumString enumerator = ((BrowsePosition)position).Enumerator;
                ArrayList elements = new ArrayList();
                if (FetchElements(((Opc.Ae.BrowsePosition)position).BrowseType, maxElements, enumerator, elements) != 0)
                {
                    position.Dispose();
                    position = (IBrowsePosition)null;
                }
                return (BrowseElement[])elements.ToArray(typeof(BrowseElement));
            }
        }

        private void InitializeBrowser()
        {
            if (m_browser != null)
                return;
            object ppUnk = (object)null;
            Guid guid = typeof(IOPCEventAreaBrowser).GUID;
            try
            {
                ((IOPCEventServer)m_server).CreateAreaBrowser(ref guid, out ppUnk);
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCEventServer.CreateAreaBrowser", ex);
            }
            m_browser = ppUnk != null ? ppUnk : throw new InvalidResponseException("unknown == null");
        }

        private void ChangeBrowsePosition(string areaID)
        {
            string szString = areaID != null ? areaID : "";
            try
            {
                ((IOPCEventAreaBrowser)m_browser).ChangeBrowsePosition(OPCAEBROWSEDIRECTION.OPCAE_BROWSE_TO, szString);
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCEventAreaBrowser.ChangeBrowsePosition", ex);
            }
        }

        private object CreateEnumerator(BrowseType browseType, string browseFilter)
        {
            OPCAEBROWSETYPE browseType1 = Interop.GetBrowseType(browseType);
            OpcRcw.Comn.IEnumString ppIEnumString = (OpcRcw.Comn.IEnumString)null;
            try
            {
                ((IOPCEventAreaBrowser)m_browser).BrowseOPCAreas(browseType1, browseFilter != null ? browseFilter : "", out ppIEnumString);
            }
            catch (Exception ex)
            {
                throw OpcCom.Interop.CreateException("IOPCEventAreaBrowser.BrowseOPCAreas", ex);
            }
            return ppIEnumString != null ? (object)(System.Runtime.InteropServices.ComTypes.IEnumString)ppIEnumString : throw new InvalidResponseException("enumerator == null");
        }

        private string GetQualifiedName(string name, BrowseType browseType)
        {
            string qualifiedName = (string)null;
            if (browseType == BrowseType.Area)
            {
                try
                {
                    ((IOPCEventAreaBrowser)m_browser).GetQualifiedAreaName(name, out qualifiedName);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventAreaBrowser.GetQualifiedAreaName", ex);
                }
            }
            else
            {
                try
                {
                    ((IOPCEventAreaBrowser)m_browser).GetQualifiedSourceName(name, out qualifiedName);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCEventAreaBrowser.GetQualifiedSourceName", ex);
                }
            }
            return qualifiedName;
        }

        private int FetchElements(
          BrowseType browseType,
          int maxElements,
          System.Runtime.InteropServices.ComTypes.IEnumString enumerator,
          ArrayList elements)
        {
            string[] rgelt = new string[1];
            int celt1 = maxElements <= 0 || maxElements - elements.Count >= rgelt.Length ? rgelt.Length : maxElements - elements.Count;
            IntPtr num1 = Marshal.AllocCoTaskMem(4);
            try
            {
                int num2;
                int celt2;
                for (num2 = enumerator.Next(celt1, rgelt, num1); num2 == 0; num2 = enumerator.Next(celt2, rgelt, num1))
                {
                    int num3 = Marshal.ReadInt32(num1);
                    for (int index = 0; index < num3; ++index)
                        elements.Add((object)new BrowseElement()
                        {
                            Name = rgelt[index],
                            QualifiedName = GetQualifiedName(rgelt[index], browseType),
                            NodeType = browseType
                        });
                    if (maxElements <= 0 || elements.Count < maxElements)
                        celt2 = maxElements <= 0 || maxElements - elements.Count >= rgelt.Length ? rgelt.Length : maxElements - elements.Count;
                    else
                        break;
                }
                return num2;
            }
            finally
            {
                if (num1 != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(num1);
            }
        }
    }
}
