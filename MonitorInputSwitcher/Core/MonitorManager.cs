using System.Runtime.InteropServices;
using MonitorInputSwitcher.Models;

namespace MonitorInputSwitcher.Core
{
    /// <summary>
    /// Manages monitor detection, input switching, and DDC/CI communication
    /// </summary>
    public class MonitorManager : IDisposable
    {
        #region Win32 API Imports
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref Win32.DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, Win32.MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref Win32.MONITORINFOEX lpmi);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [In, Out] Win32.PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, Win32.PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte bVCPCode, out byte pvct, out uint pdwCurrentValue, out uint pdwMaximumValue);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint dwNewValue);
        #endregion

        private readonly Dictionary<string, IntPtr> _monitorHandles = new();
        private readonly Dictionary<string, string> _deviceNames = new();

        /// <summary>
        /// Gets all available monitors in the system
        /// </summary>
        public List<DisplayMonitor> GetAllMonitors()
        {
            BuildMonitorHandleMap();
            var monitors = new List<DisplayMonitor>();
            var deviceCounts = new Dictionary<string, int>();

            foreach (var screen in Screen.AllScreens)
            {
                var baseDescription = GetMonitorDescription(screen.DeviceName);
                
                // Handle duplicate names
                string friendlyName;
                if (deviceCounts.ContainsKey(baseDescription))
                {
                    deviceCounts[baseDescription]++;
                    friendlyName = $"{baseDescription} ({deviceCounts[baseDescription]})";
                }
                else
                {
                    deviceCounts[baseDescription] = 1;
                    friendlyName = $"{baseDescription} (1)";
                }

                var monitor = new DisplayMonitor(
                    screen.DeviceName,
                    friendlyName,
                    screen.Bounds,
                    screen.Primary,
                    _monitorHandles.GetValueOrDefault(screen.DeviceName, IntPtr.Zero)
                );

                _deviceNames[screen.DeviceName] = friendlyName;
                monitors.Add(monitor);
            }

            return monitors;
        }

        /// <summary>
        /// Gets current input source for a monitor
        /// </summary>
        public string? GetCurrentInput(DisplayMonitor monitor)
        {
            try
            {
                if (!_monitorHandles.TryGetValue(monitor.DeviceName, out var hMon) || hMon == IntPtr.Zero)
                    return null;

                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, out var count) || count == 0)
                    return null;

                var monitors = new Win32.PHYSICAL_MONITOR[count];
                if (!GetPhysicalMonitorsFromHMONITOR(hMon, count, monitors))
                    return null;

                string? result = null;
                foreach (var pm in monitors)
                {
                    if (GetVCPFeatureAndVCPFeatureReply(pm.hPhysicalMonitor, Win32.VCP_INPUT_SELECT, out _, out var current, out _))
                    {
                        result = InputType.GetFriendlyName(current);
                        break;
                    }
                }

                DestroyPhysicalMonitors(count, monitors);
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Switches monitor input source
        /// </summary>
        public async Task<bool> SwitchInputAsync(DisplayMonitor monitor, string inputType)
        {
            try
            {
                if (!InputType.IsValidInput(inputType))
                    return false;

                if (!_monitorHandles.TryGetValue(monitor.DeviceName, out var hMon) || hMon == IntPtr.Zero)
                    return false;

                var inputCode = InputType.GetInputCode(inputType);
                
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, out var count) || count == 0)
                    return false;

                var monitors = new Win32.PHYSICAL_MONITOR[count];
                if (!GetPhysicalMonitorsFromHMONITOR(hMon, count, monitors))
                    return false;

                bool success = false;
                foreach (var pm in monitors)
                {
                    if (SetVCPFeature(pm.hPhysicalMonitor, Win32.VCP_INPUT_SELECT, inputCode))
                    {
                        success = true;
                    }
                }

                DestroyPhysicalMonitors(count, monitors);
                
                if (success)
                {
                    await Task.Delay(1200); // Wait for monitor to switch
                }
                
                return success;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods
        private void BuildMonitorHandleMap()
        {
            _monitorHandles.Clear();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMon, hdc, rect, data) =>
            {
                var info = new Win32.MONITORINFOEX { cbSize = Marshal.SizeOf<Win32.MONITORINFOEX>() };
                if (GetMonitorInfo(hMon, ref info))
                {
                    _monitorHandles[info.szDevice] = hMon;
                }
                return true;
            }, IntPtr.Zero);
        }

        private string GetMonitorDescription(string deviceName)
        {
            try
            {
                var device = new Win32.DISPLAY_DEVICE { cb = Marshal.SizeOf<Win32.DISPLAY_DEVICE>() };
                uint i = 0;
                
                while (EnumDisplayDevices(null, i, ref device, 0))
                {
                    if (device.DeviceName == deviceName)
                    {
                        return string.IsNullOrWhiteSpace(device.DeviceString) 
                            ? deviceName.Replace("\\\\.\\", "") 
                            : device.DeviceString;
                    }
                    i++;
                }
                
                return deviceName.Replace("\\\\.\\", "");
            }
            catch
            {
                return deviceName.Replace("\\\\.\\", "");
            }
        }
        #endregion

        public void Dispose()
        {
            _monitorHandles.Clear();
            _deviceNames.Clear();
        }
    }
}