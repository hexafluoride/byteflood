#include "stdafx.h"
#include "TorrentCheckedAlert.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
	TorrentCheckedAlert::TorrentCheckedAlert(libtorrent::torrent_checked_alert* alert)
		: TorrentAlert((libtorrent::torrent_alert*) alert)
	{
	}
}