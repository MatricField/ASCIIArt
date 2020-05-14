using System.Collections.Generic;
using System.Drawing;

namespace ASCIIArt.Engine
{
    public interface IConsoleDisplayInfo
    {
        int HeightInRows { get; }
        int WidthInColumns { get; }
        Size ClientAreaSize { get; }
        Size CharPixelSize { get; }
        //Use string for rich text
        IReadOnlyDictionary<string, byte[]> PrintableChars { get; }
        void SetConsoleSize(int width, int height);
    }
}