using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;

namespace RBXMSEAPI.Classes
{
    public static class fluxteam_net_api
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inhert_handle, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("Fluxteam_net_API.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool run_script(IntPtr proc, int pid, string path, [MarshalAs(UnmanagedType.LPWStr)] string script);

        [DllImport("Fluxteam_net_API.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool is_injected(IntPtr proc, int pid, string path);

        [DllImport("Fluxteam_net_API.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool inject_dll(int pid, [MarshalAs(UnmanagedType.LPWStr)] string script);

        // Token: 0x06000012 RID: 18 RVA: 0x00002164 File Offset: 0x00000364
        private static fluxteam_net_api.Result r_inject(string dll_path)
        {
            FileInfo fileInfo = new FileInfo(dll_path);
            FileSecurity accessControl = fileInfo.GetAccessControl();
            SecurityIdentifier identity = new SecurityIdentifier("S-1-15-2-1");
            accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            fileInfo.SetAccessControl(accessControl);
            Process[] processesByName = Process.GetProcessesByName("Windows10Universal");
            if (processesByName.Length == 0)
            {
                return fluxteam_net_api.Result.ProcNotOpen;
            }
            uint num = 0U;
            while ((ulong)num < (ulong)((long)processesByName.Length))
            {
                Process process = processesByName[(int)num];
                if (fluxteam_net_api.pid != process.Id)
                {
                    IntPtr intPtr = fluxteam_net_api.OpenProcess(1082U, false, process.Id);
                    if (intPtr == fluxteam_net_api.NULL)
                    {
                        return fluxteam_net_api.Result.OpenProcFail;
                    }
                    IntPtr intPtr2 = fluxteam_net_api.VirtualAllocEx(intPtr, fluxteam_net_api.NULL, (IntPtr)((dll_path.Length + 1) * Marshal.SizeOf(typeof(char))), 12288U, 64U);
                    if (intPtr2 == fluxteam_net_api.NULL)
                    {
                        return fluxteam_net_api.Result.AllocFail;
                    }
                    byte[] bytes = Encoding.Default.GetBytes(dll_path);
                    int num2 = fluxteam_net_api.WriteProcessMemory(intPtr, intPtr2, bytes, (IntPtr)((dll_path.Length + 1) * Marshal.SizeOf(typeof(char))), 0);
                    if (num2 == 0 || (long)num2 == 6L)
                    {
                        return fluxteam_net_api.Result.Unknown;
                    }
                    if (fluxteam_net_api.CreateRemoteThread(intPtr, fluxteam_net_api.NULL, fluxteam_net_api.NULL, fluxteam_net_api.GetProcAddress(fluxteam_net_api.GetModuleHandle("kernel32.dll"), "LoadLibraryA"), intPtr2, 0U, fluxteam_net_api.NULL) == fluxteam_net_api.NULL)
                    {
                        return fluxteam_net_api.Result.LoadLibFail;
                    }
                    fluxteam_net_api.pid = process.Id;
                    fluxteam_net_api.phandle = intPtr;
                    return fluxteam_net_api.Result.Success;
                }
                else
                {
                    if (fluxteam_net_api.pid == process.Id)
                    {
                        return fluxteam_net_api.Result.AlreadyInjected;
                    }
                    num += 1U;
                }
            }
            return fluxteam_net_api.Result.Unknown;
        }
        public static fluxteam_net_api.Result inject_custom()
        {
            fluxteam_net_api.Result result;
            try
            {
                if (!File.Exists(fluxteam_net_api.dll_path))
                {
                    result = fluxteam_net_api.Result.DLLNotFound;
                }
                else
                {
                    result = fluxteam_net_api.r_inject(fluxteam_net_api.dll_path);
                }
            }
            catch
            {
                result = fluxteam_net_api.Result.Unknown;
            }
            return result;
        }
        public static void inject()
        {
            switch (fluxteam_net_api.inject_custom())
            {
                case fluxteam_net_api.Result.DLLNotFound:
                    MessageBox.Show("dll not found\n", "Injection");
                    return;
                case fluxteam_net_api.Result.OpenProcFail:
                    MessageBox.Show("OpenProcFail failed\n", "Injection");
                    return;
                case fluxteam_net_api.Result.AllocFail:
                    MessageBox.Show("AllocFail failed\n", "Injection");
                    return;
                case fluxteam_net_api.Result.LoadLibFail:
                    MessageBox.Show("LoadLibFail failed\n", "Injection");
                    return;
                case fluxteam_net_api.Result.AlreadyInjected:
                    break;
                case fluxteam_net_api.Result.ProcNotOpen:
                    MessageBox.Show("Failure to find UWP game\nmake sure you are using microsoft store", "Injection");
                    return;
                case fluxteam_net_api.Result.Unknown:
                    MessageBox.Show("Unknown\n", "Injection");
                    break;
                default:
                    return;
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000023C9 File Offset: 0x000005C9
        public static bool is_injected(int pid)
        {
            fluxteam_net_api.phandle = fluxteam_net_api.OpenProcess(1082U, false, pid);
            return fluxteam_net_api.is_injected(fluxteam_net_api.phandle, pid, fluxteam_net_api.dll_path);
        }

        // Token: 0x06000016 RID: 22 RVA: 0x000023EC File Offset: 0x000005EC
        public static bool run_script(int pid, string script)
        {
            fluxteam_net_api.pid = pid;
            fluxteam_net_api.phandle = fluxteam_net_api.OpenProcess(1082U, false, pid);
            if (pid == 0)
            {
                MessageBox.Show("press Inject first", "api Error");
                return false;
            }
            if (script == string.Empty)
            {
                return fluxteam_net_api.is_injected(pid);
            }
            return fluxteam_net_api.run_script(fluxteam_net_api.phandle, pid, fluxteam_net_api.dll_path, script);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x0000244C File Offset: 0x0000064C
        public static void create_files(string dll_path_)
        {
            if (!File.Exists(dll_path_))
            {
                MessageBox.Show("Failure to initalize Fluxteam.net api\nDLL path was invalid", "Error");
                Environment.Exit(0);
            }
            fluxteam_net_api.dll_path = dll_path_;
            string text = "";
            foreach (string text2 in Directory.GetDirectories(Environment.GetEnvironmentVariable("LocalAppData") + "\\Packages"))
            {
                if (text2.Contains("OBLOXCORPORATION"))
                {
                    if (Directory.GetDirectories(text2 + "\\AC").Any((string dir) => dir.Contains("Temp")))
                    {
                        text = text2 + "\\AC";
                    }
                }
            }
            if (text == "")
            {
                return;
            }
            try
            {
                if (Directory.Exists("workspace"))
                {
                    Directory.Move("workspace", "old_workspace");
                }
                if (Directory.Exists("autoexec"))
                {
                    Directory.Move("autoexec", "old_autoexec");
                }
            }
            catch
            {
            }
            string text3 = Path.Combine(text, "workspace");
            string text4 = Path.Combine(text, "autoexec");
            if (!Directory.Exists(text3))
            {
                Directory.CreateDirectory(text3);
            }
            if (!Directory.Exists(text4))
            {
                Directory.CreateDirectory(text4);
            }
        }
        public static string dll_path;
        public static IntPtr phandle;
        public static int pid = 0;
        private static readonly IntPtr NULL = (IntPtr)0;
        public enum Result : uint
        {
            Success,
            DLLNotFound,
            OpenProcFail,
            AllocFail,
            LoadLibFail,
            AlreadyInjected,
            ProcNotOpen,
            Unknown
        }
    }
}
