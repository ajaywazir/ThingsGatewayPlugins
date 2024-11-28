

using Opc;
using Opc.Da;
using OpcRcw.Da;
using System;
using System.Collections;


namespace OpcCom.Da20
{
  public class Subscription : OpcCom.Da.Subscription
  {
    internal Subscription(object group, SubscriptionState state, int filters)
      : base(group, state, filters)
    {
    }

    public override void Refresh()
    {
      lock (this)
      {
        if (this.m_group == null)
          throw new NotConnectedException();
        try
        {
          int pdwCancelID = 0;
          ((IOPCAsyncIO2) this.m_group).Refresh2(OPCDATASOURCE.OPC_DS_CACHE, ++this.m_counter, out pdwCancelID);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCAsyncIO2.RefreshMaxAge", ex);
        }
      }
    }

    public override void SetEnabled(bool enabled)
    {
      lock (this)
      {
        if (this.m_group == null)
          throw new NotConnectedException();
        try
        {
          ((IOPCAsyncIO2) this.m_group).SetEnable(enabled ? 1 : 0);
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCAsyncIO2.SetEnable", ex);
        }
      }
    }

    public override bool GetEnabled()
    {
      lock (this)
      {
        if (this.m_group == null)
          throw new NotConnectedException();
        try
        {
          int pbEnable = 0;
          ((IOPCAsyncIO2) this.m_group).GetEnable(out pbEnable);
          return pbEnable != 0;
        }
        catch (Exception ex)
        {
          throw OpcCom.Interop.CreateException("IOPCAsyncIO2.GetEnable", ex);
        }
      }
    }

    protected override ItemValueResult[] Read(ItemIdentifier[] itemIDs, Item[] items)
    {
      ItemValueResult[] itemValueResultArray = new ItemValueResult[itemIDs.Length];
      ArrayList arrayList1 = new ArrayList();
      ArrayList arrayList2 = new ArrayList();
      for (int index = 0; index < itemIDs.Length; ++index)
      {
        itemValueResultArray[index] = new ItemValueResult(itemIDs[index]);
        if (items[index].MaxAgeSpecified && (items[index].MaxAge < 0 || items[index].MaxAge == int.MaxValue))
          arrayList1.Add((object) itemValueResultArray[index]);
        else
          arrayList2.Add((object) itemValueResultArray[index]);
      }
      if (arrayList1.Count > 0)
        this.Read((ItemValueResult[]) arrayList1.ToArray(typeof (ItemValueResult)), true);
      if (arrayList2.Count > 0)
        this.Read((ItemValueResult[]) arrayList2.ToArray(typeof (ItemValueResult)), false);
      return itemValueResultArray;
    }

    private void Read(ItemValueResult[] items, bool cache)
    {
      if (items.Length == 0)
        return;
      int[] phServer = new int[items.Length];
      for (int index = 0; index < items.Length; ++index)
        phServer[index] = (int) items[index].ServerHandle;
      IntPtr ppItemValues = IntPtr.Zero;
      IntPtr ppErrors = IntPtr.Zero;
      try
      {
        ((IOPCSyncIO) this.m_group).Read(cache ? OPCDATASOURCE.OPC_DS_CACHE : OPCDATASOURCE.OPC_DS_DEVICE, items.Length, phServer, out ppItemValues, out ppErrors);
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCSyncIO.Read", ex);
      }
      ItemValue[] itemValues = OpcCom.Da.Interop.GetItemValues(ref ppItemValues, items.Length, true);
      int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, items.Length, true);
      for (int index = 0; index < items.Length; ++index)
      {
        items[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
        items[index].DiagnosticInfo = (string) null;
        if (int32s[index] == -1073479674)
          items[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
        if (items[index].ResultID.Succeeded())
        {
          items[index].Value = itemValues[index].Value;
          items[index].Quality = itemValues[index].Quality;
          items[index].QualitySpecified = itemValues[index].QualitySpecified;
          items[index].Timestamp = itemValues[index].Timestamp;
          items[index].TimestampSpecified = itemValues[index].TimestampSpecified;
        }
      }
    }

    protected override IdentifiedResult[] Write(ItemIdentifier[] itemIDs, ItemValue[] items)
    {
      IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
      ArrayList arrayList1 = new ArrayList(itemIDs.Length);
      ArrayList arrayList2 = new ArrayList(itemIDs.Length);
      for (int index = 0; index < items.Length; ++index)
      {
        identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
        if (items[index].QualitySpecified || items[index].TimestampSpecified)
        {
          identifiedResultArray[index].ResultID = ResultID.Da.E_NO_WRITEQT;
          identifiedResultArray[index].DiagnosticInfo = (string) null;
        }
        else
        {
          arrayList1.Add((object) identifiedResultArray[index]);
          arrayList2.Add((object) items[index]);
        }
      }
      if (arrayList1.Count == 0)
        return identifiedResultArray;
      int[] phServer = new int[arrayList1.Count];
      object[] pItemValues = new object[arrayList1.Count];
      for (int index = 0; index < phServer.Length; ++index)
      {
        phServer[index] = (int) ((ItemIdentifier) arrayList1[index]).ServerHandle;
        pItemValues[index] = OpcCom.Interop.GetVARIANT(((ItemValue) arrayList2[index]).Value);
      }
      IntPtr ppErrors = IntPtr.Zero;
      try
      {
        ((IOPCSyncIO) this.m_group).Write(arrayList1.Count, phServer, pItemValues, out ppErrors);
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCSyncIO.Write", ex);
      }
      int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, arrayList1.Count, true);
      for (int index = 0; index < arrayList1.Count; ++index)
      {
        IdentifiedResult identifiedResult = (IdentifiedResult) arrayList1[index];
        identifiedResult.ResultID = OpcCom.Interop.GetResultID(int32s[index]);
        identifiedResult.DiagnosticInfo = (string) null;
        if (int32s[index] == -1073479674)
          identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
      }
      return identifiedResultArray;
    }

