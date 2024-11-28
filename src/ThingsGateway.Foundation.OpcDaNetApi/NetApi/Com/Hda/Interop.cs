

using Opc.Hda;
using OpcRcw.Hda;
using System;
using System.Runtime.InteropServices;


namespace OpcCom.Hda
{
  public class Interop
  {
    internal static OPCHDA_FILETIME Convert(FILETIME input)
    {
      return new OPCHDA_FILETIME()
      {
        dwLowDateTime = input.dwLowDateTime,
        dwHighDateTime = input.dwHighDateTime
      };
    }

    internal static FILETIME Convert(OPCHDA_FILETIME input)
    {
      return new FILETIME()
      {
        dwLowDateTime = input.dwLowDateTime,
        dwHighDateTime = input.dwHighDateTime
      };
    }

    internal static OPCHDA_FILETIME GetFILETIME(Decimal input)
    {
      return new OPCHDA_FILETIME()
      {
        dwHighDateTime = (int) (((ulong) (input * 10000000M) & 18446744069414584320UL) >> 32),
        dwLowDateTime = (int) ((long) (ulong) (input * 10000000M) & (long) uint.MaxValue)
      };
    }

    internal static OPCHDA_FILETIME[] GetFILETIMEs(DateTime[] input)
    {
      OPCHDA_FILETIME[] filetimEs = (OPCHDA_FILETIME[]) null;
      if (input != null)
      {
        filetimEs = new OPCHDA_FILETIME[input.Length];
        for (int index = 0; index < input.Length; ++index)
          filetimEs[index] = Interop.Convert(OpcCom.Interop.GetFILETIME(input[index]));
      }
      return filetimEs;
    }

    internal static OPCHDA_TIME GetTime(Time input)
    {
      OPCHDA_TIME time = new OPCHDA_TIME();
      if (input != null)
      {
        time.ftTime = Interop.Convert(OpcCom.Interop.GetFILETIME(input.AbsoluteTime));
        time.szTime = input.IsRelative ? input.ToString() : "";
        time.bString = input.IsRelative ? 1 : 0;
      }
      else
      {
        time.ftTime = Interop.Convert(OpcCom.Interop.GetFILETIME(DateTime.MinValue));
        time.szTime = "";
        time.bString = 1;
      }
      return time;
    }

