

using System;
using System.Collections;


namespace Opc.Dx
{
    [Serializable]
    public class DXConnectionQuery
    {
        private string m_name;
        private string m_browsePath;
        private DXConnectionCollection m_masks = new DXConnectionCollection();
        private bool m_recursive;

        public string Name
        {
            get => m_name;
            set => m_name = value;
        }

        public string BrowsePath
        {
            get => m_browsePath;
            set => m_browsePath = value;
        }

        public bool Recursive
        {
            get => m_recursive;
            set => m_recursive = value;
        }

        public DXConnectionCollection Masks => m_masks;

        public DXConnection[] Query(Server server, out ResultID[] errors)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            return server.QueryDXConnections(BrowsePath, Masks.ToArray(), Recursive, out errors);
        }

        public GeneralResponse Update(
          Server server,
          DXConnection connectionDefinition,
          out ResultID[] errors)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            return server.UpdateDXConnections(BrowsePath, Masks.ToArray(), Recursive, connectionDefinition, out errors);
        }

        public GeneralResponse Delete(Server server, out ResultID[] errors)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            return server.DeleteDXConnections(BrowsePath, Masks.ToArray(), Recursive, out errors);
        }

        public GeneralResponse CopyDefaultAttributes(
          Server server,
          bool configToStatus,
          out ResultID[] errors)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            return server.CopyDXConnectionDefaultAttributes(configToStatus, BrowsePath, Masks.ToArray(), Recursive, out errors);
        }

        public DXConnectionQuery()
        {
        }

        public DXConnectionQuery(DXConnectionQuery query)
        {
            if (query == null)
                return;
            Name = query.Name;
            BrowsePath = query.BrowsePath;
            Recursive = query.Recursive;
            m_masks = new DXConnectionCollection((ICollection)query.Masks);
        }

        public virtual object Clone() => (object)new DXConnectionQuery(this);
    }
}
