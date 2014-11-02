#include "stdafx.h"
#include "ScrapeReplyAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    ScrapeReplyAlert::ScrapeReplyAlert(libtorrent::scrape_reply_alert* alert)
        : TrackerAlert((libtorrent::tracker_alert*) alert)
    {
        this->_complete = alert->complete;
        this->_incomplete = alert->incomplete;
    }

    int ScrapeReplyAlert::Complete::get()
    {
        return this->_complete;
    }

    int ScrapeReplyAlert::Incomplete::get()
    {
        return this->_incomplete;
    }
}
