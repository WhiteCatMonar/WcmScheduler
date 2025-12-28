using System;

namespace MainApplication.Models.SaveData
{
    public class NodeDetailsDataModel
    {
        public string TaskName { get; set; }
        public string Person { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string Comment { get; set; }
    }
}
