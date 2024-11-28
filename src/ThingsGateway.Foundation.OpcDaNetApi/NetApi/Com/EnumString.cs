

using OpcRcw.Comn;
using System;
using System.Runtime.InteropServices;


namespace OpcCom
{
  public class EnumString : IDisposable
  {
    private IEnumString m_enumerator;

    public EnumString(object enumerator) => this.m_enumerator = (IEnumString) enumerator;

    public void Dispose()
    {
      Interop.ReleaseServer((object) this.m_enumerator);
      this.m_enumerator = (IEnumString) null;
    }

    public string[] Next(int count)
    {
      try
      {
        IntPtr pArray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (IntPtr)) * count);
        try
        {
          int pceltFetched = 0;
          this.m_enumerator.RemoteNext(count, pArray, out pceltFetched);
          return pceltFetched == 0 ? new string[0] : Interop.GetUnicodeStrings(ref pArray, pceltFetched, true);
        }
        finally
        {
          Marshal.FreeCoTaskMem(pArray);
        }
      }
      catch (Exception ex)
      {
        return (string[]) null;
      }
    }

    public void Skip(int count) => this.m_enumerator.Skip(count);

    public void Reset() => this.m_enumerator.Reset();

    public EnumString Clone()
    {
      IEnumString ppenum = (IEnumString) null;
      this.m_enumerator.Clone(out ppenum);
      return new EnumString((object) ppenum);
    }
  }
}
