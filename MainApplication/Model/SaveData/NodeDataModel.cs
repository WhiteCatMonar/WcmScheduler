using System;
using System.Collections.Generic;

namespace MainApplication.Model.SaveData
{
    public class NodeDataModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "TaskNode";

        public PositionDataModel Position { get; set; } = new PositionDataModel();
        public NodeDetailsDataModel Details { get; set; } = new NodeDetailsDataModel();

        public List<PortDataModel> Ports { get; set; } = new List<PortDataModel>();
    }
}
