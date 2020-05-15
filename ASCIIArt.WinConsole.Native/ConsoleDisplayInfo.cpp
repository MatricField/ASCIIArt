#include "pch.h"

#include <memory>
#include <dwrite_1.h>
#include <vector>
#include "ConsoleDisplayInfo.h"

ASCIIArtWinConsole::Native::ConsoleDisplayInfo::ConsoleDisplayInfo():
    fontInfo(new CONSOLE_FONT_INFOEX{sizeof(CONSOLE_FONT_INFOEX)})
{
    // Lock Console
    auto hWindow { GetForegroundWindow() };
    auto style { GetWindowLong(hWindow, GWL_STYLE) };
    style &= ~WS_SIZEBOX;
    SetWindowLong(hWindow, GWL_STYLE, style);
    _PrintableChars = gcnew Lazy<IReadOnlyDictionary<String^, array<byte>^>^>(
        gcnew Func<IReadOnlyDictionary<String^, array<byte>^>^>(this, &ConsoleDisplayInfo::GetAvailableCharBitmaps)
        );
    Initialize();
}

void ASCIIArtWinConsole::Native::ConsoleDisplayInfo::SetConsoleSize(int width, int height)
{
    Console::SetWindowSize(width, height);
    Initialize();
}

ASCIIArtWinConsole::Native::ConsoleDisplayInfo::~ConsoleDisplayInfo()
{
    this->!ConsoleDisplayInfo();
}

ASCIIArtWinConsole::Native::ConsoleDisplayInfo::!ConsoleDisplayInfo()
{
    delete fontInfo;
    fontInfo = nullptr;
}

void ASCIIArtWinConsole::Native::ConsoleDisplayInfo::Initialize()
{
    auto hStdOut { GetStdHandle(STD_OUTPUT_HANDLE) };
    GetCurrentConsoleFontEx(hStdOut, FALSE, fontInfo);
    _CharPixelSize = Size(fontInfo->dwFontSize.X, fontInfo->dwFontSize.Y);
}

IReadOnlyDictionary<String^, array<byte>^>^ ASCIIArtWinConsole::Native::ConsoleDisplayInfo::GetAvailableCharBitmaps()
{
    using namespace System::IO;
    using namespace System::Threading;
    using namespace System::Drawing;
    using namespace System::Drawing::Imaging;
    using namespace System::Collections::Generic;

    auto printable = GetPrintableChars();
    auto result{ gcnew Dictionary<String^, array<byte>^>() };
    auto currentWindow{ GetConsoleWindow() };
    auto stream{ gcnew MemoryStream() };
    auto chunkSize{ WidthInColumns * HeightInRows };

    auto buffer{ gcnew List<Tuple<Char, Point>^>() };
    auto e = printable->GetEnumerator();
    bool next = true;
    for (decltype(chunkSize) begin; next; begin += chunkSize)
    {
        Console::Clear();
        buffer->Clear();
        for (decltype(begin) i = 0; i < begin + chunkSize && (next = e->MoveNext()); i++)
        {
            Point loc(i % WidthInColumns * CharPixelSize.Width, i / WidthInColumns * CharPixelSize.Height);
            auto c = e->Current;
            Console::Write(c);
            buffer->Add(gcnew Tuple<Char, Point>(c, loc));
        }
        Thread::Sleep(300);
        auto bitmap{ CaptureScreen(currentWindow) };
        for each(auto pair in buffer)
        {      
            auto c = pair->Item1;
            auto loc = pair->Item2;
            System::Drawing::Rectangle charRect(loc, CharPixelSize);
            auto piece = bitmap->Clone(charRect, bitmap->PixelFormat);
            stream->Seek(0, SeekOrigin::Begin);
            stream->SetLength(0);
            piece->Save(stream, ImageFormat::Png);
            result->Add(c.ToString(), stream->ToArray());
        }
    }
    Console::Clear();
    return result;
}

[StructLayout(LayoutKind::Sequential)]
value struct Range
{
    UINT32 first;
    UINT32 last;
};

ref class UnicodeRangeEmrt :
    public IEnumerator<Char>, public IDisposable
{
    IList<Range>^ ranges;
    Char _Current;
    std::size_t iRange;
public:
    explicit UnicodeRangeEmrt(IList<Range>^ ranges):
        ranges(ranges), _Current(), iRange()
    {

    }
    ~UnicodeRangeEmrt()
    {

    }

    virtual bool MoveNext()
    {
        if (iRange == ranges->Count)
        {
            return false;
        }
        if (_Current++ < ranges[iRange].last)
        {
            return true;
        }
        else if (iRange++ < ranges->Count)
        {
            _Current = ranges[iRange].first;
            return true;
        }
        else
        {
            return false;
        }
    }
    virtual void Reset()
    {
        throw gcnew NotSupportedException();
    }
    virtual property Char Current
    {
        Char get()
        {
            return _Current;
        }
    }

protected:
    virtual property System::Object^ NonGenericCurrent
    {
        System::Object^ get() = System::Collections::IEnumerator::Current::get
        {
            return Current;
        }
    }
};

