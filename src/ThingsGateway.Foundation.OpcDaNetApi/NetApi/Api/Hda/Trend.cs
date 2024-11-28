

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
      this.m_server = server != null ? server : throw new ArgumentNullException(nameof (server));
      do
      {
        this.Name = string.Format("Trend{0,2:00}", (object) ++Trend.m_count);
      }
      while (this.m_server.Trends[this.Name] != null);
    }

    public Server Server => this.m_server;

    public string Name
    {
      get => this.m_name;
      set => this.m_name = value;
    }

    public int AggregateID
    {
      get => this.m_aggregateID;
      set => this.m_aggregateID = value;
    }

    public Time StartTime
    {
      get => this.m_startTime;
      set => this.m_startTime = value;
    }

    public Time EndTime
    {
      get => this.m_endTime;
      set => this.m_endTime = value;
    }

    public int MaxValues
    {
      get => this.m_maxValues;
      set => this.m_maxValues = value;
    }

    public bool IncludeBounds
    {
      get => this.m_includeBounds;
      set => this.m_includeBounds = value;
    }

    public Decimal ResampleInterval
    {
      get => this.m_resampleInterval;
      set => this.m_resampleInterval = value;
    }

    public ItemTimeCollection Timestamps
    {
      get => this.m_timestamps;
      set
      {
        this.m_timestamps = value != null ? value : throw new ArgumentNullException(nameof (value));
      }
    }

    public Decimal UpdateInterval
    {
      get => this.m_updateInterval;
      set => this.m_updateInterval = value;
    }

    public bool SubscriptionActive => this.m_subscription != null;

    public Decimal PlaybackInterval
    {
      get => this.m_playbackInterval;
      set => this.m_playbackInterval = value;
    }

    public Decimal PlaybackDuration
    {
      get => this.m_playbackDuration;
      set => this.m_playbackDuration = value;
    }

    public bool PlaybackActive => this.m_playback != null;

    public ItemCollection Items => this.m_items;

    public Item[] GetItems()
    {
      Item[] items = new Item[this.m_items.Count];
      for (int index = 0; index < this.m_items.Count; ++index)
        items[index] = this.m_items[index];
      return items;
    }

    public Item AddItem(ItemIdentifier itemID)
    {
      if (itemID == null)
        throw new ArgumentNullException(nameof (itemID));
      if (itemID.ClientHandle == null)
        itemID.ClientHandle = (object) Guid.NewGuid().ToString();
      IdentifiedResult[] items = this.m_server.CreateItems(new ItemIdentifier[1]
      {
        itemID
      });
      if (items == null || items.Length != 1)
        throw new InvalidResponseException();
      Item obj = !items[0].ResultID.Failed() ? new Item((ItemIdentifier) items[0]) : throw new ResultIDException(items[0].ResultID, "Could not add item to trend.");
      this.m_items.Add(obj);
      return obj;
    }

    public void RemoveItem(Item item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof (item));
      for (int index = 0; index < this.m_items.Count; ++index)
      {
        if (item.Equals((object) this.m_items[index]))
        {
          this.m_server.ReleaseItems(new ItemIdentifier[1]
          {
            (ItemIdentifier) item
          });
          this.m_items.RemoveAt(index);
          return;
        }
      }
      throw new ArgumentOutOfRangeException(nameof (item), (object) item.Key, "Item not found in collection.");
    }

    public void ClearItems()
    {
      this.m_server.ReleaseItems((ItemIdentifier[]) this.GetItems());
      this.m_items.Clear();
    }

    public ItemValueCollection[] Read() => this.Read(this.GetItems());

    public ItemValueCollection[] Read(Item[] items)
    {
      return this.AggregateID == 0 ? this.ReadRaw(items) : this.ReadProcessed(items);
    }

    public IdentifiedResult[] Read(
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.Read(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] Read(
      Item[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.AggregateID == 0 ? this.ReadRaw((ItemIdentifier[]) items, requestHandle, callback, out request) : this.ReadProcessed(items, requestHandle, callback, out request);
    }

    public ItemValueCollection[] ReadRaw() => this.ReadRaw(this.GetItems());

    public ItemValueCollection[] ReadRaw(Item[] items)
    {
      return this.m_server.ReadRaw(this.StartTime, this.EndTime, this.MaxValues, this.IncludeBounds, (ItemIdentifier[]) items);
    }

    public IdentifiedResult[] ReadRaw(
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.Read(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] ReadRaw(
      ItemIdentifier[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.m_server.ReadRaw(this.StartTime, this.EndTime, this.MaxValues, this.IncludeBounds, items, requestHandle, callback, out request);
    }

    public ItemValueCollection[] ReadProcessed() => this.ReadProcessed(this.GetItems());

    public ItemValueCollection[] ReadProcessed(Item[] items)
    {
      return this.m_server.ReadProcessed(this.StartTime, this.EndTime, this.ResampleInterval, this.ApplyDefaultAggregate(items));
    }

    public IdentifiedResult[] ReadProcessed(
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.ReadProcessed(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] ReadProcessed(
      Item[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.m_server.ReadProcessed(this.StartTime, this.EndTime, this.ResampleInterval, this.ApplyDefaultAggregate(items), requestHandle, callback, out request);
    }

    public IdentifiedResult[] Subscribe(object subscriptionHandle, DataUpdateEventHandler callback)
    {
      return this.AggregateID != 0 ? this.m_server.AdviseProcessed(this.StartTime, this.ResampleInterval, (int) this.UpdateInterval, this.ApplyDefaultAggregate(this.GetItems()), subscriptionHandle, callback, out this.m_subscription) : this.m_server.AdviseRaw(this.StartTime, this.UpdateInterval, (ItemIdentifier[]) this.GetItems(), subscriptionHandle, callback, out this.m_subscription);
    }

    public void SubscribeCancel()
    {
      if (this.m_subscription == null)
        return;
      this.m_server.CancelRequest(this.m_subscription);
      this.m_subscription = (IRequest) null;
    }

    public IdentifiedResult[] Playback(object playbackHandle, DataUpdateEventHandler callback)
    {
      return this.AggregateID != 0 ? this.m_server.PlaybackProcessed(this.StartTime, this.EndTime, this.ResampleInterval, (int) this.PlaybackDuration, this.PlaybackInterval, this.ApplyDefaultAggregate(this.GetItems()), playbackHandle, callback, out this.m_playback) : this.m_server.PlaybackRaw(this.StartTime, this.EndTime, this.MaxValues, this.PlaybackInterval, this.PlaybackDuration, (ItemIdentifier[]) this.GetItems(), playbackHandle, callback, out this.m_playback);
    }

    public void PlaybackCancel()
    {
      if (this.m_playback == null)
        return;
      this.m_server.CancelRequest(this.m_playback);
      this.m_playback = (IRequest) null;
    }

    public ModifiedValueCollection[] ReadModified() => this.ReadModified(this.GetItems());

    public ModifiedValueCollection[] ReadModified(Item[] items)
    {
      return this.m_server.ReadModified(this.StartTime, this.EndTime, this.MaxValues, (ItemIdentifier[]) items);
    }

    public IdentifiedResult[] ReadModified(
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.ReadModified(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] ReadModified(
      Item[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.m_server.ReadModified(this.StartTime, this.EndTime, this.MaxValues, (ItemIdentifier[]) items, requestHandle, callback, out request);
    }

    public ItemValueCollection[] ReadAtTime() => this.ReadAtTime(this.GetItems());

    public ItemValueCollection[] ReadAtTime(Item[] items)
    {
      DateTime[] timestamps = new DateTime[this.Timestamps.Count];
      for (int index = 0; index < this.Timestamps.Count; ++index)
        timestamps[index] = this.Timestamps[index];
      return this.m_server.ReadAtTime(timestamps, (ItemIdentifier[]) items);
    }

    public IdentifiedResult[] ReadAtTime(
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      return this.ReadAtTime(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] ReadAtTime(
      Item[] items,
      object requestHandle,
      ReadValuesEventHandler callback,
      out IRequest request)
    {
      DateTime[] timestamps = new DateTime[this.Timestamps.Count];
      for (int index = 0; index < this.Timestamps.Count; ++index)
        timestamps[index] = this.Timestamps[index];
      return this.m_server.ReadAtTime(timestamps, (ItemIdentifier[]) items, requestHandle, callback, out request);
    }

    public ItemAttributeCollection ReadAttributes(ItemIdentifier item, int[] attributeIDs)
    {
      return this.m_server.ReadAttributes(this.StartTime, this.EndTime, item, attributeIDs);
    }

    public ResultCollection ReadAttributes(
      ItemIdentifier item,
      int[] attributeIDs,
      object requestHandle,
      ReadAttributesEventHandler callback,
      out IRequest request)
    {
      return this.m_server.ReadAttributes(this.StartTime, this.EndTime, item, attributeIDs, requestHandle, callback, out request);
    }

    public AnnotationValueCollection[] ReadAnnotations() => this.ReadAnnotations(this.GetItems());

    public AnnotationValueCollection[] ReadAnnotations(Item[] items)
    {
      return this.m_server.ReadAnnotations(this.StartTime, this.EndTime, (ItemIdentifier[]) items);
    }

    public IdentifiedResult[] ReadAnnotations(
      object requestHandle,
      ReadAnnotationsEventHandler callback,
      out IRequest request)
    {
      return this.ReadAnnotations(this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] ReadAnnotations(
      Item[] items,
      object requestHandle,
      ReadAnnotationsEventHandler callback,
      out IRequest request)
    {
      return this.m_server.ReadAnnotations(this.StartTime, this.EndTime, (ItemIdentifier[]) items, requestHandle, callback, out request);
    }

    public IdentifiedResult[] Delete() => this.Delete(this.GetItems());

    public IdentifiedResult[] Delete(Item[] items)
    {
      return this.m_server.Delete(this.StartTime, this.EndTime, (ItemIdentifier[]) items);
    }

    public IdentifiedResult[] Delete(
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      return this.Delete((ItemIdentifier[]) this.GetItems(), requestHandle, callback, out request);
    }

    public IdentifiedResult[] Delete(
      ItemIdentifier[] items,
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      return this.m_server.Delete(this.StartTime, this.EndTime, items, requestHandle, callback, out request);
    }

    public ResultCollection[] DeleteAtTime() => this.DeleteAtTime(this.GetItems());

    public ResultCollection[] DeleteAtTime(Item[] items)
    {
      ItemTimeCollection[] items1 = new ItemTimeCollection[items.Length];
      for (int index = 0; index < items.Length; ++index)
      {
        items1[index] = (ItemTimeCollection) this.Timestamps.Clone();
        items1[index].ItemName = items[index].ItemName;
        items1[index].ItemPath = items[index].ItemPath;
        items1[index].ClientHandle = items[index].ClientHandle;
        items1[index].ServerHandle = items[index].ServerHandle;
      }
      return this.m_server.DeleteAtTime(items1);
    }

    public IdentifiedResult[] DeleteAtTime(
      object requestHandle,
      UpdateCompleteEventHandler callback,
      out IRequest request)
    {
      return this.DeleteAtTime(this.GetItems(), requestHandle, callback, out request);
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
        items1[index] = (ItemTimeCollection) this.Timestamps.Clone();
        items1[index].ItemName = items[index].ItemName;
        items1[index].ItemPath = items[index].ItemPath;
        items1[index].ClientHandle = items[index].ClientHandle;
        items1[index].ServerHandle = items[index].ServerHandle;
      }
      return this.m_server.DeleteAtTime(items1, requestHandle, callback, out request);
    }

    protected Trend(SerializationInfo info, StreamingContext context)
    {
      this.m_name = (string) info.GetValue(nameof (Name), typeof (string));
      this.m_aggregateID = (int) info.GetValue(nameof (AggregateID), typeof (int));
      this.m_startTime = (Time) info.GetValue(nameof (StartTime), typeof (Time));
      this.m_endTime = (Time) info.GetValue(nameof (EndTime), typeof (Time));
      this.m_maxValues = (int) info.GetValue(nameof (MaxValues), typeof (int));
      this.m_includeBounds = (bool) info.GetValue(nameof (IncludeBounds), typeof (bool));
      this.m_resampleInterval = (Decimal) info.GetValue(nameof (ResampleInterval), typeof (Decimal));
      this.m_updateInterval = (Decimal) info.GetValue(nameof (UpdateInterval), typeof (Decimal));
      this.m_playbackInterval = (Decimal) info.GetValue(nameof (PlaybackInterval), typeof (Decimal));
      this.m_playbackDuration = (Decimal) info.GetValue(nameof (PlaybackDuration), typeof (Decimal));
      DateTime[] dateTimeArray = (DateTime[]) info.GetValue(nameof (Timestamps), typeof (DateTime[]));
      if (dateTimeArray != null)
      {
        foreach (DateTime dateTime in dateTimeArray)
          this.m_timestamps.Add(dateTime);
      }
      Item[] objArray = (Item[]) info.GetValue(nameof (Items), typeof (Item[]));
      if (objArray == null)
        return;
      foreach (Item obj in objArray)
        this.m_items.Add(obj);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Name", (object) this.m_name);
      info.AddValue("AggregateID", this.m_aggregateID);
      info.AddValue("StartTime", (object) this.m_startTime);
      info.AddValue("EndTime", (object) this.m_endTime);
      info.AddValue("MaxValues", this.m_maxValues);
      info.AddValue("IncludeBounds", this.m_includeBounds);
      info.AddValue("ResampleInterval", this.m_resampleInterval);
      info.AddValue("UpdateInterval", this.m_updateInterval);
      info.AddValue("PlaybackInterval", this.m_playbackInterval);
      info.AddValue("PlaybackDuration", this.m_playbackDuration);
      DateTime[] dateTimeArray = (DateTime[]) null;
      if (this.m_timestamps.Count > 0)
      {
        dateTimeArray = new DateTime[this.m_timestamps.Count];
        for (int index = 0; index < dateTimeArray.Length; ++index)
          dateTimeArray[index] = this.m_timestamps[index];
      }
      info.AddValue("Timestamps", (object) dateTimeArray);
      Item[] objArray = (Item[]) null;
      if (this.m_items.Count > 0)
      {
        objArray = new Item[this.m_items.Count];
        for (int index = 0; index < objArray.Length; ++index)
          objArray[index] = this.m_items[index];
      }
      info.AddValue("Items", (object) objArray);
    }

    internal void SetServer(Server server) => this.m_server = server;

    public virtual object Clone()
    {
      Trend trend = (Trend) this.MemberwiseClone();
      trend.m_items = new ItemCollection();
      foreach (Item obj in this.m_items)
        trend.m_items.Add(obj.Clone());
      trend.m_timestamps = new ItemTimeCollection();
      foreach (DateTime timestamp in this.m_timestamps)
        trend.m_timestamps.Add(timestamp);
      trend.m_subscription = (IRequest) null;
      trend.m_playback = (IRequest) null;
      return (object) trend;
    }

    private Item[] ApplyDefaultAggregate(Item[] items)
    {
      int num = this.AggregateID;
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

    private class Names
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
