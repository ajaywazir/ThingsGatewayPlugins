

using Opc;

using OpcRcw.Comn;

using System.Collections;
using System.Net;
using System.Runtime.InteropServices;


namespace OpcCom
{
    public class ServerEnumerator2 : IDiscovery, IDisposable
    {
        private IOPCServerList2 m_server;
        private string m_host;
        private const string ProgID = "OPC.ServerList.2";
        private static readonly Guid CLSID = new Guid("13486D51-4821-11D2-A494-3CB306C10000");

        public void Dispose()
        {
        }

        public string[] EnumerateHosts() => Interop.EnumComputers();

        public Opc.Server[] GetAvailableServers(Specification specification)
        {
            return GetAvailableServers(specification, (string)null, (ConnectData)null);
        }

        public Opc.Server[] GetAvailableServers(
          Specification specification,
          string host,
          ConnectData connectData)
        {
            lock (this)
            {
                NetworkCredential credential = connectData?.GetCredential((Uri)null, (string)null);
                m_server = (IOPCServerList2)Interop.CreateInstance(ServerEnumerator2.CLSID, host, credential);
                if (m_server == null)
                    throw new("GetOpcServer failed, please check if OPC runtime is installed");
                m_host = host;
                try
                {
                    ArrayList arrayList = new ArrayList();
                    Guid guid = new Guid(specification.ID);
                    IOPCEnumGUID ppenumClsid = (IOPCEnumGUID)null;
                    m_server.EnumClassesOfCategories(1, new Guid[1]
                    {
            guid
                    }, 0, (Guid[])null, out ppenumClsid);

                    Guid[] guidArray = ServerEnumerator2.ReadClasses(ppenumClsid);
                    Interop.ReleaseServer((object)ppenumClsid);
                    foreach (Guid clsid in guidArray)
                    {
                        Factory factory = new Factory();
                        try
                        {
                            URL url = CreateUrl(specification, clsid);
                            Opc.Server server = (Opc.Server)null;
                            if (specification == Specification.COM_DA_30)
                                server = (Opc.Server)new Opc.Da.Server((Opc.Factory)factory, url);
                            else if (specification == Specification.COM_DA_20)
                                server = (Opc.Server)new Opc.Da.Server((Opc.Factory)factory, url);
                            else if (specification == Specification.COM_DA_10)
                                server = (Opc.Server)new Opc.Da.Server((Opc.Factory)factory, url);
                            else if (specification == Specification.COM_AE_10)
                                server = (Opc.Server)new Opc.Ae.Server((Opc.Factory)factory, url);
                            else if (specification == Specification.COM_HDA_10)
                                server = (Opc.Server)new Opc.Hda.Server((Opc.Factory)factory, url);
                            else if (specification == Specification.COM_DX_10)
                                server = (Opc.Server)new Opc.Dx.Server((Opc.Factory)factory, url);
                            arrayList.Add((object)server);
                        }
                        catch
                        {
                        }
                    }
                    return (Opc.Server[])arrayList.ToArray(typeof(Opc.Server));
                }
                finally
                {
                    Interop.ReleaseServer((object)m_server);
                    m_server = (IOPCServerList2)null;
                }
            }
        }

        public Guid CLSIDFromProgID(string progID, string host, ConnectData connectData)
        {
            lock (this)
            {
                NetworkCredential credential = connectData?.GetCredential((Uri)null, (string)null);
                m_server = (IOPCServerList2)Interop.CreateInstance(ServerEnumerator2.CLSID, host, credential);
                m_host = host;
                Guid clsid;
                try
                {
                    m_server.CLSIDFromProgID(progID, out clsid);
                }
                catch
                {
                    clsid = Guid.Empty;
                }
                finally
                {
                    Interop.ReleaseServer((object)m_server);
                    m_server = (IOPCServerList2)null;
                }
                return clsid;
            }
        }

        private static Guid[] ReadClasses(IOPCEnumGUID enumerator)
        {
            ArrayList arrayList = new ArrayList();
            int pceltFetched = 0;
            int celt = 10;
            IntPtr num = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * celt);
            try
            {
                do
                {
                    try
                    {
                        enumerator.Next(celt, num, out pceltFetched);
                        IntPtr ptr = num;
                        for (int index = 0; index < pceltFetched; ++index)
                        {
                            Guid structure = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
                            arrayList.Add((object)structure);
                            ptr = (nint)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(Guid)));
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                while (pceltFetched > 0);
                return (Guid[])arrayList.ToArray(typeof(Guid));
            }
            finally
            {
                Marshal.FreeCoTaskMem(num);
            }
        }

        private URL CreateUrl(Specification specification, Guid clsid)
        {
            URL url = new URL();
            url.HostName = m_host;
            url.Port = 0;
            url.Path = (string)null;
            if (specification == Specification.COM_DA_30)
                url.Scheme = "opcda";
            else if (specification == Specification.COM_DA_20)
                url.Scheme = "opcda";
            else if (specification == Specification.COM_DA_10)
                url.Scheme = "opcda";
            else if (specification == Specification.COM_DX_10)
                url.Scheme = "opcdx";
            else if (specification == Specification.COM_AE_10)
                url.Scheme = "opcae";
            else if (specification == Specification.COM_HDA_10)
                url.Scheme = "opchda";
            else if (specification == Specification.COM_BATCH_10)
                url.Scheme = "opcbatch";
            else if (specification == Specification.COM_BATCH_20)
                url.Scheme = "opcbatch";
            try
            {
                string ppszProgID = (string)null;
                string ppszUserType = (string)null;
                string ppszVerIndProgID = (string)null;
                m_server.GetClassDetails(ref clsid, out ppszProgID, out ppszUserType, out ppszVerIndProgID);
                if (ppszVerIndProgID != null)
                    url.Path = string.Format("{0}/{1}", (object)ppszVerIndProgID, (object)("{" + clsid.ToString() + "}"));
                else if (ppszProgID != null)
                    url.Path = string.Format("{0}/{1}", (object)ppszProgID, (object)("{" + clsid.ToString() + "}"));
            }
            catch
            {
            }
            finally
            {
                if (url.Path == null)
                    url.Path = string.Format("{0}", (object)("{" + clsid.ToString() + "}"));
            }
            return url;
        }
    }
}
