namespace ABS_WIZZ
{
    public static class ABS_Params
    {
        // Room & Element Data Parameters
        public const string L1_ASSET = "(01)ECD_ABS_L1_Asset";
        public const string L2_LEVEL = "(04)ECD_ABS_L2_Level";
        public const string L3_ROOM = "(05)ECD_ABS_L3_Room";
        
        // Target Parameter for Room Code / Unique Number
        public const string L7_ONSITE_TAG = "(17)ECD_ABS_L7_Onsite_Equipment_Tag";
        
        // Equipment Type Identification
        public const string EQUIP_TYPE_PARAM = "(04)ECD_ABS_L6_Equipment_Type";
    }

    public static class ABS_Logger
    {
        private static System.Collections.Generic.List<string> _logs = new System.Collections.Generic.List<string>();

        public static void Log(string message)
        {
            _logs.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void Clear() => _logs.Clear();

        public static string GetReport() => string.Join("\n", _logs);
    }
}
