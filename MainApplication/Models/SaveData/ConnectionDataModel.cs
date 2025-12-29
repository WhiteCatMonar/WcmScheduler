using System;

namespace MainApplication.Models.SaveData
{
    public class ConnectionDataModel
    {
        /* ---------------------------------------------------------
         * データプロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線自体のID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 接続元ポートのID
        /// </summary>
        public string FromPortId { get; set; }

        /// <summary>
        /// 接続先ポートのID
        /// </summary>
        public string ToPortId { get; set; }
    }
}

/* --- End of file --- */
