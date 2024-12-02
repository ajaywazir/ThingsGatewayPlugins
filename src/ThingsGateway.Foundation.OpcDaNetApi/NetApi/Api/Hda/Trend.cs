

using System;
using System.Runtime.Serialization;


namespace Opc.Hda
{
    [Serializable]
    public class Trend : ISerializable, ICloneable
    {
        private static int m_count;
        private Server m_server;
        private string m_name;
        private int m_aggregateID;
        private Time m_startTime;
        private Time m_endTime;
        private int m_maxValues;
        private bool m_includeBounds;
        private Decimal m_resampleInterval;
        private ItemTimeCollection m_timestamps = new ItemTimeCollection();
        private ItemCollection m_items = new ItemCollection();
        private Decimal m_updateInterval;
        private Decimal m_playbackInterval;
        private Decimal m_playbackDuration;
        private IRequest m_subscription;
        private IRequest m_playback;

        public Trend(Server server)
        {
            m_server = server != null ? server : throw new ArgumentNullException(nameof(server));
            do
            {
                Name = string.Format("Trend{0,2:00}", (object)++Trend.m_count);
            }
            while (m_server.Trends[Name] != null);
        }

        public Server Server => m_server;

        public string Name
        {
            get => m_name;
            set => m_name = value;
        }

        public int AggregateID
        {
            get => m_aggregateID;
            set => m_aggregateID = value;
        }

        public Time StartTime
        {
            get => m_startTime;
            set => m_startTime = value;
        }

        public Time EndTime
        {
            get => m_endTime;
            set => m_endTime = value;
        }

        public int MaxValues
        {
            get => m_maxValues;
            set => m_maxValues = value;
        }

        public bool IncludeBounds
        {
            get => m_includeBounds;
            set => m_includeBounds = value;
        }

        public Decimal ResampleInterval
        {
            get => m_resampleInterval;
            set => m_resampleInterval = value;
        }

        public ItemTimeCollection Timestamps
        {
            get => m_timestamps;
            set
            {
                m_timestamps = value != null ? value : throw new ArgumentNullException(nameof(value));
            }
        }

        public Decimal UpdateInterval
        {
            get => m_updateInterval;
            set => m_updateInterval = value;
        }

        public bool SubscriptionActive => m_subscription != null;

        public Decimal PlaybackInterval
        {
            get => m_playbackInterval;
            set => m_playbackInterval = value;
        }

        public Decimal PlaybackDuration
        {
            get => m_playbackDuration;
            set => m_playbackDuration = value;
        }

        public bool PlaybackActive => m_playback != null;

        public ItemCollection Items => m_items;

        public Item[] GetItems()
        {
            Item[] items = new Item[m_items.Count];
            for (int index = 0; index < m_items.Count; ++index)
                items[index] = m_items[index];
            return items;
        }

        public Item AddItem(ItemIdentifier itemID)
        {
            if (itemID == null)
                throw new ArgumentNullException(nameof(itemID));
            if (itemID.ClientHandle == null)
                itemID.ClientHandle = (object)Guid.NewGuid().ToString();
            IdentifiedResult[] items = m_server.CreateItems(new ItemIdentifier[1]
            {
        itemID
            });
            if (items == null || items.Length != 1)
                throw new InvalidResponseException();
            Item obj = !items[0].ResultID.Failed() ? new Item((ItemIdentifier)items[0]) : throw new ResultIDException(items[0].ResultID, "Could not add item to trend.");
            m_items.Add(obj);
            return obj;
        }

        public void RemoveItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            for (int index = 0; index < m_items.Count; ++index)
            {
                if (item.Equals((object)m_items[index]))
                {
                    m_server.ReleaseItems(new ItemIdentifier[1]
                    {
            (ItemIdentifier) item
                    });
                    m_items.RemoveAt(index);
                    return;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(item), (object)item.Key, "Item not found in collection.");
        }

        public void ClearItems()
        {
            m_server.ReleaseItems((ItemIdentifier[])GetItems());
            m_items.Clear();
        }

        public ItemValueCollection[] Read() => Read(GetItems());

