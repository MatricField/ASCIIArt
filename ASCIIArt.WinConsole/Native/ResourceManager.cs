using System;
using System.Runtime.InteropServices;
using static ASCIIArt.WinConsole.Native.Kernel32;

namespace ASCIIArt.WinConsole.Native
{
    internal class ResourceManager
    {
        public static byte[] GetResourceFromExecutable(string lpFileName, string lpName, string lpType)
        {
            IntPtr hModule = LoadLibrary(lpFileName);
            if (hModule != IntPtr.Zero)
            {
                IntPtr hResource = FindResource(hModule, lpName, lpType);
                if (hResource != IntPtr.Zero)
                {
                    uint resSize = SizeofResource(hModule, hResource);
                    IntPtr resData = LoadResource(hModule, hResource);
                    if (resData != IntPtr.Zero)
                    {
                        byte[] uiBytes = new byte[resSize];
                        IntPtr ipMemorySource = LockResource(resData);
                        Marshal.Copy(ipMemorySource, uiBytes, 0, (int)resSize);
                        return uiBytes;
                    }
                }
            }
            return null;
        }
    }
}
