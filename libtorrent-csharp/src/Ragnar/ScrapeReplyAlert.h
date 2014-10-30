#pragma once

#include "TrackerAlert.h"

namespace libtorrent
{
    struct scrape_reply_alert;
}

namespace Ragnar
{
    public ref class ScrapeReplyAlert : TrackerAlert
    {
    private:
        int _complete;
        int _incomplete;

    internal:
        ScrapeReplyAlert(libtorrent::scrape_reply_alert* alert);

    public:
        property int Complete { int get(); }

        property int Incomplete { int get(); }
    };
}
