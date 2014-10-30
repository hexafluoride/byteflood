#pragma once

#include "TorrentAlert.h"

namespace libtorrent
{
    struct stats_alert;
}

namespace Ragnar
{
    public ref class StatsAlert : TorrentAlert
    {
    private:
        int _uploadPayload;
        int _uploadProtocol;
        int _downloadPayload;
        int _downloadProtocol;
        int _uploadIpProtocol;
        int _uploadDhtProtocol;
        int _uploadTrackerProtocol;
        int _downloadIpProtocol;
        int _downloadDhtProtocol;
        int _downloadTrackerProtocol;
        int _interval;

    internal:
        StatsAlert(libtorrent::stats_alert* alert);

    public:
        property int UploadPayload { int get() { return this->_uploadPayload; } }

        property int UploadProtocol { int get() { return this->_uploadProtocol; } }

        property int DownloadPayload { int get() { return this->_downloadPayload; } }

        property int DownloadProtocol { int get() { return this->_downloadProtocol; } }

        property int UploadIpProtocol { int get() { return this->_uploadIpProtocol; } }

        property int UploadDhtProtocol { int get() { return this->_uploadDhtProtocol; } }

        property int UploadTrackerProtocol { int get() { return this->_uploadTrackerProtocol; } }

        property int DownloadIpProtocol { int get() { return this->_downloadIpProtocol; } }

        property int DownloadDhtProtocol { int get() { return this->_downloadDhtProtocol; } }

        property int DownloadTrackerProtocol { int get() { return this->_downloadTrackerProtocol; } }

        property int Interval { int get() { return this->_interval; } }
    };
}