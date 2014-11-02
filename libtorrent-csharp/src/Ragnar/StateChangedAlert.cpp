#include "stdafx.h"
#include "StateChangedAlert.h"
#include "TorrentState.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    StateChangedAlert::StateChangedAlert(libtorrent::state_changed_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_previousState = static_cast<TorrentState>(alert->prev_state);
        this->_state = static_cast<TorrentState>(alert->state);
    }

    TorrentState StateChangedAlert::State::get()
    {
        return this->_state;
    }

    TorrentState StateChangedAlert::PreviousState::get()
    {
        return this->_previousState;
    }
}