    protected override IdentifiedResult[] BeginRead(
      ItemIdentifier[] itemIDs,
      Item[] items,
      int requestID,
      out int cancelID)
    {
      try
      {
        int[] phServer = new int[itemIDs.Length];
        for (int index = 0; index < itemIDs.Length; ++index)
          phServer[index] = (int) itemIDs[index].ServerHandle;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCAsyncIO2) this.m_group).Read(itemIDs.Length, phServer, requestID, out cancelID, out ppErrors);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, itemIDs.Length, true);
        IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
        for (int index = 0; index < itemIDs.Length; ++index)
        {
          identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
          identifiedResultArray[index].ResultID = OpcCom.Interop.GetResultID(int32s[index]);
          identifiedResultArray[index].DiagnosticInfo = (string) null;
          if (int32s[index] == -1073479674)
            identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
        }
        return identifiedResultArray;
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCAsyncIO2.Read", ex);
      }
    }

    protected override IdentifiedResult[] BeginWrite(
      ItemIdentifier[] itemIDs,
      ItemValue[] items,
      int requestID,
      out int cancelID)
    {
      cancelID = 0;
      ArrayList arrayList1 = new ArrayList();
      ArrayList arrayList2 = new ArrayList();
      IdentifiedResult[] identifiedResultArray = new IdentifiedResult[itemIDs.Length];
      for (int index = 0; index < itemIDs.Length; ++index)
      {
        identifiedResultArray[index] = new IdentifiedResult(itemIDs[index]);
        identifiedResultArray[index].ResultID = ResultID.S_OK;
        identifiedResultArray[index].DiagnosticInfo = (string) null;
        if (items[index].QualitySpecified || items[index].TimestampSpecified)
        {
          identifiedResultArray[index].ResultID = ResultID.Da.E_NO_WRITEQT;
          identifiedResultArray[index].DiagnosticInfo = (string) null;
        }
        else
        {
          arrayList1.Add((object) identifiedResultArray[index]);
          arrayList2.Add(OpcCom.Interop.GetVARIANT(items[index].Value));
        }
      }
      if (arrayList1.Count == 0)
        return identifiedResultArray;
      try
      {
        int[] phServer = new int[arrayList1.Count];
        for (int index = 0; index < arrayList1.Count; ++index)
          phServer[index] = (int) ((ItemIdentifier) arrayList1[index]).ServerHandle;
        IntPtr ppErrors = IntPtr.Zero;
        ((IOPCAsyncIO2) this.m_group).Write(arrayList1.Count, phServer, (object[]) arrayList2.ToArray(typeof (object)), requestID, out cancelID, out ppErrors);
        int[] int32s = OpcCom.Interop.GetInt32s(ref ppErrors, arrayList1.Count, true);
        for (int index = 0; index < arrayList1.Count; ++index)
        {
          IdentifiedResult identifiedResult = (IdentifiedResult) arrayList1[index];
          identifiedResult.ResultID = OpcCom.Interop.GetResultID(int32s[index]);
          identifiedResult.DiagnosticInfo = (string) null;
          if (int32s[index] == -1073479674)
            identifiedResultArray[index].ResultID = new ResultID(ResultID.Da.E_READONLY, -1073479674L);
        }
      }
      catch (Exception ex)
      {
        throw OpcCom.Interop.CreateException("IOPCAsyncIO2.Write", ex);
      }
      return identifiedResultArray;
    }
  }
}
