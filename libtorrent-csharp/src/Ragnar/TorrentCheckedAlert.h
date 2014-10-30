#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
	struct torrent_checked_alert;
}

namespace Ragnar
{
	public ref class TorrentCheckedAlert : TorrentAlert
	{
	internal:
		TorrentCheckedAlert(libtorrent::torrent_checked_alert* alert);
	};
}
