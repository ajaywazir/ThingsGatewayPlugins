

using System;
using System.Collections;
using System.Runtime.Serialization;


namespace Opc.Da
{
  [Serializable]
  public class Subscription : ISubscription, IDisposable, ISerializable, ICloneable
  {
    internal Server m_server;
    internal ISubscription m_subscription;
    private SubscriptionState m_state = new SubscriptionState();
    private Item[] m_items;
    private bool m_enabled = true;
    private int m_filters = 9;

    public Subscription(Server server, ISubscription subscription)
    {
      if (server == null)
        throw new ArgumentNullException(nameof (server));
      if (subscription == null)
        throw new ArgumentNullException(nameof (subscription));
      this.m_server = server;
      this.m_subscription = subscription;
      this.GetResultFilters();
      this.GetState();
    }

    public void Dispose()
    {
      if (this.m_subscription == null)
        return;
      this.m_subscription.Dispose();
      this.m_server = (Server) null;
      this.m_subscription = (ISubscription) null;
      this.m_items = (Item[]) null;
    }

    protected Subscription(SerializationInfo info, StreamingContext context)
    {
      this.m_state = (SubscriptionState) info.GetValue(nameof (State), typeof (SubscriptionState));
      this.m_filters = (int) info.GetValue(nameof (Filters), typeof (int));
      this.m_items = (Item[]) info.GetValue(nameof (Items), typeof (Item[]));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("State", (object) this.m_state);
      info.AddValue("Filters", this.m_filters);
      info.AddValue("Items", (object) this.m_items);
    }

    public virtual object Clone()
    {
      Subscription subscription = (Subscription) this.MemberwiseClone();
      subscription.m_server = (Server) null;
      subscription.m_subscription = (ISubscription) null;
      subscription.m_state = (SubscriptionState) this.m_state.Clone();
      subscription.m_state.ServerHandle = (object) null;
      subscription.m_state.Active = false;
      if (subscription.m_items != null)
      {
        ArrayList arrayList = new ArrayList();
        foreach (Item obj in subscription.m_items)
          arrayList.Add(obj.Clone());
        subscription.m_items = (Item[]) arrayList.ToArray(typeof (Item));
      }
      return (object) subscription;
    }

    public Server Server => this.m_server;

    public string Name => this.m_state.Name;

    public object ClientHandle => this.m_state.ClientHandle;

    public object ServerHandle => this.m_state.ServerHandle;

    public bool Active => this.m_state.Active;

    public bool Enabled => this.m_enabled;

    public string Locale => this.m_state.Locale;

    public int Filters => this.m_filters;

    public SubscriptionState State => (SubscriptionState) this.m_state.Clone();

    public Item[] Items
    {
      get
      {
        if (this.m_items == null)
          return new Item[0];
        Item[] items = new Item[this.m_items.Length];
        for (int index = 0; index < this.m_items.Length; ++index)
          items[index] = (Item) this.m_items[index].Clone();
        return items;
      }
    }

    public event DataChangedEventHandler DataChanged
    {
      add => this.m_subscription.DataChanged += value;
      remove => this.m_subscription.DataChanged -= value;
    }

    public int GetResultFilters()
    {
      this.m_filters = this.m_subscription.GetResultFilters();
      return this.m_filters;
    }

    public void SetResultFilters(int filters)
    {
      this.m_subscription.SetResultFilters(filters);
      this.m_filters = filters;
    }

    public SubscriptionState GetState()
    {
      this.m_state = this.m_subscription.GetState();
      return this.m_state;
    }

    public SubscriptionState ModifyState(int masks, SubscriptionState state)
    {
      this.m_state = this.m_subscription.ModifyState(masks, state);
      return this.m_state;
    }