ref class UnicodeRangeEmrb :
    public IEnumerable<Char>
{
    IList<Range>^ ranges;
public:
    explicit UnicodeRangeEmrb(IList<Range>^ ranges) :
        ranges(ranges)
    {

    }

    virtual System::Collections::Generic::IEnumerator<wchar_t>^ GetEnumerator()
    {
        return gcnew UnicodeRangeEmrt(ranges);
    }

protected:
    virtual System::Collections::IEnumerator^ NonGenericGetEnumerator() =
        System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew UnicodeRangeEmrt(ranges);
    }
};

IEnumerable<Char>^ ASCIIArtWinConsole::Native::ConsoleDisplayInfo::GetPrintableChars()
{
    CoInitialize(nullptr);
    try
    {
        CComPtr<IDWriteFactory1> factory;
        HRESULT hr;
        hr = DWriteCreateFactory(
            DWRITE_FACTORY_TYPE_SHARED,
            __uuidof(IDWriteFactory1),
            (IUnknown**)(&factory));
        Check(hr);
        CComPtr<IDWriteFontCollection> fontCollection;
        hr = factory->GetSystemFontCollection(&fontCollection);
        Check(hr);
        CComPtr<IDWriteFontFamily> fam;
        BOOL exist;
        UINT index;
        hr = fontCollection->FindFamilyName(fontInfo->FaceName, &index, &exist);
        Check(hr);
        if (!exist)
        {
            //TODO: Load terminal font
            throw gcnew NotImplementedException();
        }
        hr = fontCollection->GetFontFamily(index, &fam);
        CComPtr<IDWriteFont> _font;
        hr = fam->GetFont(0, &_font);
        Check(hr);
        CComPtr<IDWriteFont1> font;
        hr = _font->QueryInterface<IDWriteFont1>(&font);
        Check(hr);
        UINT32 rangeCount;
        hr = font->GetUnicodeRanges(0, nullptr, &rangeCount);
        if (hr != E_NOT_SUFFICIENT_BUFFER)
        {
            Check(hr);
        }
        std::vector<DWRITE_UNICODE_RANGE> ranges(rangeCount);
        hr = font->GetUnicodeRanges(ranges.size(), ranges.data(), &rangeCount);
        Check(hr);

        auto rangeBuffer{ gcnew List<Range>(ranges.size()) };        
        for (auto& r : ranges)
        {
            auto p = IntPtr(&r);
            rangeBuffer->Add(Marshal::PtrToStructure<Range>(p));
        }
        return gcnew UnicodeRangeEmrb(rangeBuffer);
    }
    finally
    {
        CoUninitialize();
    }
}

Bitmap^ ASCIIArtWinConsole::Native::ConsoleDisplayInfo::CaptureScreen(HWND hWindow)
{
    using namespace System::Drawing::Imaging;
    RECT rect;
    GetClientRect(hWindow, &rect);
    auto result{
        gcnew Bitmap(
        rect.right - rect.left,
        rect.top - rect.bottom,
        PixelFormat::Format32bppArgb) 
    };
    auto bmpGraphics{ Graphics::FromImage(result) };
    POINT _loc{ rect.left, rect.top };
    ClientToScreen(hWindow, &_loc);
    Point loc(_loc.x, _loc.y);
    Size size(rect.right - rect.left, rect.top - rect.bottom);
    bmpGraphics->CopyFromScreen(loc, Point::Empty, size);
    return result;
}

void ASCIIArtWinConsole::Native::ConsoleDisplayInfo::Check(HRESULT hr)
{
    Marshal::ThrowExceptionForHR(hr);
}

int ASCIIArtWinConsole::Native::ConsoleDisplayInfo::HeightInRows::get()
{
    return Console::WindowHeight;
}

int ASCIIArtWinConsole::Native::ConsoleDisplayInfo::WidthInColumns::get()
{
    return Console::WindowWidth;
}

Size ASCIIArtWinConsole::Native::ConsoleDisplayInfo::ClientAreaSize::get()
{
    return Size(WidthInColumns * CharPixelSize.Width, HeightInRows * CharPixelSize.Height);
}

Size ASCIIArtWinConsole::Native::ConsoleDisplayInfo::CharPixelSize::get()
{
    return this->_CharPixelSize;
}

IReadOnlyDictionary<String^, array<byte>^>^ ASCIIArtWinConsole::Native::ConsoleDisplayInfo::PrintableChars::get()
{
    return this->_PrintableChars->Value;
}