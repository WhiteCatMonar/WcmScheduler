using System.Collections.Generic;

namespace MainApplication.Models.SaveData
{
    public class TaskEditorSaveData
    {
        public List<NodeDataModel> Nodes { get; set; } = new List<NodeDataModel>();
        public List<ConnectionDataModel> Connections { get; set; } = new List<ConnectionDataModel>();
    }
}