    public virtual ItemResult[] AddItems(Item[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (items.Length == 0)
        return new ItemResult[0];
      ItemResult[] itemResultArray = this.m_subscription.AddItems(items);
      if (itemResultArray == null || itemResultArray.Length == 0)
        throw new InvalidResponseException();
      ArrayList arrayList = new ArrayList();
      if (this.m_items != null)
        arrayList.AddRange((ICollection) this.m_items);
      for (int index = 0; index < itemResultArray.Length; ++index)
      {
        if (!itemResultArray[index].ResultID.Failed())
        {
          Item obj = new Item((Item) itemResultArray[index]);
          obj.ItemName = items[index].ItemName;
          obj.ItemPath = items[index].ItemPath;
          obj.ClientHandle = items[index].ClientHandle;
          arrayList.Add((object) obj);
        }
      }
      this.m_items = (Item[]) arrayList.ToArray(typeof (Item));
      this.GetState();
      return itemResultArray;
    }

    public virtual ItemResult[] ModifyItems(int masks, Item[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (items.Length == 0)
        return new ItemResult[0];
      ItemResult[] itemResultArray = this.m_subscription.ModifyItems(masks, items);
      if (itemResultArray == null || itemResultArray.Length == 0)
        throw new InvalidResponseException();
      for (int index1 = 0; index1 < itemResultArray.Length; ++index1)
      {
        if (!itemResultArray[index1].ResultID.Failed())
        {
          for (int index2 = 0; index2 < this.m_items.Length; ++index2)
          {
            if (this.m_items[index2].ServerHandle.Equals(items[index1].ServerHandle))
            {
              Item obj = new Item((Item) itemResultArray[index1]);
              obj.ItemName = this.m_items[index2].ItemName;
              obj.ItemPath = this.m_items[index2].ItemPath;
              obj.ClientHandle = this.m_items[index2].ClientHandle;
              this.m_items[index2] = obj;
              break;
            }
          }
        }
      }
      this.GetState();
      return itemResultArray;
    }

    public virtual IdentifiedResult[] RemoveItems(ItemIdentifier[] items)
    {
      if (items == null)
        throw new ArgumentNullException(nameof (items));
      if (items.Length == 0)
        return new IdentifiedResult[0];
      IdentifiedResult[] identifiedResultArray = this.m_subscription.RemoveItems(items);
      if (identifiedResultArray == null || identifiedResultArray.Length == 0)
        throw new InvalidResponseException();
      ArrayList arrayList = new ArrayList();
      foreach (Item obj in this.m_items)
      {
        bool flag = false;
        for (int index = 0; index < identifiedResultArray.Length; ++index)
        {
          if (obj.ServerHandle.Equals(items[index].ServerHandle))
          {
            flag = identifiedResultArray[index].ResultID.Succeeded();
            break;
          }
        }
        if (!flag)
          arrayList.Add((object) obj);
      }
      this.m_items = (Item[]) arrayList.ToArray(typeof (Item));
      this.GetState();
      return identifiedResultArray;
    }

    public ItemValueResult[] Read(Item[] items) => this.m_subscription.Read(items);

    public IdentifiedResult[] Write(ItemValue[] items) => this.m_subscription.Write(items);

    public IdentifiedResult[] Read(
      Item[] items,
      object requestHandle,
      ReadCompleteEventHandler callback,
      out IRequest request)
    {
      return this.m_subscription.Read(items, requestHandle, callback, out request);
    }

    public IdentifiedResult[] Write(
      ItemValue[] items,
      object requestHandle,
      WriteCompleteEventHandler callback,
      out IRequest request)
    {
      return this.m_subscription.Write(items, requestHandle, callback, out request);
    }

    public void Cancel(IRequest request, CancelCompleteEventHandler callback)
    {
      this.m_subscription.Cancel(request, callback);
    }

    public void Refresh() => this.m_subscription.Refresh();

    public void Refresh(object requestHandle, out IRequest request)
    {
      this.m_subscription.Refresh(requestHandle, out request);
    }

    public void SetEnabled(bool enabled)
    {
      this.m_subscription.SetEnabled(enabled);
      this.m_enabled = enabled;
    }

    public bool GetEnabled()
    {
      this.m_enabled = this.m_subscription.GetEnabled();
      return this.m_enabled;
    }

    private class Names
    {
      internal const string STATE = "State";
      internal const string FILTERS = "Filters";
      internal const string ITEMS = "Items";
    }
  }
}
