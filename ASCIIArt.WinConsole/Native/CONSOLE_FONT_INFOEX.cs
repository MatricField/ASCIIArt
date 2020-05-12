using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASCIIArt.WinConsole.Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CONSOLE_FONT_INFOEX
    {
        public uint cbSize;
        public uint nFont;
        public COORD dwFontSize;
        public int FontFamily;
        public int FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;

        public static CONSOLE_FONT_INFOEX Create()
        {
            return new CONSOLE_FONT_INFOEX()
            {
                cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>()
            };
        }
    }
}