        public ItemValueCollection[] Read(Item[] items)
        {
            return AggregateID == 0 ? ReadRaw(items) : ReadProcessed(items);
        }

        public IdentifiedResult[] Read(
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return Read(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] Read(
          Item[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return AggregateID == 0 ? ReadRaw((ItemIdentifier[])items, requestHandle, callback, out request) : ReadProcessed(items, requestHandle, callback, out request);
        }

        public ItemValueCollection[] ReadRaw() => ReadRaw(GetItems());

        public ItemValueCollection[] ReadRaw(Item[] items)
        {
            return m_server.ReadRaw(StartTime, EndTime, MaxValues, IncludeBounds, (ItemIdentifier[])items);
        }

        public IdentifiedResult[] ReadRaw(
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return Read(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] ReadRaw(
          ItemIdentifier[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return m_server.ReadRaw(StartTime, EndTime, MaxValues, IncludeBounds, items, requestHandle, callback, out request);
        }

        public ItemValueCollection[] ReadProcessed() => ReadProcessed(GetItems());

        public ItemValueCollection[] ReadProcessed(Item[] items)
        {
            return m_server.ReadProcessed(StartTime, EndTime, ResampleInterval, ApplyDefaultAggregate(items));
        }

        public IdentifiedResult[] ReadProcessed(
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return ReadProcessed(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] ReadProcessed(
          Item[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return m_server.ReadProcessed(StartTime, EndTime, ResampleInterval, ApplyDefaultAggregate(items), requestHandle, callback, out request);
        }

        public IdentifiedResult[] Subscribe(object subscriptionHandle, DataUpdateEventHandler callback)
        {
            return AggregateID != 0 ? m_server.AdviseProcessed(StartTime, ResampleInterval, (int)UpdateInterval, ApplyDefaultAggregate(GetItems()), subscriptionHandle, callback, out m_subscription) : m_server.AdviseRaw(StartTime, UpdateInterval, (ItemIdentifier[])GetItems(), subscriptionHandle, callback, out m_subscription);
        }

        public void SubscribeCancel()
        {
            if (m_subscription == null)
                return;
            m_server.CancelRequest(m_subscription);
            m_subscription = (IRequest)null;
        }

        public IdentifiedResult[] Playback(object playbackHandle, DataUpdateEventHandler callback)
        {
            return AggregateID != 0 ? m_server.PlaybackProcessed(StartTime, EndTime, ResampleInterval, (int)PlaybackDuration, PlaybackInterval, ApplyDefaultAggregate(GetItems()), playbackHandle, callback, out m_playback) : m_server.PlaybackRaw(StartTime, EndTime, MaxValues, PlaybackInterval, PlaybackDuration, (ItemIdentifier[])GetItems(), playbackHandle, callback, out m_playback);
        }

        public void PlaybackCancel()
        {
            if (m_playback == null)
                return;
            m_server.CancelRequest(m_playback);
            m_playback = (IRequest)null;
        }

        public ModifiedValueCollection[] ReadModified() => ReadModified(GetItems());

        public ModifiedValueCollection[] ReadModified(Item[] items)
        {
            return m_server.ReadModified(StartTime, EndTime, MaxValues, (ItemIdentifier[])items);
        }

        public IdentifiedResult[] ReadModified(
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return ReadModified(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] ReadModified(
          Item[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return m_server.ReadModified(StartTime, EndTime, MaxValues, (ItemIdentifier[])items, requestHandle, callback, out request);
        }

        public ItemValueCollection[] ReadAtTime() => ReadAtTime(GetItems());

        public ItemValueCollection[] ReadAtTime(Item[] items)
        {
            DateTime[] timestamps = new DateTime[Timestamps.Count];
            for (int index = 0; index < Timestamps.Count; ++index)
                timestamps[index] = Timestamps[index];
            return m_server.ReadAtTime(timestamps, (ItemIdentifier[])items);
        }

        public IdentifiedResult[] ReadAtTime(
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            return ReadAtTime(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] ReadAtTime(
          Item[] items,
          object requestHandle,
          ReadValuesEventHandler callback,
          out IRequest request)
        {
            DateTime[] timestamps = new DateTime[Timestamps.Count];
            for (int index = 0; index < Timestamps.Count; ++index)
                timestamps[index] = Timestamps[index];
            return m_server.ReadAtTime(timestamps, (ItemIdentifier[])items, requestHandle, callback, out request);
        }

        public ItemAttributeCollection ReadAttributes(ItemIdentifier item, int[] attributeIDs)
        {
            return m_server.ReadAttributes(StartTime, EndTime, item, attributeIDs);
        }

        public ResultCollection ReadAttributes(
          ItemIdentifier item,
          int[] attributeIDs,
          object requestHandle,
          ReadAttributesEventHandler callback,
          out IRequest request)
        {
            return m_server.ReadAttributes(StartTime, EndTime, item, attributeIDs, requestHandle, callback, out request);
        }

        public AnnotationValueCollection[] ReadAnnotations() => ReadAnnotations(GetItems());

        public AnnotationValueCollection[] ReadAnnotations(Item[] items)
        {
            return m_server.ReadAnnotations(StartTime, EndTime, (ItemIdentifier[])items);
        }

        public IdentifiedResult[] ReadAnnotations(
          object requestHandle,
          ReadAnnotationsEventHandler callback,
          out IRequest request)
        {
            return ReadAnnotations(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] ReadAnnotations(
          Item[] items,
          object requestHandle,
          ReadAnnotationsEventHandler callback,
          out IRequest request)
        {
            return m_server.ReadAnnotations(StartTime, EndTime, (ItemIdentifier[])items, requestHandle, callback, out request);
        }

        public IdentifiedResult[] Delete() => Delete(GetItems());

        public IdentifiedResult[] Delete(Item[] items)
        {
            return m_server.Delete(StartTime, EndTime, (ItemIdentifier[])items);
        }

        public IdentifiedResult[] Delete(
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            return Delete((ItemIdentifier[])GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] Delete(
          ItemIdentifier[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            return m_server.Delete(StartTime, EndTime, items, requestHandle, callback, out request);
        }

        public ResultCollection[] DeleteAtTime() => DeleteAtTime(GetItems());

        public ResultCollection[] DeleteAtTime(Item[] items)
        {
            ItemTimeCollection[] items1 = new ItemTimeCollection[items.Length];
            for (int index = 0; index < items.Length; ++index)
            {
                items1[index] = (ItemTimeCollection)Timestamps.Clone();
                items1[index].ItemName = items[index].ItemName;
                items1[index].ItemPath = items[index].ItemPath;
                items1[index].ClientHandle = items[index].ClientHandle;
                items1[index].ServerHandle = items[index].ServerHandle;
            }
            return m_server.DeleteAtTime(items1);
        }

        public IdentifiedResult[] DeleteAtTime(
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            return DeleteAtTime(GetItems(), requestHandle, callback, out request);
        }

        public IdentifiedResult[] DeleteAtTime(
          Item[] items,
          object requestHandle,
          UpdateCompleteEventHandler callback,
          out IRequest request)
        {
            ItemTimeCollection[] items1 = new ItemTimeCollection[items.Length];
            for (int index = 0; index < items.Length; ++index)
            {
                items1[index] = (ItemTimeCollection)Timestamps.Clone();
                items1[index].ItemName = items[index].ItemName;
                items1[index].ItemPath = items[index].ItemPath;
                items1[index].ClientHandle = items[index].ClientHandle;
                items1[index].ServerHandle = items[index].ServerHandle;
            }
            return m_server.DeleteAtTime(items1, requestHandle, callback, out request);
        }

        protected Trend(SerializationInfo info, StreamingContext context)
        {
            m_name = (string)info.GetValue(nameof(Name), typeof(string));
            m_aggregateID = (int)info.GetValue(nameof(AggregateID), typeof(int));
            m_startTime = (Time)info.GetValue(nameof(StartTime), typeof(Time));
            m_endTime = (Time)info.GetValue(nameof(EndTime), typeof(Time));
            m_maxValues = (int)info.GetValue(nameof(MaxValues), typeof(int));
            m_includeBounds = (bool)info.GetValue(nameof(IncludeBounds), typeof(bool));
            m_resampleInterval = (Decimal)info.GetValue(nameof(ResampleInterval), typeof(Decimal));
            m_updateInterval = (Decimal)info.GetValue(nameof(UpdateInterval), typeof(Decimal));
            m_playbackInterval = (Decimal)info.GetValue(nameof(PlaybackInterval), typeof(Decimal));
            m_playbackDuration = (Decimal)info.GetValue(nameof(PlaybackDuration), typeof(Decimal));
            DateTime[] dateTimeArray = (DateTime[])info.GetValue(nameof(Timestamps), typeof(DateTime[]));
            if (dateTimeArray != null)
            {
                foreach (DateTime dateTime in dateTimeArray)
                    m_timestamps.Add(dateTime);
            }
            Item[] objArray = (Item[])info.GetValue(nameof(Items), typeof(Item[]));
            if (objArray == null)
                return;
            foreach (Item obj in objArray)
                m_items.Add(obj);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", (object)m_name);
            info.AddValue("AggregateID", m_aggregateID);
            info.AddValue("StartTime", (object)m_startTime);
            info.AddValue("EndTime", (object)m_endTime);
            info.AddValue("MaxValues", m_maxValues);
            info.AddValue("IncludeBounds", m_includeBounds);
            info.AddValue("ResampleInterval", m_resampleInterval);
            info.AddValue("UpdateInterval", m_updateInterval);
            info.AddValue("PlaybackInterval", m_playbackInterval);
            info.AddValue("PlaybackDuration", m_playbackDuration);
            DateTime[] dateTimeArray = (DateTime[])null;
            if (m_timestamps.Count > 0)
            {
                dateTimeArray = new DateTime[m_timestamps.Count];
                for (int index = 0; index < dateTimeArray.Length; ++index)
                    dateTimeArray[index] = m_timestamps[index];
            }
            info.AddValue("Timestamps", (object)dateTimeArray);
            Item[] objArray = (Item[])null;
            if (m_items.Count > 0)
            {
                objArray = new Item[m_items.Count];
                for (int index = 0; index < objArray.Length; ++index)
                    objArray[index] = m_items[index];
            }
            info.AddValue("Items", (object)objArray);
        }

        internal void SetServer(Server server) => m_server = server;

        public virtual object Clone()
        {
            Trend trend = (Trend)MemberwiseClone();
            trend.m_items = new ItemCollection();
            foreach (Item obj in m_items)
                trend.m_items.Add(obj.Clone());
            trend.m_timestamps = new ItemTimeCollection();
            foreach (DateTime timestamp in m_timestamps)
                trend.m_timestamps.Add(timestamp);
            trend.m_subscription = (IRequest)null;
            trend.m_playback = (IRequest)null;
            return (object)trend;
        }

        private Item[] ApplyDefaultAggregate(Item[] items)
        {
            int num = AggregateID;
            if (num == 0)
                num = 1;
            Item[] objArray = new Item[items.Length];
            for (int index = 0; index < items.Length; ++index)
            {
                objArray[index] = new Item(items[index]);
                if (objArray[index].AggregateID == 0)
                    objArray[index].AggregateID = num;
            }
            return objArray;
        }

        private sealed class Names
        {
            internal const string NAME = "Name";
            internal const string AGGREGATE_ID = "AggregateID";
            internal const string START_TIME = "StartTime";
            internal const string END_TIME = "EndTime";
            internal const string MAX_VALUES = "MaxValues";
            internal const string INCLUDE_BOUNDS = "IncludeBounds";
            internal const string RESAMPLE_INTERVAL = "ResampleInterval";
            internal const string UPDATE_INTERVAL = "UpdateInterval";
            internal const string PLAYBACK_INTERVAL = "PlaybackInterval";
            internal const string PLAYBACK_DURATION = "PlaybackDuration";
            internal const string TIMESTAMPS = "Timestamps";
            internal const string ITEMS = "Items";
        }
    }
}
