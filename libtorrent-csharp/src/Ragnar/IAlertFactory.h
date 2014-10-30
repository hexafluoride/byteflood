#pragma once

namespace Ragnar
{
    ref class Alert;

    public interface class IAlertFactory
    {
        Alert^ Pop();

        System::Collections::Generic::IEnumerable<Alert^>^ PopAll();

        bool PeekWait(System::TimeSpan timeout);
    };
}
