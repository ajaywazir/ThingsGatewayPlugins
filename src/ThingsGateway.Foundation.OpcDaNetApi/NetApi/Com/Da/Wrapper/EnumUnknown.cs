

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
            foreach (object unknown in (IEnumerable)unknowns)
                m_unknowns.Add(unknown);
        }

        public void Skip(int celt)
        {
            lock (this)
            {
                try
                {
                    m_index += celt;
                    if (m_index <= m_unknowns.Count)
                        return;
                    m_index = m_unknowns.Count;
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
                    ppenum = (IEnumUnknown)new EnumUnknown((ICollection)m_unknowns);
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
                    m_index = 0;
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
                    if (m_index >= m_unknowns.Count)
                        return;
                    for (int index = 0; index < m_unknowns.Count - m_index && index < source.Length; ++index)
                    {
                        source[index] = Marshal.GetIUnknownForObject(m_unknowns[m_index + index]);
                        ++pceltFetched;
                    }
                    m_index += pceltFetched;
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
