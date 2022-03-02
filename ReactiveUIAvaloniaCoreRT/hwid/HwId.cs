using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using libc.hwid.Helpers;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.COINITBASE;
using static TerraFX.Interop.Windows.CLSCTX;
using System.Runtime.InteropServices;
namespace libc.hwid
{
    public unsafe static class HwId
    {
        public static string Generate()
        {
            var res = new[]
            {
                GetInfo(Hardware.Cpuid),
                GetInfo(Hardware.Motherboard),
                                GetInfo(Hardware.SMBIOS)

            };

            var input = string.Join("\n", res);
            var result = Hash(input);

            return result;
        }

        private static string Hash(string input)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash) // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }
        }

        private static string Wmi(string wmiClass, string wmiProperty)
        {
    //        var hresult = CoInitializeEx(null, (uint)COINITBASE_MULTITHREADED);
    //        if (FAILED(hresult))
    //            return "";
    //        hresult = CoInitializeSecurity(
    //    null,
    //    -1,
    //    null,
    //    null,
    //    0,
    //    3,
    //    null,
    //    0,
    //    null
    //);
    //        if (FAILED(hresult))
    //            return "";
            IWbemLocator* pLoc = null;
            var hresult = CoCreateInstance(
        __uuidof<WbemLocator>(),
        null,
        (uint)CLSCTX_INPROC_SERVER,
        __uuidof<IWbemLocator>(),
        (void**)(&pLoc)
    );
            if (FAILED(hresult))
                return "";
            IWbemServices* pServ = null;
            hresult = pLoc->ConnectServer(
                    (ushort*)Marshal.StringToBSTR("ROOT\\CIMV2").ToPointer(),
        null,
        null,
        null,
        0,
        null,
        null,
        &pServ
    );
            if (FAILED(hresult))
                return "";
            hresult = CoSetProxyBlanket(
        (IUnknown*)pServ,
        10,
        0,
        null,
        3,
        3,
        null,
        0
    );
            if (FAILED(hresult))
                return "";
            IEnumWbemClassObject* pEnumerator = null;
            hresult = pServ->ExecQuery(
                (ushort*)Marshal.StringToBSTR("WQL").ToPointer(),
                (ushort*)Marshal.StringToBSTR($"SELECT * FROM {wmiClass}").ToPointer(),
                (int)(WBEM_GENERIC_FLAG_TYPE.WBEM_FLAG_FORWARD_ONLY | WBEM_GENERIC_FLAG_TYPE.WBEM_FLAG_RETURN_IMMEDIATELY),
                null,
                &pEnumerator
            );
            if (FAILED(hresult))
                return "";
            IWbemClassObject* pclsObj = null;
            uint uReturn = 0;

            string result = "";

            while (pEnumerator != null)
            {
                HRESULT hr =
                    pEnumerator->Next((int)WBEM_TIMEOUT_TYPE.WBEM_INFINITE, 1, &pclsObj, &uReturn);
                if (uReturn == 0)
                    break;
                VARIANT vtProp;

                fixed (char* lpString = wmiProperty)
                    hr = pclsObj->Get((ushort*)lpString, 0, &vtProp, null, null);
                string manufacturer = Marshal.PtrToStringBSTR((IntPtr)vtProp.bstrVal);

                //hr = pclsObj->Get(L"Product", 0, &vtProp, 0, 0);
                //string product = W2A(vtProp.bstrVal);
                //hr = pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
                //string serialNumber = W2A(vtProp.bstrVal);

                result = manufacturer;

                VariantClear(&vtProp);

                pclsObj->Release();
                break;
            }
            pServ->Release();
            pLoc->Release();
            pEnumerator->Release();
            //CoUninitialize();
            return result;
        }

        private static string Dmidecode(string query, string find)
        {
            var cmd = new Cmd();

            var k = cmd.Run("/usr/bin/sudo", $" {query}", new CmdOptions
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStdOut = true,
                UseOsShell = false
            }, true);

            find = find.EndsWith(":") ? find : $"{find}:";

            var lines = k.Output.Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim(' ', '\t'));

            var line = lines.First(a => a.StartsWith(find));
            var res = line.Substring(line.IndexOf(find, StringComparison.Ordinal) + find.Length).Trim(' ', '\t');

            return res;
        }

        private static string GetIoregOutput(string node)
        {
            var proc = new Process();

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/sh"
            };

            var command = @"/usr/sbin/ioreg -rd1 -c IOPlatformExpertDevice | awk -F'\""' '/" + node +
                          "/{ print $(NF-1) }'";

            psi.Arguments = $"-c \"{command}\"";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            string result = null;
            proc.StartInfo = psi;

            proc.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    result = e.Data;
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();

            return result;
        }

        private static string GetInfo(Hardware hw)
        {
            switch (hw)
            {
                case Hardware.Motherboard when AppInfo.IsLinux:
                    {
                        var result = Dmidecode("dmidecode -t 2", "Manufacturer");

                        return result;
                    }
                case Hardware.Motherboard when AppInfo.IsWindows:
                    return Wmi("Win32_BaseBoard", "Manufacturer");
                case Hardware.Motherboard when AppInfo.IsMacOs:
                    var macSerial = GetIoregOutput("IOPlatformSerialNumber");

                    return macSerial;
                case Hardware.Cpuid when AppInfo.IsLinux:
                    {
                        var res = Dmidecode("dmidecode -t 4", "ID");
                        var parts = res.Split(' ').Reverse();
                        var result = string.Join("", parts);

                        return result;
                    }
                case Hardware.SMBIOS when AppInfo.IsWindows:
                    return Wmi("Win32_ComputerSystemProduct", "UUID");
                case Hardware.Cpuid when AppInfo.IsWindows:
                    // We try by asm but fallback with wmi if it fails.
                    var asmCpuId = Asm.GetProcessorId();

                    return asmCpuId?.Length > 2 ? asmCpuId : Wmi("Win32_Processor", "ProcessorId");
                case Hardware.Cpuid when AppInfo.IsMacOs:
                    var uuid = GetIoregOutput("IOPlatformUUID");

                    return uuid;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private enum Hardware
        {
            Motherboard,
            Cpuid,
            SMBIOS
        }
    }
}