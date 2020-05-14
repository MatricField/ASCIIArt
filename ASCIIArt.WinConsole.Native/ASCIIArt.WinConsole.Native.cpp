#include "pch.h"

#include <dwrite.h>
#include "ASCIIArt.WinConsole.Native.h"

ASCIIArtWinConsoleNative::ConsoleDisplayInfo::ConsoleDisplayInfo()
{
    // Lock Console
    auto hWindow { GetForegroundWindow() };
    auto style { GetWindowLong(hWindow, GWL_STYLE) };
    style &= ~WS_SIZEBOX;
    SetWindowLong(hWindow, GWL_STYLE, style);
    Initialize();
}

void ASCIIArtWinConsoleNative::ConsoleDisplayInfo::Initialize()
{
    auto hStdOut { GetStdHandle(STD_OUTPUT_HANDLE) };
    CONSOLE_FONT_INFOEX fontInfo = { sizeof(CONSOLE_FONT_INFOEX) };
    GetCurrentConsoleFontEx(hStdOut, FALSE, &fontInfo);
    _CharPixelSize = Size(fontInfo.dwFontSize.X, fontInfo.dwFontSize.Y);
    _PrintableChars = GetAvailableCharBitmaps();
}

Dictionary<String^, array<byte>^>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetAvailableCharBitmaps()
{
    auto printable = GetPrintableChars();

}

array<Char>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetPrintableChars()
{
    throw gcnew System::NotImplementedException();
    // TODO: insert return statement here
}

IEnumerable<char>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetConsoleFontUnicodeRage(const CONSOLE_FONT_INFOEX& info)
{
    CoInitialize(nullptr);
    try
    {
        CComPtr<IDWriteFactory> factory;
        ThrowIfNotSuccess(
            DWriteCreateFactory(
                DWRITE_FACTORY_TYPE_SHARED,
                __uuidof(IDWriteFactory),
                (IUnknown**)(&factory))
        );

        CComPtr<IDWriteFontCollection> fontCollection;

        ThrowIfNotSuccess(
            factory->GetSystemFontCollection(&fontCollection)
        );

        CComPtr<IDWriteFontFamily> fam;
        ThrowIfNotSuccess(
            fontCollection->GetFontFamily(info.nFont, &fam)
        );
    }
    finally
    {
        CoUninitialize();
        throw;
    }
}

void ASCIIArtWinConsoleNative::ConsoleDisplayInfo::ThrowIfNotSuccess(HRESULT hr)
{
    if (!SUCCEEDED(hr))
    {
        Marshal::ThrowExceptionForHR(hr);
    }
}

int ASCIIArtWinConsoleNative::ConsoleDisplayInfo::HeightInRows::get()
{
    return Console::WindowHeight;
}

int ASCIIArtWinConsoleNative::ConsoleDisplayInfo::WidthInColumns::get()
{
    return Console::WindowWidth;
}

Size ASCIIArtWinConsoleNative::ConsoleDisplayInfo::ClientAreaSize::get()
{
    return Size(WidthInColumns * CharPixelSize.Width, HeightInRows * CharPixelSize.Height);
}

Size ASCIIArtWinConsoleNative::ConsoleDisplayInfo::CharPixelSize::get()
{
    return this->_CharPixelSize;
}

IReadOnlyDictionary<String^, array<byte>^>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::PrintableChars::get()
{
    return this->_PrintableChars;
}