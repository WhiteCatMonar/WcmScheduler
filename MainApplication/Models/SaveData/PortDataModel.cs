using System;

namespace MainApplication.Models.SaveData
{
    public class PortDataModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
