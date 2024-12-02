

using Opc;
using Opc.Dx;

using OpcRcw.Dx;

using System;


namespace OpcCom.Dx
{
    [Serializable]
    public class Server : OpcCom.Da.Server, Opc.Dx.IServer, Opc.Da.IServer, Opc.IServer, IDisposable
    {
        public Server(URL url, object server)
          : base(url, server)
        {
        }

        public Opc.Dx.SourceServer[] GetSourceServers()
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    int pdwCount = 0;
                    IntPtr ppServers = IntPtr.Zero;
                    ((IOPCConfiguration)m_server).GetServers(out pdwCount, out ppServers);
                    return Interop.GetSourceServers(ref ppServers, pdwCount, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.GetServers", ex);
                }
            }
        }

        public GeneralResponse AddSourceServers(Opc.Dx.SourceServer[] servers)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.SourceServer[] sourceServers = Interop.GetSourceServers(servers);
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).AddServers(sourceServers.Length, sourceServers, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.AddServers", ex);
                }
            }
        }

        public GeneralResponse ModifySourceServers(Opc.Dx.SourceServer[] servers)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.SourceServer[] sourceServers = Interop.GetSourceServers(servers);
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).ModifyServers(sourceServers.Length, sourceServers, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.ModifyServers", ex);
                }
            }
        }

        public GeneralResponse DeleteSourceServers(Opc.Dx.ItemIdentifier[] servers)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.ItemIdentifier[] itemIdentifiers = Interop.GetItemIdentifiers(servers);
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).DeleteServers(itemIdentifiers.Length, itemIdentifiers, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.DeleteServers", ex);
                }
            }
        }

        public GeneralResponse CopyDefaultSourceServerAttributes(
          bool configToStatus,
          Opc.Dx.ItemIdentifier[] servers)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.ItemIdentifier[] itemIdentifiers = Interop.GetItemIdentifiers(servers);
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).CopyDefaultServerAttributes(configToStatus ? 1 : 0, itemIdentifiers.Length, itemIdentifiers, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.CopyDefaultServerAttributes", ex);
                }
            }
        }

        public Opc.Dx.DXConnection[] QueryDXConnections(
          string browsePath,
          Opc.Dx.DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pDXConnectionMasks = Interop.GetDXConnections(connectionMasks) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    int pdwCount = 0;
                    IntPtr ppErrors = IntPtr.Zero;
                    IntPtr ppConnections = IntPtr.Zero;
                    ((IOPCConfiguration)m_server).QueryDXConnections(browsePath != null ? browsePath : "", pDXConnectionMasks.Length, pDXConnectionMasks, recursive ? 1 : 0, out ppErrors, out pdwCount, out ppConnections);
                    errors = Interop.GetResultIDs(ref ppErrors, pDXConnectionMasks.Length, true);
                    return Interop.GetDXConnections(ref ppConnections, pdwCount, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.QueryDXConnections", ex);
                }
            }
        }

        public GeneralResponse AddDXConnections(Opc.Dx.DXConnection[] connections)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pConnections = Interop.GetDXConnections(connections) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).AddDXConnections(pConnections.Length, pConnections, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.AddDXConnections", ex);
                }
            }
        }

        public GeneralResponse ModifyDXConnections(Opc.Dx.DXConnection[] connections)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pDXConnectionDefinitions = Interop.GetDXConnections(connections) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).ModifyDXConnections(pDXConnectionDefinitions.Length, pDXConnectionDefinitions, out pResponse);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.ModifyDXConnections", ex);
                }
            }
        }

        public GeneralResponse UpdateDXConnections(
          string browsePath,
          Opc.Dx.DXConnection[] connectionMasks,
          bool recursive,
          Opc.Dx.DXConnection connectionDefinition,
          out ResultID[] errors)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pDXConnectionMasks = Interop.GetDXConnections(connectionMasks) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    OpcRcw.Dx.DXConnection dxConnection = Interop.GetDXConnection(connectionDefinition);
                    IntPtr ppErrors = IntPtr.Zero;
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).UpdateDXConnections(browsePath != null ? browsePath : "", pDXConnectionMasks.Length, pDXConnectionMasks, recursive ? 1 : 0, ref dxConnection, out ppErrors, out pResponse);
                    errors = Interop.GetResultIDs(ref ppErrors, pDXConnectionMasks.Length, true);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.UpdateDXConnections", ex);
                }
            }
        }

        public GeneralResponse DeleteDXConnections(
          string browsePath,
          Opc.Dx.DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pDXConnectionMasks = Interop.GetDXConnections(connectionMasks) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    IntPtr ppErrors = IntPtr.Zero;
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).DeleteDXConnections(browsePath != null ? browsePath : "", pDXConnectionMasks.Length, pDXConnectionMasks, recursive ? 1 : 0, out ppErrors, out pResponse);
                    errors = Interop.GetResultIDs(ref ppErrors, pDXConnectionMasks.Length, true);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.DeleteDXConnections", ex);
                }
            }
        }

        public GeneralResponse CopyDXConnectionDefaultAttributes(
          bool configToStatus,
          string browsePath,
          Opc.Dx.DXConnection[] connectionMasks,
          bool recursive,
          out ResultID[] errors)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    OpcRcw.Dx.DXConnection[] pDXConnectionMasks = Interop.GetDXConnections(connectionMasks) ?? Array.Empty<OpcRcw.Dx.DXConnection>();
                    IntPtr ppErrors = IntPtr.Zero;
                    DXGeneralResponse pResponse;
                    ((IOPCConfiguration)m_server).CopyDXConnectionDefaultAttributes(configToStatus ? 1 : 0, browsePath != null ? browsePath : "", pDXConnectionMasks.Length, pDXConnectionMasks, recursive ? 1 : 0, out ppErrors, out pResponse);
                    errors = Interop.GetResultIDs(ref ppErrors, pDXConnectionMasks.Length, true);
                    return Interop.GetGeneralResponse(pResponse, true);
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.CopyDXConnectionDefaultAttributes", ex);
                }
            }
        }

        public string ResetConfiguration(string configurationVersion)
        {
            lock (this)
            {
                if (m_server == null)
                    throw new NotConnectedException();
                try
                {
                    string pszConfigurationVersion = (string)null;
                    ((IOPCConfiguration)m_server).ResetConfiguration(configurationVersion, out pszConfigurationVersion);
                    return pszConfigurationVersion;
                }
                catch (Exception ex)
                {
                    throw OpcCom.Interop.CreateException("IOPCConfiguration.ResetConfiguration", ex);
                }
            }
        }
    }
}
