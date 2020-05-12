#pragma once

#include <dwrite.h>
#include <atlcomcli.h>
#include <comdef.h>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace ATL;

namespace ASCIIArtWinConsoleNative {
    public ref class ConsoleDisplayInfo
    {
    private:
        array<char>^ GetConsoleFontUnicodeRage()
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
                    fontCollection->GetFontFamily(0, &fam)
                );
            }
            finally
            {
                CoUninitialize();
                throw;
            }
        }
        static void ThrowIfNotSuccess(HRESULT hr)
        {
            if (!SUCCEEDED(hr))
            {
                Marshal::ThrowExceptionForHR(hr);
            }
        }
    };
}
