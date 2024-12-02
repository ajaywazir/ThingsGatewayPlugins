

using OpcRcw.Comn;

using System;
using System.Collections;
using System.Runtime.InteropServices;


namespace OpcCom.Da.Wrapper
{
    public class EnumString : IEnumString
    {
        private ArrayList m_strings = new ArrayList();
        private int m_index;

        internal EnumString(ICollection strings)
        {
            if (strings == null)
                return;
            foreach (object obj in (IEnumerable)strings)
                m_strings.Add(obj);
        }

        public void Skip(int celt)
        {
            lock (this)
            {
                try
                {
                    m_index += celt;
                    if (m_index <= m_strings.Count)
                        return;
                    m_index = m_strings.Count;
                }
                catch (Exception ex)
                {
                    throw Server.CreateException(ex);
                }
            }
        }

        public void Clone(out IEnumString ppenum)
        {
            lock (this)
            {
                try
                {
                    ppenum = (IEnumString)new EnumString((ICollection)m_strings);
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
                    if (m_index >= m_strings.Count)
                        return;
                    for (int index = 0; index < m_strings.Count - m_index && index < source.Length; ++index)
                    {
                        source[index] = Marshal.StringToCoTaskMemUni((string)m_strings[m_index + index]);
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
