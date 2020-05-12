using System.Collections.Generic;
using System.Drawing;

namespace ASCIIArt.Engine
{
    public interface IConsoleDisplayInfo
    {
        int CharPixelHeight { get; }
        int CharPixelWidth { get; }
        int HeightInRows { get; }
        int WidthInColumns { get; }
        Size ClientAreaSize { get; }
        Size CharPixelSize { get; }
        //Use string for rich text
        IEnumerable<(string, byte[])> PrintableChars { get; }
    }
}