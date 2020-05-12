using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASCIIArt.WinConsole.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COORD
    {
        public short X;
        public short Y;

        public COORD(short x, short y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(in COORD coord)
        {
            return new Point(coord.X, coord.Y);
        }

        public static implicit operator Size(in COORD coord)
        {
            return new Size(coord.X, coord.Y);
        }
    }
}
