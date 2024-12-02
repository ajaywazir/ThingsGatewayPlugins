

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Hda
{
    [Serializable]
    public class Server : Opc.Server, IServer, Opc.IServer, IDisposable
    {
        private Hashtable m_items = new Hashtable();
        private AttributeCollection m_attributes = new AttributeCollection();
        private AggregateCollection m_aggregates = new AggregateCollection();
        private TrendCollection m_trends = new TrendCollection();

        public Server(Factory factory, URL url)
          : base(factory, url)
        {
        }

        public AttributeCollection Attributes => m_attributes;

        public AggregateCollection Aggregates => m_aggregates;

        public ItemIdentifierCollection Items => new ItemIdentifierCollection(m_items.Values);

        public TrendCollection Trends => m_trends;

        public override void Connect(URL url, ConnectData connectData)
        {
            base.Connect(url, connectData);
            GetAttributes();
            GetAggregates();
            foreach (Trend trend in m_trends)
            {
                ArrayList arrayList = new ArrayList();
                foreach (Item itemID in trend.Items)
                    arrayList.Add((object)new ItemIdentifier((ItemIdentifier)itemID));
                IdentifiedResult[] items = CreateItems((ItemIdentifier[])arrayList.ToArray(typeof(ItemIdentifier)));
                if (items != null)
                {
                    for (int index = 0; index < items.Length; ++index)
                    {
                        trend.Items[index].ServerHandle = (object)null;
                        if (items[index].ResultID.Succeeded())
                            trend.Items[index].ServerHandle = items[index].ServerHandle;
                    }
                }
            }
        }

        public override void Disconnect()
        {
            if (m_server == null)
                throw new NotConnectedException();
            if (m_items.Count > 0)
            {
                try
                {
                    ArrayList arrayList = new ArrayList(m_items.Count);
                    arrayList.AddRange((ICollection)m_items);
                    ((IServer)m_server).ReleaseItems((ItemIdentifier[])arrayList.ToArray(typeof(ItemIdentifier)));
                }
                catch
                {
                }
                m_items.Clear();
            }
            foreach (Trend trend in m_trends)
            {
                foreach (ItemIdentifier itemIdentifier in trend.Items)
                    itemIdentifier.ServerHandle = (object)null;
            }
            base.Disconnect();
        }

        public ServerStatus GetStatus()
        {
            ServerStatus status = m_server != null ? ((IServer)m_server).GetStatus() : throw new NotConnectedException();
            if (status.StatusInfo == null)
                status.StatusInfo = GetString("serverState." + status.ServerState.ToString());
            return status;
        }

        public Attribute[] GetAttributes()
        {
            if (m_server == null)
                throw new NotConnectedException();
            m_attributes.Clear();
            Attribute[] attributes = ((IServer)m_server).GetAttributes();
            if (attributes != null)
                m_attributes.Init((ICollection)attributes);
            return attributes;
        }

        public Aggregate[] GetAggregates()
        {
            if (m_server == null)
                throw new NotConnectedException();
            m_aggregates.Clear();
            Aggregate[] aggregates = ((IServer)m_server).GetAggregates();
            if (aggregates != null)
                m_aggregates.Init((ICollection)aggregates);
            return aggregates;
        }

        public IBrowser CreateBrowser(BrowseFilter[] filters, out ResultID[] results)
        {
            return m_server != null ? ((IServer)m_server).CreateBrowser(filters, out results) : throw new NotConnectedException();
        }

        public IdentifiedResult[] CreateItems(ItemIdentifier[] items)
        {
            IdentifiedResult[] items1 = m_server != null ? ((IServer)m_server).CreateItems(items) : throw new NotConnectedException();
            if (items1 != null)
            {
                foreach (IdentifiedResult itemID in items1)
                {
                    if (itemID.ResultID.Succeeded())
                        m_items.Add(itemID.ServerHandle, (object)new ItemIdentifier((ItemIdentifier)itemID));
                }
            }
            return items1;
        }

        public IdentifiedResult[] ReleaseItems(ItemIdentifier[] items)
        {
            IdentifiedResult[] identifiedResultArray = m_server != null ? ((IServer)m_server).ReleaseItems(items) : throw new NotConnectedException();
            if (identifiedResultArray != null)
            {
                foreach (IdentifiedResult identifiedResult in identifiedResultArray)
                {
                    if (identifiedResult.ResultID.Succeeded())
                        m_items.Remove(identifiedResult.ServerHandle);
                }
            }
            return identifiedResultArray;
        }

        public IdentifiedResult[] ValidateItems(ItemIdentifier[] items)
        {
            return m_server != null ? ((IServer)m_server).ValidateItems(items) : throw new NotConnectedException();
        }

        public ItemValueCollection[] ReadRaw(
          Time startTime,
          Time endTime,
          int maxValues,
          bool includeBounds,
          ItemIdentifier[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadRaw(startTime, endTime, maxValues, includeBounds, items);
        }

        public IdentifiedResult[] ReadRaw(
          Time startTime,
          Time endTime,
          int maxValues,
          bool includeBounds,
          ItemIdentifier[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadRaw(startTime, endTime, maxValues, includeBounds, items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] AdviseRaw(
          Time startTime,
          Decimal updateInterval,
          ItemIdentifier[] items,
          object requestHandle,
          DataUpdateEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).AdviseRaw(startTime, updateInterval, items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] PlaybackRaw(
          Time startTime,
          Time endTime,
          int maxValues,
          Decimal updateInterval,
          Decimal playbackDuration,
          ItemIdentifier[] items,
          object requestHandle,
          DataUpdateEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).PlaybackRaw(startTime, endTime, maxValues, updateInterval, playbackDuration, items, requestHandle, callback, out request);
        }

        public ItemValueCollection[] ReadProcessed(
          Time startTime,
          Time endTime,
          Decimal resampleInterval,
          Item[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadProcessed(startTime, endTime, resampleInterval, items);
        }

        public IdentifiedResult[] ReadProcessed(
          Time startTime,
          Time endTime,
          Decimal resampleInterval,
          Item[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadProcessed(startTime, endTime, resampleInterval, items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] AdviseProcessed(
          Time startTime,
          Decimal resampleInterval,
          int numberOfIntervals,
          Item[] items,
          object requestHandle,
          DataUpdateEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).AdviseProcessed(startTime, resampleInterval, numberOfIntervals, items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] PlaybackProcessed(
          Time startTime,
          Time endTime,
          Decimal resampleInterval,
          int numberOfIntervals,
          Decimal updateInterval,
          Item[] items,
          object requestHandle,
          DataUpdateEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).PlaybackProcessed(startTime, endTime, resampleInterval, numberOfIntervals, updateInterval, items, requestHandle, callback, out request);
        }

        public ItemValueCollection[] ReadAtTime(DateTime[] timestamps, ItemIdentifier[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAtTime(timestamps, items);
        }

        public IdentifiedResult[] ReadAtTime(
          DateTime[] timestamps,
          ItemIdentifier[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAtTime(timestamps, items, requestHandle, callback, out request);
        }

        public ModifiedValueCollection[] ReadModified(
          Time startTime,
          Time endTime,
          int maxValues,
          ItemIdentifier[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadModified(startTime, endTime, maxValues, items);
        }

        public IdentifiedResult[] ReadModified(
          Time startTime,
          Time endTime,
          int maxValues,
          ItemIdentifier[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadModified(startTime, endTime, maxValues, items, requestHandle, callback, out request);
        }

        public ItemAttributeCollection ReadAttributes(
          Time startTime,
          Time endTime,
          ItemIdentifier item,
          int[] attributeIDs)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAttributes(startTime, endTime, item, attributeIDs);
        }

        public ResultCollection ReadAttributes(
          Time startTime,
          Time endTime,
          ItemIdentifier item,
          int[] attributeIDs,
          object requestHandle,
          ReadAttributesEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAttributes(startTime, endTime, item, attributeIDs, requestHandle, callback, out request);
        }

        public AnnotationValueCollection[] ReadAnnotations(
          Time startTime,
          Time endTime,
          ItemIdentifier[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAnnotations(startTime, endTime, items);
        }

        public IdentifiedResult[] ReadAnnotations(
          Time startTime,
          Time endTime,
          ItemIdentifier[] items,
          object requestHandle,
          ReadAnnotationsEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).ReadAnnotations(startTime, endTime, items, requestHandle, callback, out request);
        }

        public ResultCollection[] InsertAnnotations(AnnotationValueCollection[] items)
        {
            return m_server != null ? ((IServer)m_server).InsertAnnotations(items) : throw new NotConnectedException();
        }

        public IdentifiedResult[] InsertAnnotations(
          AnnotationValueCollection[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).InsertAnnotations(items, requestHandle, callback, out request);
        }

        public ResultCollection[] Insert(ItemValueCollection[] items, bool replace)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Insert(items, replace);
        }

        public IdentifiedResult[] Insert(
          ItemValueCollection[] items,
          bool replace,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Insert(items, replace, requestHandle, callback, out request);
        }

        public ResultCollection[] Replace(ItemValueCollection[] items)
        {
            return m_server != null ? ((IServer)m_server).Replace(items) : throw new NotConnectedException();
        }

        public IdentifiedResult[] Replace(
          ItemValueCollection[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Replace(items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] Delete(Time startTime, Time endTime, ItemIdentifier[] items)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Delete(startTime, endTime, items);
        }

        public IdentifiedResult[] Delete(
          Time startTime,
          Time endTime,
          ItemIdentifier[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).Delete(startTime, endTime, items, requestHandle, callback, out request);
        }

        public ResultCollection[] DeleteAtTime(ItemTimeCollection[] items)
        {
            return m_server != null ? ((IServer)m_server).DeleteAtTime(items) : throw new NotConnectedException();
        }

        public IdentifiedResult[] DeleteAtTime(
          ItemTimeCollection[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            return ((IServer)m_server).DeleteAtTime(items, requestHandle, callback, out request);
        }

        public void CancelRequest(IRequest request)
        {
            if (m_server == null)
                throw new NotConnectedException();
            ((IServer)m_server).CancelRequest(request);
        }

        public void CancelRequest(IRequest request, CancelCompleteEventHandler callback)
        {
            if (m_server == null)
                throw new NotConnectedException();
            ((IServer)m_server).CancelRequest(request, callback);
        }

        protected Server(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            Trend[] trendArray = (Trend[])info.GetValue(nameof(Trends), typeof(Trend[]));
            if (trendArray == null)
                return;
            foreach (Trend trend in trendArray)
            {
                trend.SetServer(this);
                m_trends.Add(trend);
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            Trend[] trendArray = (Trend[])null;
            if (m_trends.Count > 0)
            {
                trendArray = new Trend[m_trends.Count];
                for (int index = 0; index < trendArray.Length; ++index)
                    trendArray[index] = m_trends[index];
            }
            info.AddValue("Trends", (object)trendArray);
        }

        public override object Clone() => (object)(Server)base.Clone();

        private sealed class Names
        {
            internal const string TRENDS = "Trends";
        }
    }
}
