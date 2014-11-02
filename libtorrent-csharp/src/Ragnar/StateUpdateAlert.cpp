#include "stdafx.h"
#include "Collections\Vector.h"
#include "Interop\TorrentStatusValueConverter.h"
#include "StateUpdateAlert.h"
#include "TorrentStatus.h"

#include <libtorrent\alert_types.hpp>
#include <libtorrent\torrent_handle.hpp>

namespace Ragnar
{
    using namespace Ragnar::Collections;
    using namespace Ragnar::Interop;

    StateUpdateAlert::StateUpdateAlert(libtorrent::state_update_alert* alert)
        : Alert((libtorrent::alert*) alert)
    {
        this->_statuses = gcnew System::Collections::Generic::List<TorrentStatus^>(alert->status.size());

        for (int i = 0; i < alert->status.size(); i++)
        {
            this->_statuses->Add(gcnew TorrentStatus(alert->status[i]));
        }
    }
}
