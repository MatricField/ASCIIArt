using ASCIIArt.Engine;
using ASCIIArt.WinConsole.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace ASCIIArt.WinConsole
{
    public class ConsoleDisplayInfo : IConsoleDisplayInfo
    {
        public int WidthInColumns => Console.WindowWidth;
        public int HeightInRows => Console.WindowHeight;

        public Size ClientAreaSize =>
            new Size(WidthInColumns * CharPixelSize.Width, HeightInRows * CharPixelSize.Height);

        public Size CharPixelSize { get; private set; }

        public IReadOnlyDictionary<string, byte[]> PrintableChars { get; private set; }

        public ConsoleDisplayInfo()
        {
            // Lock Console
            var hWindow = User32.GetForegroundWindow();
            var style = (WindowStyles)User32.GetWindowLong(hWindow, WindowLongFlags.GWL_STYLE);
            style &= ~WindowStyles.WS_SIZEBOX;
            User32.SetWindowLong(hWindow, WindowLongFlags.GWL_STYLE, (int)style);

            // Fetch Font Info
            var hStdOut = Kernel32.GetStdHandle(StdHandle.STD_OUTPUT_HANDLE);
            var fontInfo = CONSOLE_FONT_INFOEX.Create();
            Kernel32.GetCurrentConsoleFontEx(hStdOut, false, ref fontInfo);

            CharPixelSize = fontInfo.dwFontSize;

            PrintableChars = GetAvailableCharBitmaps();
        }

        private Dictionary<string, byte[]> GetAvailableCharBitmaps()
        {
            var printable = GetPrintableChars();
            Console.Clear();
            Console.WriteLine(new string(printable));
            var currentWindow = Kernel32.GetConsoleWindow();

            //Wait for screen to update
            Thread.Sleep(300);

            using (var stream = new MemoryStream())
            using (var bitmap = CaptureScreen(currentWindow))
            {
                Console.Clear();
                var result = new Dictionary<string, byte[]>();
                var i = 0;
                foreach (var c in printable)
                {
                    var loc = new Point()
                    {
                        X = i % WidthInColumns * CharPixelSize.Width,
                        Y = i / WidthInColumns * CharPixelSize.Height,
                    };
                    var charRect = new Rectangle(loc, CharPixelSize);
                    var piece = bitmap.Clone(charRect, bitmap.PixelFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.SetLength(0);
                    piece.Save(stream, ImageFormat.Png);
                    result.Add(c.ToString(), stream.ToArray());
                    i++;
                }
                return result;
            }
        }

        private static char[] GetPrintableChars()
        {
            var layout = User32.GetKeyboardLayout(Kernel32.GetCurrentThreadId());
            return Enumerable
                .Range(1, 127)
                .Select(Convert.ToChar)
                //.Where(c => !char.IsWhiteSpace(c) && User32.VkKeyScanEx(c, layout) != -1)
                .Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c))
                .Append(' ')
                .ToArray();
        }

        private static Bitmap CaptureScreen(IntPtr hWindow)
        {
            User32.GetClientRect(hWindow, out var rect);
            var result = new Bitmap(
                rect.Width,
                rect.Height,
                PixelFormat.Format32bppArgb);
            using (var bmpGraphics = Graphics.FromImage(result))
            {
                POINT loc = rect.Location;
                User32.ClientToScreen(hWindow, ref loc);
                bmpGraphics.CopyFromScreen(loc, Point.Empty, rect.Size);
            }
            return result;
        }
    }
}
