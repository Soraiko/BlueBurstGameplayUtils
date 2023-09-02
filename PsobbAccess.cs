using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBurstGameplayUtils
{
    public static class PsobbAccess
    {
        public static System.Diagnostics.Process PSOBB;
        public static System.IntPtr PSOBB_Id;
        public static System.IntPtr old_PSOBB_Id = new System.IntPtr(-1);


        static PsobbAccess()
        {
            PSOBB_Id = IntPtr.Zero;
        }

        //Handle PSOBB
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern System.IntPtr OpenProcess(System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.Int32 dwProcessId);

        //Read from PSOBB
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = false)]
        public static extern System.Boolean ReadProcessMemory(System.IntPtr hProcess, System.IntPtr lpBaseAddress, [System.Runtime.InteropServices.Out] System.Byte[] lpBuffer, System.Int32 dwSize, out System.Int32 BytesRead);

        //Write to PSOBB
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern System.Boolean WriteProcessMemory(System.IntPtr hProcess, System.IntPtr lpBaseAddress, System.Byte[] lpBuffer, System.Int32 nSize, [System.Runtime.InteropServices.Out] System.Int32 lpNumberOfBytesWritten);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetWindowLong(System.IntPtr hWnd, int nIndex, int dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(System.IntPtr hWnd, int nIndex);

        const int GWL_STYLE = -16;
        const int WS_CAPTION = 0x00C00000;
        const int WS_SYSMENU = 0x00080000;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_MINIMIZEBOX = 0x00020000;
        const int WS_MAXIMIZEBOX = 0x00010000;


        public static bool SeekPSOBB()
        {
            if (PSOBB_Id != System.IntPtr.Zero)
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(PSOBB.Id);
                    if (process.HasExited == false && process.Id == PSOBB.Id)
                        return true;
                }
                catch { }
            }

            PSOBB_Id = System.IntPtr.Zero;
            var psobb = System.Diagnostics.Process.GetProcessesByName("psobb");

            if (psobb.Length > 0)
            {
                try
                {
                    PSOBB = psobb[0];
                    PSOBB_Id = OpenProcess(0x001F0FFF, true, PSOBB.Id);

                    if (false && old_PSOBB_Id != PSOBB_Id)
                    {
                        System.Threading.Thread.Sleep(1000);
                        var style = GetWindowLong(PSOBB.MainWindowHandle, GWL_STYLE);
                        style = style & ~WS_CAPTION;
                        style = style & ~WS_SYSMENU;
                        style = style & ~WS_THICKFRAME;
                        style = style & ~WS_MINIMIZEBOX;
                        style = style & ~WS_MAXIMIZEBOX;

                        SetWindowLong(PSOBB.MainWindowHandle, GWL_STYLE, style & ~WS_CAPTION);
                        ShowWindow(PSOBB.MainWindowHandle, 3);

                    }

                    old_PSOBB_Id = PSOBB_Id;
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        public static System.Int32 bytesread = 0;
        public static System.Byte[] RAMinputBuffer;

        public static void GetBuffer(System.Int64 address, System.Int32 size)
        {
            RAMinputBuffer = new System.Byte[size];
            ReadProcessMemory(PSOBB_Id, new System.IntPtr(address), RAMinputBuffer, size, out bytesread);
        }

        public static unsafe System.Int32 ReadIntRAM(System.Int64 address)
        {
            System.Int32 output = 0;
            GetBuffer(address, 4);
            fixed (System.Byte* ptr = &RAMinputBuffer[0])
                output = *(System.Int32*)(ptr);
            return output;
        }

        public static unsafe System.UInt32 ReadUIntRAM(System.Int64 address)
        {
            System.UInt32 output = 0;
            GetBuffer(address, 4);
            fixed (System.Byte* ptr = &RAMinputBuffer[0])
                output = *(System.UInt32*)(ptr);
            return output;
        }

        public static unsafe System.Int16 ReadShortRAM(System.Int64 address)
        {
            System.Int16 output = 0;
            GetBuffer(address, 2);
            fixed (System.Byte* ptr = &RAMinputBuffer[0])
                output = *(System.Int16*)(ptr);
            return output;
        }

        public static unsafe System.UInt16 ReadUShortRAM(System.Int64 address)
        {
            System.UInt16 output = 0;
            GetBuffer(address, 2);
            fixed (System.Byte* ptr = &RAMinputBuffer[0])
                output = *(System.UInt16*)(ptr);
            return output;
        }

        public static System.Byte[] ReadBytesRAM(System.Int64 address, System.Int32 length)
        {
            GetBuffer(address, length);
            return RAMinputBuffer;
        }

        public static System.Byte ReadByteRAM(System.Int64 address)
        {
            System.Byte output = 0;
            GetBuffer(address, 1);
            output = RAMinputBuffer[0];
            return output;
        }

        public static System.SByte ReadSByteRAM(System.Int64 address)
        {
            System.SByte output = 0;
            GetBuffer(address, 1);
            output = (System.SByte)RAMinputBuffer[0];
            return output;
        }

        public static System.Single ReadFloatRAM(System.Int64 address)
        {
            System.Single output = 0f;
            GetBuffer(address, 4);
            output = System.BitConverter.ToSingle(RAMinputBuffer, 0);
            return output;
        }

        public static System.Byte[] SubArray(System.Byte[] data, System.Int64 index, System.Int64 length)
        {
            System.Byte[] result = new System.Byte[length];
            System.Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static System.String ReadASCIIRAM(System.Int64 address, System.Int32 length)
        {
            System.String output = "";
            GetBuffer(address, length);
            for (System.Int32 i = 0; i < RAMinputBuffer.Length; i++)
                if (RAMinputBuffer[i] < 33 || RAMinputBuffer[i] > 126) RAMinputBuffer = SubArray(RAMinputBuffer, 0, i);
            output = System.Text.Encoding.ASCII.GetString(RAMinputBuffer);
            return output;
        }


        public static void WriteIntRAM(System.Int64 address, System.Int32 valeur)
        {
            System.Byte[] valeurBytes = System.BitConverter.GetBytes(valeur);
            WriteProcessMemory(PSOBB_Id, new System.IntPtr(address), valeurBytes, 4, 0);
        }

        public static void WriteUIntRAM(System.Int64 address, System.UInt32 valeur)
        {
            System.Byte[] valeurBytes = System.BitConverter.GetBytes(valeur);
            WriteProcessMemory(PSOBB_Id, new System.IntPtr(address), valeurBytes, 4, 0);
        }

        public static void WriteShortRAM(System.Int64 address, System.Int16 valeur)
        {
            System.Byte[] valeurBytes = System.BitConverter.GetBytes(valeur);
            WriteProcessMemory(PSOBB_Id, new System.IntPtr(address), valeurBytes, 2, 0);
        }

        public static void WriteUShortRAM(System.Int64 address, System.UInt16 valeur)
        {
            System.Byte[] valeurBytes = System.BitConverter.GetBytes(valeur);
            WriteProcessMemory(PSOBB_Id, new System.IntPtr(address), valeurBytes, 2, 0);
        }

        public static void WriteFloatRAM(System.Int64 address, System.Single valeur)
        {
            System.Byte[] valeurBytes = System.BitConverter.GetBytes(valeur);
            WriteProcessMemory(PSOBB_Id, new System.IntPtr(address), valeurBytes, 4, 0);
        }
    }
}
