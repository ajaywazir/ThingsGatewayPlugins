

using OpcRcw.Comn;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class EnumUnknown : IEnumUnknown
  {
    private ArrayList m_unknowns = new ArrayList();
    private int m_index;

    internal EnumUnknown(ICollection unknowns)
    {
      if (unknowns == null)
        return;
      foreach (object unknown in (IEnumerable) unknowns)
        this.m_unknowns.Add(unknown);
    }

    public void Skip(int celt)
    {
      lock (this)
      {
        try
        {
          this.m_index += celt;
          if (this.m_index <= this.m_unknowns.Count)
            return;
          this.m_index = this.m_unknowns.Count;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Clone(out IEnumUnknown ppenum)
    {
      lock (this)
      {
        try
        {
          ppenum = (IEnumUnknown) new EnumUnknown((ICollection) this.m_unknowns);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Reset()
    {
      lock (this)
      {
        try
        {
          this.m_index = 0;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void RemoteNext(int celt, IntPtr rgelt, out int pceltFetched)
    {
      lock (this)
      {
        try
        {
          if (rgelt == IntPtr.Zero)
            throw new ExternalException("E_INVALIDARG", -2147024809);
          IntPtr[] source = new IntPtr[celt];
          pceltFetched = 0;
          if (this.m_index >= this.m_unknowns.Count)
            return;
          for (int index = 0; index < this.m_unknowns.Count - this.m_index && index < source.Length; ++index)
          {
            source[index] = Marshal.GetIUnknownForObject(this.m_unknowns[this.m_index + index]);
            ++pceltFetched;
          }
          this.m_index += pceltFetched;
          Marshal.Copy(source, 0, rgelt, pceltFetched);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }
  }
}
