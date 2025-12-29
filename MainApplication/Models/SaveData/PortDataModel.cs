using System;

namespace MainApplication.Models.SaveData
{
    public class PortDataModel
    {
        /* ---------------------------------------------------------
         * データプロパティ(ポート情報)
         * --------------------------------------------------------- */

        /// <summary>
        /// ポートの一意識別子
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ポート名(例：In,Out,Triggerなど)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ポート種別(Input/Outputなど)
        /// </summary>
        public string Type { get; set; }
    }
}

/* --- End of file --- */