    internal static ItemValueCollection[] GetItemValueCollections(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      ItemValueCollection[] valueCollections = (ItemValueCollection[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        valueCollections = new ItemValueCollection[count];
        IntPtr pInput1 = pInput;
        for (int index = 0; index < count; ++index)
        {
          valueCollections[index] = Interop.GetItemValueCollection(pInput1, deallocate);
          pInput1 = (IntPtr) (pInput1.ToInt64() + (long) Marshal.SizeOf(typeof (OPCHDA_ITEM)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return valueCollections;
    }

    internal static ItemValueCollection GetItemValueCollection(IntPtr pInput, bool deallocate)
    {
      ItemValueCollection itemValueCollection = (ItemValueCollection) null;
      if (pInput != IntPtr.Zero)
      {
        itemValueCollection = Interop.GetItemValueCollection((OPCHDA_ITEM) Marshal.PtrToStructure(pInput, typeof (OPCHDA_ITEM)), deallocate);
        if (deallocate)
          Marshal.DestroyStructure(pInput, typeof (OPCHDA_ITEM));
      }
      return itemValueCollection;
    }

    internal static ItemValueCollection GetItemValueCollection(OPCHDA_ITEM input, bool deallocate)
    {
      ItemValueCollection itemValueCollection = new ItemValueCollection();
      itemValueCollection.ClientHandle = (object) input.hClient;
      itemValueCollection.AggregateID = input.haAggregate;
      object[] varianTs = OpcCom.Interop.GetVARIANTs(ref input.pvDataValues, input.dwCount, deallocate);
      DateTime[] filetimEs = OpcCom.Interop.GetFILETIMEs(ref input.pftTimeStamps, input.dwCount, deallocate);
      int[] int32s = OpcCom.Interop.GetInt32s(ref input.pdwQualities, input.dwCount, deallocate);
      for (int index = 0; index < input.dwCount; ++index)
        itemValueCollection.Add(new Opc.Hda.ItemValue()
        {
          Value = varianTs[index],
          Timestamp = filetimEs[index],
          Quality = new Opc.Da.Quality((short) (int32s[index] & (int) ushort.MaxValue)),
          HistorianQuality = (Opc.Hda.Quality) ((long) int32s[index] & 4294901760L)
        });
      return itemValueCollection;
    }

    internal static ModifiedValueCollection[] GetModifiedValueCollections(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      ModifiedValueCollection[] valueCollections = (ModifiedValueCollection[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        valueCollections = new ModifiedValueCollection[count];
        IntPtr pInput1 = pInput;
        for (int index = 0; index < count; ++index)
        {
          valueCollections[index] = Interop.GetModifiedValueCollection(pInput1, deallocate);
          pInput1 = (IntPtr) (pInput1.ToInt64() + (long) Marshal.SizeOf(typeof (OPCHDA_MODIFIEDITEM)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return valueCollections;
    }

    internal static ModifiedValueCollection GetModifiedValueCollection(
      IntPtr pInput,
      bool deallocate)
    {
      ModifiedValueCollection modifiedValueCollection = (ModifiedValueCollection) null;
      if (pInput != IntPtr.Zero)
      {
        modifiedValueCollection = Interop.GetModifiedValueCollection((OPCHDA_MODIFIEDITEM) Marshal.PtrToStructure(pInput, typeof (OPCHDA_MODIFIEDITEM)), deallocate);
        if (deallocate)
          Marshal.DestroyStructure(pInput, typeof (OPCHDA_MODIFIEDITEM));
      }
      return modifiedValueCollection;
    }

    internal static ModifiedValueCollection GetModifiedValueCollection(
      OPCHDA_MODIFIEDITEM input,
      bool deallocate)
    {
      ModifiedValueCollection modifiedValueCollection = new ModifiedValueCollection();
      modifiedValueCollection.ClientHandle = (object) input.hClient;
      object[] varianTs = OpcCom.Interop.GetVARIANTs(ref input.pvDataValues, input.dwCount, deallocate);
      DateTime[] filetimEs1 = OpcCom.Interop.GetFILETIMEs(ref input.pftTimeStamps, input.dwCount, deallocate);
      int[] int32s1 = OpcCom.Interop.GetInt32s(ref input.pdwQualities, input.dwCount, deallocate);
      DateTime[] filetimEs2 = OpcCom.Interop.GetFILETIMEs(ref input.pftModificationTime, input.dwCount, deallocate);
      int[] int32s2 = OpcCom.Interop.GetInt32s(ref input.pEditType, input.dwCount, deallocate);
      string[] unicodeStrings = OpcCom.Interop.GetUnicodeStrings(ref input.szUser, input.dwCount, deallocate);
      for (int index = 0; index < input.dwCount; ++index)
      {
        ModifiedValue modifiedValue = new ModifiedValue();
        modifiedValue.Value = varianTs[index];
        modifiedValue.Timestamp = filetimEs1[index];
        modifiedValue.Quality = new Opc.Da.Quality((short) (int32s1[index] & (int) ushort.MaxValue));
        modifiedValue.HistorianQuality = (Opc.Hda.Quality) ((long) int32s1[index] & 4294901760L);
        modifiedValue.ModificationTime = filetimEs2[index];
        modifiedValue.EditType = (EditType) int32s2[index];
        modifiedValue.User = unicodeStrings[index];
        modifiedValueCollection.Add((Opc.Hda.ItemValue) modifiedValue);
      }
      return modifiedValueCollection;
    }

    internal static AttributeValueCollection[] GetAttributeValueCollections(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      AttributeValueCollection[] valueCollections = (AttributeValueCollection[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        valueCollections = new AttributeValueCollection[count];
        IntPtr pInput1 = pInput;
        for (int index = 0; index < count; ++index)
        {
          valueCollections[index] = Interop.GetAttributeValueCollection(pInput1, deallocate);
          pInput1 = (IntPtr) (pInput1.ToInt64() + (long) Marshal.SizeOf(typeof (OPCHDA_ATTRIBUTE)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return valueCollections;
    }

    internal static AttributeValueCollection GetAttributeValueCollection(
      IntPtr pInput,
      bool deallocate)
    {
      AttributeValueCollection attributeValueCollection = (AttributeValueCollection) null;
      if (pInput != IntPtr.Zero)
      {
        attributeValueCollection = Interop.GetAttributeValueCollection((OPCHDA_ATTRIBUTE) Marshal.PtrToStructure(pInput, typeof (OPCHDA_ATTRIBUTE)), deallocate);
        if (deallocate)
          Marshal.DestroyStructure(pInput, typeof (OPCHDA_ATTRIBUTE));
      }
      return attributeValueCollection;
    }

    internal static AttributeValueCollection GetAttributeValueCollection(
      OPCHDA_ATTRIBUTE input,
      bool deallocate)
    {
      AttributeValueCollection attributeValueCollection = new AttributeValueCollection();
      attributeValueCollection.AttributeID = input.dwAttributeID;
      object[] varianTs = OpcCom.Interop.GetVARIANTs(ref input.vAttributeValues, input.dwNumValues, deallocate);
      DateTime[] filetimEs = OpcCom.Interop.GetFILETIMEs(ref input.ftTimeStamps, input.dwNumValues, deallocate);
      for (int index = 0; index < input.dwNumValues; ++index)
        attributeValueCollection.Add(new AttributeValue()
        {
          Value = varianTs[index],
          Timestamp = filetimEs[index]
        });
      return attributeValueCollection;
    }

    internal static AnnotationValueCollection[] GetAnnotationValueCollections(
      ref IntPtr pInput,
      int count,
      bool deallocate)
    {
      AnnotationValueCollection[] valueCollections = (AnnotationValueCollection[]) null;
      if (pInput != IntPtr.Zero && count > 0)
      {
        valueCollections = new AnnotationValueCollection[count];
        IntPtr pInput1 = pInput;
        for (int index = 0; index < count; ++index)
        {
          valueCollections[index] = Interop.GetAnnotationValueCollection(pInput1, deallocate);
          pInput1 = (IntPtr) (pInput1.ToInt64() + (long) Marshal.SizeOf(typeof (OPCHDA_ANNOTATION)));
        }
        if (deallocate)
        {
          Marshal.FreeCoTaskMem(pInput);
          pInput = IntPtr.Zero;
        }
      }
      return valueCollections;
    }

    internal static AnnotationValueCollection GetAnnotationValueCollection(
      IntPtr pInput,
      bool deallocate)
    {
      AnnotationValueCollection annotationValueCollection = (AnnotationValueCollection) null;
      if (pInput != IntPtr.Zero)
      {
        annotationValueCollection = Interop.GetAnnotationValueCollection((OPCHDA_ANNOTATION) Marshal.PtrToStructure(pInput, typeof (OPCHDA_ANNOTATION)), deallocate);
        if (deallocate)
          Marshal.DestroyStructure(pInput, typeof (OPCHDA_ANNOTATION));
      }
      return annotationValueCollection;
    }

    internal static AnnotationValueCollection GetAnnotationValueCollection(
      OPCHDA_ANNOTATION input,
      bool deallocate)
    {
      AnnotationValueCollection annotationValueCollection = new AnnotationValueCollection();
      annotationValueCollection.ClientHandle = (object) input.hClient;
      DateTime[] filetimEs1 = OpcCom.Interop.GetFILETIMEs(ref input.ftTimeStamps, input.dwNumValues, deallocate);
      string[] unicodeStrings1 = OpcCom.Interop.GetUnicodeStrings(ref input.szAnnotation, input.dwNumValues, deallocate);
      DateTime[] filetimEs2 = OpcCom.Interop.GetFILETIMEs(ref input.ftAnnotationTime, input.dwNumValues, deallocate);
      string[] unicodeStrings2 = OpcCom.Interop.GetUnicodeStrings(ref input.szUser, input.dwNumValues, deallocate);
      for (int index = 0; index < input.dwNumValues; ++index)
        annotationValueCollection.Add(new AnnotationValue()
        {
          Timestamp = filetimEs1[index],
          Annotation = unicodeStrings1[index],
          CreationTime = filetimEs2[index],
          User = unicodeStrings2[index]
        });
      return annotationValueCollection;
    }
  }
}
