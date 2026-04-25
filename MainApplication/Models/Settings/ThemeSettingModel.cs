namespace MainApplication.Models.Settings
{
    public class ThemeSettingModel
    {
        public string Name { get; set; } = "Default";
        public Dictionary<string, string> Colors { get; set; } = new (){
            {"node-background", "#FFFFFFFF" }
        };
    }
}

/* --- End of file --- */
