using System;

namespace MainApplication.Model.SaveData
{
    public class ConnectionDataModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromPortId { get; set; }
        public string ToPortId { get; set; }
    }
}
