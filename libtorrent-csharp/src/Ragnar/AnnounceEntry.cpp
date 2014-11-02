#include "stdafx.h"
#include "AnnounceEntry.h"
#include "Utils.h"

#include <libtorrent\torrent_info.hpp>

namespace Ragnar
{
    using namespace System;

    AnnounceEntry::AnnounceEntry(const libtorrent::announce_entry &entry)
    {
        this->_entry = new libtorrent::announce_entry(entry);
    }

    AnnounceEntry::AnnounceEntry(String^ url)
    {
        this->_entry = new libtorrent::announce_entry(Utils::GetStdStringFromManagedString(url));
    }

    AnnounceEntry::~AnnounceEntry()
    {
        if (this->_disposed)
        {
            return;
        }

        this->!AnnounceEntry();

        this->_disposed = true;
    }

    AnnounceEntry::!AnnounceEntry()
    {
        delete this->_entry;
    }

    String^ AnnounceEntry::Url::get()
    {
        return gcnew String(this->_entry->url.c_str());
    }

    String^ AnnounceEntry::TrackerId::get()
    {
        return gcnew String(this->_entry->trackerid.c_str());
    }

    String^ AnnounceEntry::Message::get()
    {
        return gcnew String(this->_entry->message.c_str());
    }

    int AnnounceEntry::ScrapeIncomplete::get()
    {
        return this->_entry->scrape_incomplete;
    }

    int AnnounceEntry::ScrapeComplete::get()
    {
        return this->_entry->scrape_complete;
    }

    int AnnounceEntry::ScrapeDownloaded::get()
    {
        return this->_entry->scrape_downloaded;
    }

    int AnnounceEntry::Tier::get()
    {
        return this->_entry->tier;
    }

    int AnnounceEntry::FailLimit::get()
    {
        return this->_entry->fail_limit;
    }

    int AnnounceEntry::Fails::get()
    {
        return this->_entry->fails;
    }

    bool AnnounceEntry::Updating::get()
    {
        return this->_entry->updating;
    }

    bool AnnounceEntry::Verified::get()
    {
        return this->_entry->verified;
    }

    bool AnnounceEntry::StartSent::get()
    {
        return this->_entry->start_sent;
    }

    bool AnnounceEntry::CompleteSent::get()
    {
        return this->_entry->complete_sent;
    }

    bool AnnounceEntry::SendStats::get()
    {
        return this->_entry->send_stats;
    }


}