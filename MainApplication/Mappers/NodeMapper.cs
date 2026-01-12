using MainApplication.Models.SaveData;
using MainApplication.ViewModels.ProjectModel;
using System.Windows;
using static MainApplication.ViewModels.ProjectModel.PortViewModel;

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
                    X = vm.Position.X,
                    Y = vm.Position.Y
                },

                Details = new NodeDetailsDataModel
                {
                    TaskName = vm.Detail.TaskName,
                    Person = vm.Detail.Person,
                    StartDateTime = vm.Detail.StartDateTime,
                    EndDateTime = vm.Detail.EndDateTime,
                    Comment = vm.Detail.Comment
                },

                Ports = 
                [.. 
                    vm.AllPorts.ToList().Select(p => new PortDataModel
                    {
                        Id = p.PortGuid.ToString(),
                        Name = p.Name,
                        Type = p.Type.ToString()
                    })
                ]
            };
        }

        /* ---------------------------------------------------------
         * DataModel → ViewModel
         * --------------------------------------------------------- */
        public static NodeViewModel ToViewModel(NodeDataModel data, NodeEditorViewModel editor)
        {
            NodeViewModel loadedNode = new(editor.UndoRedo, editor.DateTimeEditor)
            {
                NodeGuid = Guid.Parse(data.Id),
                NodeType = data.Type,
                Position = new Point(data.Position.X, data.Position.Y)
            };
            loadedNode.Detail.TaskName = data.Details.TaskName;
            loadedNode.Detail.Person = data.Details.Person;
            loadedNode.Detail.StartDateTime = data.Details.StartDateTime;
            loadedNode.Detail.EndDateTime = data.Details.EndDateTime;
            loadedNode.Detail.Comment = data.Details.Comment;
            loadedNode.Detail.CommitEdits();
            foreach (var port in data.Ports)
            {
                var type = Enum.Parse<PortType>(port.Type);
                var vmPort = new PortViewModel
                {
                    PortGuid = Guid.Parse(port.Id),
                    Name = port.Name,
                    Type = type
                };

                switch (type)
                {
                    case PortType.Input:
                        loadedNode.InputPorts.Add(vmPort);
                        break;
                    case PortType.Output:
                        loadedNode.OutputPorts.Add(vmPort);
                        break;
                }

                if (!editor.NodePorts.TryGetValue(loadedNode, out var list))
                {
                    list = [];
                    editor.NodePorts[loadedNode] = list;
                }
                list.Add(vmPort);
            }
            return loadedNode;
        }
    }

}

/* --- End of file --- */
