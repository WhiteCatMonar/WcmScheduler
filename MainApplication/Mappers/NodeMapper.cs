using MainApplication.Models.SaveData;
using MainApplication.ViewModels;
using System;
using System.Linq;
using static MainApplication.ViewModels.PortViewModel;

namespace MainApplication.Mappers
{
    public static class NodeMapper
    {
        /* ---------------------------------------------------------
         * ViewModel → DataModel
         * --------------------------------------------------------- */
        public static NodeDataModel ToDataModel(NodeViewModel vm)
        {
            return new NodeDataModel
            {
                Id = vm.NodeGuid.ToString(),
                Type = vm.NodeType,

                Position = new PositionDataModel
                {
                    X = vm.X,
                    Y = vm.Y
                },

                Details = new NodeDetailsDataModel
                {
                    TaskName = vm.TaskName,
                    Person = vm.Person,
                    StartDateTime = vm.StartDateTime,
                    EndDateTime = vm.EndDateTime,
                    Comment = vm.Comment
                },

                Ports = vm.AllPorts.ToList().Select(p => new PortDataModel
                {
                    Id = p.PortGuid.ToString(),
                    Name = p.Name,
                    Type = p.Type.ToString()
                }).ToList()
            };
        }

        /* ---------------------------------------------------------
         * DataModel → ViewModel
         * --------------------------------------------------------- */
        public static NodeViewModel ToViewModel(NodeDataModel data, NodeEditorViewModel editor)
        {
            NodeViewModel loadedNode = new NodeViewModel(editor.UndoRedo, editor.DateTimeEditor)
            {
                NodeGuid = Guid.Parse(data.Id),
                NodeType = data.Type,
                X = data.Position.X,
                Y = data.Position.Y,
                TaskName = data.Details.TaskName,
                Person = data.Details.Person,
                StartDateTime = data.Details.StartDateTime,
                EndDateTime = data.Details.EndDateTime,
                Comment = data.Details.Comment
            };
            loadedNode.CommitEdits();
            foreach (var port in data.Ports)
            {
                switch ((PortType)Enum.Parse(typeof(PortType), port.Type))
                {
                    case PortType.Input:
                        loadedNode.InputPorts.Add(new PortViewModel
                        {
                            PortGuid = Guid.Parse(port.Id),
                            Name = port.Name,
                            Type = PortType.Input,
                            ParentNode = loadedNode
                        });
                        break;
                    case PortType.Output:
                        loadedNode.OutputPorts.Add(new PortViewModel
                        {
                            PortGuid = Guid.Parse(port.Id),
                            Name = port.Name,
                            Type = PortType.Output,
                            ParentNode = loadedNode
                        });
                        break;
                }
            }
            return loadedNode;
        }
    }

}
