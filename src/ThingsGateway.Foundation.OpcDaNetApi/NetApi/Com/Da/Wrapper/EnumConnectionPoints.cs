

using OpcRcw.Comn;
using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
  public class EnumConnectionPoints : IEnumConnectionPoints
  {
    private ArrayList m_connectionPoints = new ArrayList();
    private int m_index;

    internal EnumConnectionPoints(ICollection connectionPoints)
    {
      if (connectionPoints == null)
        return;
      foreach (IConnectionPoint connectionPoint in (IEnumerable) connectionPoints)
        this.m_connectionPoints.Add((object) connectionPoint);
    }

    public void Skip(int cConnections)
    {
      lock (this)
      {
        try
        {
          this.m_index += cConnections;
          if (this.m_index <= this.m_connectionPoints.Count)
            return;
          this.m_index = this.m_connectionPoints.Count;
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }

    public void Clone(out IEnumConnectionPoints ppenum)
    {
      lock (this)
      {
        try
        {
          ppenum = (IEnumConnectionPoints) new EnumConnectionPoints((ICollection) this.m_connectionPoints);
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

    public void RemoteNext(int cConnections, IntPtr ppCP, out int pcFetched)
    {
      lock (this)
      {
        try
        {
          if (ppCP == IntPtr.Zero)
            throw new ExternalException("E_INVALIDARG", -2147024809);
          IntPtr[] source = new IntPtr[cConnections];
          pcFetched = 0;
          if (this.m_index >= this.m_connectionPoints.Count)
            return;
          for (int index = 0; index < this.m_connectionPoints.Count - this.m_index && index < cConnections; ++index)
          {
            IConnectionPoint connectionPoint = (IConnectionPoint) this.m_connectionPoints[this.m_index + index];
            source[index] = Marshal.GetComInterfaceForObject((object) connectionPoint, typeof (IConnectionPoint));
            ++pcFetched;
          }
          this.m_index += pcFetched;
          Marshal.Copy(source, 0, ppCP, pcFetched);
        }
        catch (Exception ex)
        {
          throw Server.CreateException(ex);
        }
      }
    }
  }
}
