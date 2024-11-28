

using OpcRcw.Comn;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom
{
  public class EnumGuid
  {
    private IEnumGUID m_enumerator;

    public EnumGuid(object server) => this.m_enumerator = (IEnumGUID) server;

    public void Release()
    {
      Interop.ReleaseServer((object) this.m_enumerator);
      this.m_enumerator = (IEnumGUID) null;
    }

    public object GetEnumerator() => (object) this.m_enumerator;

    public Guid[] GetAll()
    {
      this.Reset();
      ArrayList arrayList = new ArrayList();
      while (true)
      {
        Guid[] c = this.Next(1);
        if (c != null)
          arrayList.AddRange((ICollection) c);
        else
          break;
      }
      return (Guid[]) arrayList.ToArray(typeof (Guid));
    }

    public Guid[] Next(int count)
    {
      IntPtr num = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (Guid)) * count);
      try
      {
        int pceltFetched = 0;
        try
        {
          this.m_enumerator.Next(count, num, out pceltFetched);
        }
        catch (Exception ex)
        {
          return (Guid[]) null;
        }
        if (pceltFetched == 0)
          return (Guid[]) null;
        IntPtr ptr = num;
        Guid[] guidArray = new Guid[pceltFetched];
        for (int index = 0; index < pceltFetched; ++index)
        {
          guidArray[index] = (Guid) Marshal.PtrToStructure(ptr, typeof (Guid));
          ptr = (IntPtr) (ptr.ToInt64() + (long) Marshal.SizeOf(typeof (Guid)));
        }
        return guidArray;
      }
      finally
      {
        Marshal.FreeCoTaskMem(num);
      }
    }

    public void Skip(int count) => this.m_enumerator.Skip(count);

    public void Reset() => this.m_enumerator.Reset();

    public EnumGuid Clone()
    {
      IEnumGUID ppenum = (IEnumGUID) null;
      this.m_enumerator.Clone(out ppenum);
      return new EnumGuid((object) ppenum);
    }
  }
}
