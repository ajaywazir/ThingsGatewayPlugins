

using Opc;

using OpcRcw.Comn;

using System.Collections;
using System.Net;
using System.Runtime.InteropServices;


namespace OpcCom
{
    public class ServerEnumerator1 : IDiscovery, IDisposable
    {
        private IOPCServerList m_server;
        private string m_host;
        private const string ProgID = "OPC.ServerList";
        private static readonly Guid CLSID = new Guid("13486D51-4821-11D2-A494-3CB306C10000");

        public void Dispose()
        {
        }

        public string[] EnumerateHosts() => Interop.EnumComputers();

        public Opc.Server[] GetAvailableServers(Specification specification)
        {
            return this.GetAvailableServers(specification, (string)null, (ConnectData)null);
        }

        public Opc.Server[] GetAvailableServers(
          Specification specification,
          string host,
          ConnectData connectData)
        {
            lock (this)
            {
                NetworkCredential credential = connectData?.GetCredential((Uri)null, (string)null);
                this.m_server = (IOPCServerList)Interop.CreateInstance(ServerEnumerator1.CLSID, host, credential);
                this.m_host = host;
                try
                {
                    ArrayList arrayList = new ArrayList();
                    Guid guid = new Guid(specification.ID);
                    object ppenumClsid = null;
                    this.m_server.EnumClassesOfCategories(1, new Guid[1]
                    {
            guid
                    }, 0, (Guid[])null, out ppenumClsid);
                    Guid[] guidArray = this.ReadClasses((IEnumGUID)ppenumClsid);
                    Interop.ReleaseServer((object)ppenumClsid);
                    foreach (Guid clsid in guidArray)
                    {
                        Factory factory = new Factory();
                        try
                        {
                            URL url = this.CreateUrl(specification, clsid);
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
                        catch (Exception ex)
                        {
                        }
                    }
                    return (Opc.Server[])arrayList.ToArray(typeof(Opc.Server));
                }
                finally
                {
                    Interop.ReleaseServer((object)this.m_server);
                    this.m_server = (IOPCServerList)null;
                }
            }
        }

        public Guid CLSIDFromProgID(string progID, string host, ConnectData connectData)
        {
            lock (this)
            {
                NetworkCredential credential = connectData?.GetCredential((Uri)null, (string)null);
                this.m_server = (IOPCServerList)Interop.CreateInstance(ServerEnumerator1.CLSID, host, credential);
                this.m_host = host;
                Guid clsid;
                try
                {
                    this.m_server.CLSIDFromProgID(progID, out clsid);
                }
                catch
                {
                    clsid = Guid.Empty;
                }
                finally
                {
                    Interop.ReleaseServer((object)this.m_server);
                    this.m_server = (IOPCServerList)null;
                }
                return clsid;
            }
        }

        private Guid[] ReadClasses(IEnumGUID enumerator)
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
                            ptr = (IntPtr)(ptr.ToInt64() + (long)Marshal.SizeOf(typeof(Guid)));
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
            url.HostName = this.m_host;
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
                this.m_server.GetClassDetails(ref clsid, out ppszProgID, out ppszUserType);
                if (ppszProgID != null)
                    url.Path = string.Format("{0}/{1}", (object)ppszProgID, (object)("{" + clsid.ToString() + "}"));
            }
            catch (Exception ex)
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
