using MainApplication.Models.SaveData;
using MainApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApplication.Mappers
{
    public static class ConnectionMapper
    {
        /* ---------------------------------------------------------
         * ViewModel → DataModel
         * --------------------------------------------------------- */
        public static ConnectionDataModel ToDataModel(ConnectionViewModel vm)
        {
            return new ConnectionDataModel
            {
                Id = vm.ConnectionGuid.ToString(),
                FromPortId = vm.FromPort.PortGuid.ToString(),
                ToPortId = vm.ToPort.PortGuid.ToString()
            };
        }

        /* ---------------------------------------------------------
         * DataModel → ViewModel
         * --------------------------------------------------------- */
        public static ConnectionViewModel ToViewModel(ConnectionDataModel data, NodeEditorViewModel editor)
        {
            PortViewModel from = null;
            PortViewModel to = null;

            foreach (NodeViewModel node in editor.Nodes.Nodes)
            {
                foreach (PortViewModel port in node.AllPorts)
                {
                    if (port.PortGuid.ToString() == data.FromPortId)
                    {
                        from = port;
                    }
                    if (port.PortGuid.ToString() == data.ToPortId)
                    {
                        to = port;
                    }
                }
            }
            if ((from == null) || (to == null))
            {
                return null;
            }
            ConnectionViewModel loadedConnection = new ConnectionViewModel(from, to, editor);
            loadedConnection.ConnectionGuid = Guid.Parse(data.Id);
            return loadedConnection;
        }
    }

}
