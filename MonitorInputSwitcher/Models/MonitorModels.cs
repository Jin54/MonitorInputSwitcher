using System.Runtime.InteropServices;

namespace MonitorInputSwitcher.Models
{
    /// <summary>
    /// Represents a display monitor with its properties and capabilities
    /// </summary>
    public class DisplayMonitor
    {
        public string DeviceName { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public bool IsPrimary { get; set; }
        public IntPtr Handle { get; set; }
        
        public DisplayMonitor(string deviceName, string friendlyName, Rectangle bounds, bool isPrimary, IntPtr handle)
        {
            DeviceName = deviceName;
            FriendlyName = friendlyName;
            Bounds = bounds;
            IsPrimary = isPrimary;
            Handle = handle;
        }
    }
    
    /// <summary>
    /// Input type definitions and mappings
    /// </summary>
    public static class InputType
    {
        public static readonly Dictionary<string, uint> InputMap = new()
        {
            {"DP", 0x0F},
            {"DP2", 0x10}, 
            {"HDMI", 0x11},
            {"HDMI1", 0x11},
            {"HDMI2", 0x12}
        };
        
        public static IEnumerable<string> GetAllInputs() => InputMap.Keys;
        
        public static bool IsValidInput(string input) => InputMap.ContainsKey(input);
        
        public static uint GetInputCode(string input) => InputMap.TryGetValue(input, out var code) ? code : 0;
        
        public static string GetFriendlyName(uint code)
        {
            return (code & 0xFF) switch
            {
                0x0F => "DP",
                0x10 => "DP2", 
                0x11 => "HDMI",
                0x12 => "HDMI2",
                0x01 => "VGA",
                0x03 => "DVI",
                _ => $"0x{code & 0xFF:X2}"
            };
        }
    }
    
    /// <summary>
    /// Win32 API structures and constants
    /// </summary>
    public static class Win32
    {
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x00000001;
        public const int CDS_SET_PRIMARY = 0x00000010;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const byte VCP_INPUT_SELECT = 0x60;
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] 
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
            public string DeviceKey;
        }

        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);
    }
}