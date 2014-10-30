#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct state_changed_alert;
}

namespace Ragnar
{
    enum class TorrentState;

    public ref class StateChangedAlert : TorrentAlert
    {
    private:
        TorrentState _state;
        TorrentState _previousState;

    internal:
        StateChangedAlert(libtorrent::state_changed_alert* alert);

    public:
        property TorrentState State { TorrentState get(); }

        property TorrentState PreviousState { TorrentState get(); }
    };
}
