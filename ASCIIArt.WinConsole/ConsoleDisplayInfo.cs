using ASCIIArt.Engine;
using ASCIIArt.WinConsole.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ASCIIArt.WinConsole
{
    public class ConsoleDisplayInfo : IConsoleDisplayInfo
    {
        public int WidthInColumns => Console.WindowWidth;
        public int HeightInRows => Console.WindowHeight;

        public int CharPixelWidth => CharPixelSize.Width;

        public int CharPixelHeight => CharPixelSize.Height;

        public Size ClientAreaSize =>
            new Size(WidthInColumns * CharPixelWidth, HeightInRows * CharPixelHeight);

        public Size CharPixelSize { get; protected set; }

        protected Lazy<IEnumerable<(string, byte[])>> _PrintableChars { get; set; }

        public IEnumerable<(string, byte[])> PrintableChars
        {
            get => _PrintableChars.Value;
        }


        public ConsoleDisplayInfo()
        {
            // Lock Console
            var hWindow = User32.GetForegroundWindow();
            var style = (WindowStyles)User32.GetWindowLong(hWindow, WindowLongFlags.GWL_STYLE);
            style &= ~WindowStyles.WS_SIZEBOX;
            User32.SetWindowLong(hWindow, WindowLongFlags.GWL_STYLE, (int)style);

            Initialize();
        }

        public void SetConsoleSize(in Size size)
        {
            SetConsoleSize(size.Width, size.Height);
        }

        public void SetConsoleSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
            Initialize();
        }

        protected virtual void Initialize()
        {
            // Fetch Font Info
            var hStdOut = Kernel32.GetStdHandle(StdHandle.STD_OUTPUT_HANDLE);
            var fontInfo = CONSOLE_FONT_INFOEX.Create();
            Kernel32.GetCurrentConsoleFontEx(hStdOut, false, ref fontInfo);

            CharPixelSize = fontInfo.dwFontSize;

            _PrintableChars = new Lazy<IEnumerable<(string, byte[])>>(
                () => GetAvailableCharBitmaps(GetPrintableChars(fontInfo)));
        }

        private List<(string, byte[])> GetAvailableCharBitmaps(char[] printable)
        {

            var result = new List<(string, byte[])>();
            var currentWindow = Kernel32.GetConsoleWindow();
            using (var stream = new MemoryStream())
            {
                foreach (var chunck in printable.Chunk(WidthInColumns * HeightInRows))
                {
                    Console.Clear();
                    Console.Write(string.Concat(chunck));
                    //Wait for screen to update
                    Thread.Sleep(300);
                    using (var bitmap = CaptureScreen(currentWindow))
                    {
                        var i = 0;
                        foreach (var c in chunck)
                        {
                            var loc = new Point()
                            {
                                X = i % WidthInColumns * CharPixelWidth,
                                Y = i / WidthInColumns * CharPixelHeight,
                            };
                            var charRect = new Rectangle(loc, CharPixelSize);
                            var piece = bitmap.Clone(charRect, bitmap.PixelFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.SetLength(0);
                            piece.Save(stream, ImageFormat.Png);
                            result.Add((c.ToString(), stream.ToArray()));
                            i++;
                        }
                    }
                }
            }
            Console.Clear();
            return result;
        }

        private static char[] GetPrintableChars(in CONSOLE_FONT_INFOEX consoleInfo)
        {
            var fontCollection = new InstalledFontCollection();
            try
            {
                var fam = new FontFamily(consoleInfo.FaceName, fontCollection);

                var font = new Font(fam, 20);
                var ranges = GetFontUnicodeRanges(font);
                return Enumerable
                    .Range(1, char.MaxValue)
                    .Where(c => ranges.Any(r => c >= r.Low && c < r.High))
                    .Select(Convert.ToChar)
                    .Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c))
                    .Append(' ')
                    .ToArray();
            }
            catch(ArgumentException)
            {
                return Enumerable
                    .Range(1, 127)
                    .Select(Convert.ToChar)
                    //.Where(c => !char.IsWhiteSpace(c) && User32.VkKeyScanEx(c, layout) != -1)
                    .Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c))
                    .Append(' ')
                    .ToArray();
            }

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

        private static List<FontRange> GetFontUnicodeRanges(Font font)
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = g.GetHdc();
            IntPtr hFont = font.ToHfont();
            IntPtr old = GDI32.SelectObject(hdc, hFont);
            uint size = GDI32.GetFontUnicodeRanges(hdc, IntPtr.Zero);
            IntPtr glyphSet = Marshal.AllocHGlobal((int)size);
            try
            {
                GDI32.GetFontUnicodeRanges(hdc, glyphSet);
                List<FontRange> fontRanges = new List<FontRange>();
                int count = Marshal.ReadInt32(glyphSet, 12);
                for (int i = 0; i < count; i++)
                {
                    FontRange range = new FontRange();
                    range.Low = (ushort)Marshal.ReadInt16(glyphSet, 16 + i * 4);
                    range.High = (ushort)(range.Low + Marshal.ReadInt16(glyphSet, 18 + i * 4) - 1);
                    fontRanges.Add(range);
                }
                return fontRanges;
            }
            finally
            {
                GDI32.SelectObject(hdc, old);
                Marshal.FreeHGlobal(glyphSet);
                g.ReleaseHdc(hdc);
                g.Dispose();
            }
        }
    }

    internal static class LinqEx
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            if (chunksize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than zero.", nameof(chunksize));
            }
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
