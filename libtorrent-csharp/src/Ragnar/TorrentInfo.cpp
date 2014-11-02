#include "stdafx.h"
#include "FileEntry.h"
#include "TorrentInfo.h"
#include "Utils.h"

#include <libtorrent\torrent_info.hpp>

namespace Ragnar
{
    TorrentInfo::TorrentInfo(const libtorrent::torrent_info &info)
    {
        this->_info = new libtorrent::torrent_info(info);
    }

    TorrentInfo::TorrentInfo(System::String^ fileName)
    {
        this->_info = new libtorrent::torrent_info(Utils::GetStdStringFromManagedString(fileName));
    }

    TorrentInfo::TorrentInfo(cli::array<byte>^ buffer)
    {
        pin_ptr<unsigned char> ptr = &buffer[0];
        const char *pbegin = (const char*)(const unsigned char*)ptr;

        this->_info = new libtorrent::torrent_info(pbegin, buffer->Length);
    }

    TorrentInfo::~TorrentInfo()
    {
        delete this->_info;
    }

    void TorrentInfo::RenameFile(int fileIndex, System::String^ fileName)
    {
        this->_info->rename_file(fileIndex, Utils::GetStdStringFromManagedString(fileName));
    }

    void TorrentInfo::AddTracker(System::String^ url)
    {
        this->AddTracker(url, 0);
    }

    void TorrentInfo::AddTracker(System::String^ url, int tier)
    {
        this->_info->add_tracker(Utils::GetStdStringFromManagedString(url), tier);
    }

    int TorrentInfo::NumPieces::get()
    {
        return this->_info->num_pieces();
    }

    long long TorrentInfo::TotalSize::get()
    {
        return this->_info->total_size();
    }

    int TorrentInfo::PieceLength::get()
    {
        return this->_info->piece_length();
    }

    System::String^ TorrentInfo::InfoHash::get()
    {
        return gcnew String(libtorrent::to_hex(this->_info->info_hash().to_string()).c_str());
    }

    int TorrentInfo::NumFiles::get()
    {
        return this->_info->num_files();
    }

    FileEntry^ TorrentInfo::FileAt(int index)
    {
        return gcnew FileEntry(this->_info->file_at(index));
    }

    System::String^ TorrentInfo::SslCert::get()
    {
        return gcnew System::String(this->_info->ssl_cert().c_str());
    }

    bool TorrentInfo::IsValid::get()
    {
        return this->_info->is_valid();
    }

    bool TorrentInfo::Private::get()
    {
        return this->_info->priv();
    }

    int TorrentInfo::PieceSize(int index)
    {
        return this->_info->piece_size(index);
    }

    System::Nullable<DateTime> TorrentInfo::CreationDate::get()
    {
        auto date = this->_info->creation_date();

        if (!date)
        {
            return Nullable<DateTime>();
        }

        return Utils::GetDateTimeFromTimeT(date.get());
    }

    System::String^ TorrentInfo::Name::get()
    {
        return gcnew System::String(this->_info->name().c_str());
    }

    System::String^ TorrentInfo::Comment::get()
    {
        return gcnew System::String(this->_info->comment().c_str());
    }

    System::String^ TorrentInfo::Creator::get()
    {
        return gcnew System::String(this->_info->creator().c_str());
    }

    int TorrentInfo::MetadataSize::get()
    {
        return this->_info->metadata_size();
    }

    bool TorrentInfo::IsMerkleTorrent::get()
    {
        return this->_info->is_merkle_torrent();
    }
}
