#pragma once

using namespace ASCIIArt::Engine;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Runtime::InteropServices;
using namespace ATL;

namespace ASCIIArtWinConsole {
    namespace Native
    {
        public ref class ConsoleDisplayInfo :
            public IConsoleDisplayInfo
        {
            Size _CharPixelSize;
            Lazy<IReadOnlyDictionary<String^, array<byte>^>^>^ _PrintableChars;
            CONSOLE_FONT_INFOEX* fontInfo;
        public:
            property int HeightInRows
            {
                virtual int get();
            }

            property int WidthInColumns
            {
                virtual int get();
            }

            property Size ClientAreaSize
            {
                virtual Size get();
            }

            property Size CharPixelSize
            {
                virtual Size get();
            }

            property IReadOnlyDictionary<String^, array<byte>^>^ PrintableChars
            {
                virtual IReadOnlyDictionary<String^, array<byte>^>^ get();
            }

            ConsoleDisplayInfo();

            virtual void SetConsoleSize(int width, int height);

            virtual ~ConsoleDisplayInfo();

            !ConsoleDisplayInfo();

        protected:
            virtual void Initialize();

            virtual IReadOnlyDictionary<String^, array<byte>^>^ GetAvailableCharBitmaps();

        private:
            IEnumerable<Char>^ GetPrintableChars();

            static Bitmap^ CaptureScreen(HWND hWindow);

            static void Check(HRESULT hr);
        };
    }

}
