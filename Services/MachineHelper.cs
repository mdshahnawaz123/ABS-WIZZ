using Microsoft.Win32;
using System;

namespace Asset.Services
{
    public static class MachineHelper
    {
        public static string GetMachineId()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid") as string;
                }
            }
            catch { }

            return Environment.MachineName;
        }
    }
}
