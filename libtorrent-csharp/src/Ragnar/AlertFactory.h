#pragma once

#include "IAlertFactory.h"

#include <memory>

namespace libtorrent
{
    class session;
    class alert;
}

namespace Ragnar
{
    ref class Alert;

    public ref class AlertFactory : IAlertFactory
    {
    private:
        libtorrent::session& _session;
        Alert^ GetAlert(std::auto_ptr<libtorrent::alert> alert);

    internal:
        AlertFactory(libtorrent::session &session);

    public:
        virtual Alert^ Pop();

        virtual System::Collections::Generic::IEnumerable<Alert^>^ PopAll();

        virtual bool PeekWait(System::TimeSpan timeout);
    };
}
