#include "stdafx.h"
#include "SaveResumeDataAlert.h"
#include "Utils.h"

#include <libtorrent\alert_types.hpp>

namespace Ragnar
{
    SaveResumeDataAlert::SaveResumeDataAlert(libtorrent::save_resume_data_alert* alert)
        : TorrentAlert((libtorrent::torrent_alert*) alert)
    {
        this->_alert = alert;
        this->_resumeData = Utils::GetByteArrayFromLibtorrentEntry(*this->_alert->resume_data.get());
    }

    cli::array<byte>^ SaveResumeDataAlert::ResumeData::get()
    {
        return this->_resumeData;
    }
}
