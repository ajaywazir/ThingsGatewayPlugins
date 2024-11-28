

using Opc;
using Opc.Ae;
using Opc.Da;
using OpcRcw.Ae;
using System;
using System.Runtime.InteropServices;


namespace OpcCom.Ae
{
  public class Interop
  {
    internal static OpcRcw.Ae.FILETIME Convert(OpcCom.FILETIME input)
    {
      return new OpcRcw.Ae.FILETIME()
      {
        dwLowDateTime = input.dwLowDateTime,
        dwHighDateTime = input.dwHighDateTime
      };
    }

    internal static OpcCom.FILETIME Convert(OpcRcw.Ae.FILETIME input)
    {
      return new OpcCom.FILETIME()
      {
        dwLowDateTime = input.dwLowDateTime,
        dwHighDateTime = input.dwHighDateTime
      };
    }

    internal static ResultID GetResultID(int input)
    {
      return input == -1073479165 ? ResultID.Ae.E_INVALIDBRANCHNAME : OpcCom.Interop.GetResultID(input);
    }

    internal static Opc.Ae.ServerStatus GetServerStatus(ref IntPtr pInput, bool deallocate)
    {
      Opc.Ae.ServerStatus serverStatus = (Opc.Ae.ServerStatus) null;
      if (pInput != IntPtr.Zero)
      {
        OPCEVENTSERVERSTATUS structure = (OPCEVENTSERVERSTATUS) Marshal.PtrToStructure(pInput, typeof (OPCEVENTSERVERSTATUS));
        serverStatus = new Opc.Ae.ServerStatus();
        serverStatus.VendorInfo = structure.szVendorInfo;
        serverStatus.ProductVersion = string.Format("{0}.{1}.{2}", (object) structure.wMajorVersion, (object) structure.wMinorVersion, (object) structure.wBuildNumber);
        serverStatus.ServerState = (ServerState) structure.dwServerState;
        serverStatus.StatusInfo = (string) null;
        serverStatus.StartTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftStartTime));
        serverStatus.CurrentTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftCurrentTime));
        serverStatus.LastUpdateTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftLastUpdateTime));
        if (deallocate)
        {
          Marshal.DestroyStructure(pInput, typeof (OPCEVENTSERVERSTATUS));
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return serverStatus;
    }

    internal static OPCAEBROWSETYPE GetBrowseType(BrowseType input)
    {
      return input == BrowseType.Area || input != BrowseType.Source ? OPCAEBROWSETYPE.OPC_AREA : OPCAEBROWSETYPE.OPC_SOURCE;
    }

    internal static EventNotification[] GetEventNotifications(ONEVENTSTRUCT[] input)
    {
      EventNotification[] eventNotifications = (EventNotification[]) null;
      if (input != null && input.Length != 0)
      {
        eventNotifications = new EventNotification[input.Length];
        for (int index = 0; index < input.Length; ++index)
          eventNotifications[index] = Interop.GetEventNotification(input[index]);
      }
      return eventNotifications;
    }

    internal static EventNotification GetEventNotification(ONEVENTSTRUCT input)
    {
      EventNotification eventNotification = new EventNotification();
      eventNotification.SourceID = input.szSource;
      eventNotification.Time = OpcCom.Interop.GetFILETIME(Interop.Convert(input.ftTime));
      eventNotification.Severity = input.dwSeverity;
      eventNotification.Message = input.szMessage;
      eventNotification.EventType = (EventType) input.dwEventType;
      eventNotification.EventCategory = input.dwEventCategory;
      eventNotification.ChangeMask = (int) input.wChangeMask;
      eventNotification.NewState = (int) input.wNewState;
      eventNotification.Quality = new Quality(input.wQuality);
      eventNotification.ConditionName = input.szConditionName;
      eventNotification.SubConditionName = input.szSubconditionName;
      eventNotification.AckRequired = input.bAckRequired != 0;
      eventNotification.ActiveTime = OpcCom.Interop.GetFILETIME(Interop.Convert(input.ftActiveTime));
      eventNotification.Cookie = input.dwCookie;
      eventNotification.ActorID = input.szActorID;
      eventNotification.SetAttributes(OpcCom.Interop.GetVARIANTs(ref input.pEventAttributes, input.dwNumEventAttrs, false));
      return eventNotification;
    }

    internal static Condition[] GetConditions(ref IntPtr pInput, int count, bool deallocate)
    {
      Condition[] conditions = (Condition[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        conditions = new Condition[count];
        IntPtr ptr = pInput;
        for (int index1 = 0; index1 < count; ++index1)
        {
          OPCCONDITIONSTATE structure = (OPCCONDITIONSTATE) Marshal.PtrToStructure(ptr, typeof (OPCCONDITIONSTATE));
          conditions[index1] = new Condition();
          conditions[index1].State = (int) structure.wState;
          conditions[index1].Quality = new Quality(structure.wQuality);
          conditions[index1].Comment = structure.szComment;
          conditions[index1].AcknowledgerID = structure.szAcknowledgerID;
          conditions[index1].CondLastActive = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftCondLastActive));
          conditions[index1].CondLastInactive = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftCondLastInactive));
          conditions[index1].SubCondLastActive = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftSubCondLastActive));
          conditions[index1].LastAckTime = OpcCom.Interop.GetFILETIME(Interop.Convert(structure.ftLastAckTime));
          conditions[index1].ActiveSubCondition.Name = structure.szActiveSubCondition;
          conditions[index1].ActiveSubCondition.Definition = structure.szASCDefinition;
          conditions[index1].ActiveSubCondition.Severity = structure.dwASCSeverity;
          conditions[index1].ActiveSubCondition.Description = structure.szASCDescription;
          string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref structure.pszSCNames, structure.dwNumSCs, deallocate);
          int[] int32s1 = OpcCom.Interop.GetInt32s(ref structure.pdwSCSeverities, structure.dwNumSCs, deallocate);
          string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref structure.pszSCDefinitions, structure.dwNumSCs, deallocate);
          string[] unicodeStrings3 = OpcCom.Interop.GetUnicodeStrings(ref structure.pszSCDescriptions, structure.dwNumSCs, deallocate);
          conditions[index1].SubConditions.Clear();
          if (structure.dwNumSCs > 0)
          {
            for (int index2 = 0; index2 < unicodeStrings1.Length; ++index2)
              conditions[index1].SubConditions.Add((object) new SubCondition()
              {
                Name = unicodeStrings1[index2],
                Severity = int32s1[index2],
                Definition = unicodeStrings2[index2],
                Description = unicodeStrings3[index2]
              });
          }
          object[] varianTs = OpcCom.Interop.GetVARIANTs(ref structure.pEventAttributes, structure.dwNumEventAttrs, deallocate);
          int[] int32s2 = OpcCom.Interop.GetInt32s(ref structure.pErrors, structure.dwNumEventAttrs, deallocate);
          conditions[index1].Attributes.Clear();
          if (structure.dwNumEventAttrs > 0)
          {
            for (int index3 = 0; index3 < varianTs.Length; ++index3)
              conditions[index1].Attributes.Add((object) new AttributeValue()
              {
                ID = 0,
                Value = varianTs[index3],
                ResultID = Interop.GetResultID(int32s2[index3])
              });
          }
          if (deallocate)
            Marshal.DestroyStructure(ptr, typeof (OPCCONDITIONSTATE));
          ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (OPCCONDITIONSTATE)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return conditions;
    }
  }
}
