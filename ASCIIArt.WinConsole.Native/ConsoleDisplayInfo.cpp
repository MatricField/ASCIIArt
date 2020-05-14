#include "pch.h"

#include <dwrite.h>
#include "ConsoleDisplayInfo.h"

ASCIIArtWinConsoleNative::ConsoleDisplayInfo::ConsoleDisplayInfo():
    fontInfo(new CONSOLE_FONT_INFOEX)
{
    // Lock Console
    auto hWindow { GetForegroundWindow() };
    auto style { GetWindowLong(hWindow, GWL_STYLE) };
    style &= ~WS_SIZEBOX;
    SetWindowLong(hWindow, GWL_STYLE, style);
    _PrintableChars = gcnew Lazy<IReadOnlyDictionary<String^, array<byte>^>^>(
        gcnew Func<IReadOnlyDictionary<String^, array<byte>^>^>(this, &GetAvailableCharBitmaps)
        );
    Initialize();
}

ASCIIArtWinConsoleNative::ConsoleDisplayInfo::~ConsoleDisplayInfo()
{
    this->!ConsoleDisplayInfo();
}

ASCIIArtWinConsoleNative::ConsoleDisplayInfo::!ConsoleDisplayInfo()
{
    delete fontInfo;
    fontInfo = nullptr;
}

void ASCIIArtWinConsoleNative::ConsoleDisplayInfo::Initialize()
{
    auto hStdOut { GetStdHandle(STD_OUTPUT_HANDLE) };
    GetCurrentConsoleFontEx(hStdOut, FALSE, fontInfo);
    _CharPixelSize = Size(fontInfo->dwFontSize.X, fontInfo->dwFontSize.Y);
}

IReadOnlyDictionary<String^, array<byte>^>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetAvailableCharBitmaps()
{
    using namespace System::IO;
    auto printable{ GetPrintableChars() };
    auto result{ gcnew Dictionary<String^, array<byte>^>() };
    auto currentWindow{ GetConsoleWindow() };
    auto stream{ gcnew MemoryStream() };

}

array<Char>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetPrintableChars()
{
    throw gcnew System::NotImplementedException();
    // TODO: insert return statement here
}

IEnumerable<char>^ ASCIIArtWinConsoleNative::ConsoleDisplayInfo::GetConsoleFontUnicodeRage()
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
            fontCollection->GetFontFamily(fontInfo -> nFont, &fam)
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
    return this->_PrintableChars->Value;
}