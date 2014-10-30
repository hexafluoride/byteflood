#include "stdafx.h"

#include "AnnounceEntry.h"
#include "PartialPieceInfo.h"
#include "PeerInfo.h"
#include "SHA1Hash.h"
#include "TorrentInfo.h"
#include "TorrentHandle.h"
#include "TorrentStatus.h"
#include "Utils.h"

#include <libtorrent\peer_info.hpp>
#include <libtorrent\torrent_handle.hpp>

namespace Ragnar
{
    TorrentHandle::TorrentHandle(const libtorrent::torrent_handle &handle)
    {
        this->_handle = new libtorrent::torrent_handle(handle);
    }

    TorrentHandle::~TorrentHandle()
    {
        if (this->_disposed)
        {
            return;
        }

        this->!TorrentHandle();

        this->_disposed = true;
    }

    TorrentHandle::!TorrentHandle()
    {
        delete this->_handle;
    }

    void TorrentHandle::ReadPiece(int pieceIndex)
    {
        this->_handle->read_piece(pieceIndex);
    }

    bool TorrentHandle::HavePiece(int pieceIndex)
    {
        return this->_handle->have_piece(pieceIndex);
    }

    System::Collections::Generic::IEnumerable<PeerInfo^>^ TorrentHandle::GetPeerInfo()
    {
        std::vector<libtorrent::peer_info> peers;
        this->_handle->get_peer_info(peers);

        auto result = gcnew System::Collections::Generic::List<PeerInfo^>(peers.size());

        for (auto i = peers.begin(); i != peers.end(); i++)
        {
            result->Add(gcnew PeerInfo(*i));
        }

        return result;
    }

    SHA1Hash^ TorrentHandle::InfoHash::get()
    {
        return gcnew SHA1Hash(this->_handle->info_hash());
    }

    TorrentStatus^ TorrentHandle::GetStatus()
    {
        return gcnew TorrentStatus(this->_handle->status());
    }

	TorrentStatus^ TorrentHandle::QueryStatus()
	{
		return gcnew TorrentStatus(this->_handle->status());
	}


    System::Collections::Generic::IEnumerable<PartialPieceInfo^>^ TorrentHandle::GetDownloadQueue()
    {
        std::vector<libtorrent::partial_piece_info> queue;
        this->_handle->get_download_queue(queue);

        auto result = gcnew System::Collections::Generic::List<PartialPieceInfo^>(queue.size());

        for (auto i = queue.begin(); i != queue.end(); i++)
        {
            result->Add(gcnew PartialPieceInfo(*i));
        }

        return result;
    }

    void TorrentHandle::ResetPieceDeadline(int pieceIndex)
    {
        this->_handle->reset_piece_deadline(pieceIndex);
    }

    void TorrentHandle::ClearPieceDeadlines()
    {
        this->_handle->clear_piece_deadlines();
    }

    void TorrentHandle::SetPieceDeadline(int pieceIndex, int deadline)
    {
        this->_handle->set_piece_deadline(pieceIndex, deadline);
    }

    void TorrentHandle::SetPriority(int priority)
    {
        this->_handle->set_priority(priority);
    }

    cli::array<long long>^ TorrentHandle::GetFileProgresses()
    {
        std::vector<libtorrent::size_type> fp;
        this->_handle->file_progress(fp);

        auto result = gcnew cli::array<long long>(fp.size());

        for (int i = 0; i < fp.size(); i++)
        {
            result[i] = fp[i];
        }

        return result;
    }

    void TorrentHandle::ClearError()
    {
        this->_handle->clear_error();
    }

    void TorrentHandle::AddTracker(AnnounceEntry^ entry)
    {
        this->_handle->add_tracker(*entry->get_ptr());
    }

    System::Collections::Generic::IEnumerable<AnnounceEntry^>^ TorrentHandle::GetTrackers()
    {
        auto trackers = this->_handle->trackers();
        auto result = gcnew System::Collections::Generic::List<AnnounceEntry^>(trackers.size());

        for (auto i = trackers.begin(); i != trackers.end(); i++)
        {
            result->Add(gcnew AnnounceEntry(*i));
        }

        return result;
    }

    void TorrentHandle::Pause()
    {
        this->_handle->pause();
    }

    void TorrentHandle::Resume()
    {
        this->_handle->resume();
    }

    void TorrentHandle::SetUploadMode(bool value)
    {
        this->_handle->set_upload_mode(value);
    }

    void TorrentHandle::SetShareMode(bool value)
    {
        this->_handle->set_share_mode(value);
    }

    void TorrentHandle::FlushCache()
    {
        this->_handle->flush_cache();
    }

    void TorrentHandle::ApplyIPFilter(bool value)
    {
        this->_handle->apply_ip_filter(value);
    }

    void TorrentHandle::ForceRecheck()
    {
        this->_handle->force_recheck();
    }

    void TorrentHandle::SaveResumeData()
    {
        this->_handle->save_resume_data();
    }

    bool TorrentHandle::NeedSaveResumeData()
    {
        return this->_handle->need_save_resume_data();
    }

    bool TorrentHandle::AutoManaged::get()
    {
        return this->_handle->is_auto_managed();
    }

    void TorrentHandle::AutoManaged::set(bool value)
    {
        this->_handle->auto_managed(value);
    }

    void TorrentHandle::QueuePositionDown()
    {
        this->_handle->queue_position_down();
    }

    void TorrentHandle::QueuePositionTop()
    {
        this->_handle->queue_position_top();
    }

    void TorrentHandle::QueuePositionBottom()
    {
        this->_handle->queue_position_bottom();
    }

    void TorrentHandle::QueuePositionUp()
    {
        this->_handle->queue_position_up();
    }

    int TorrentHandle::QueuePosition::get()
    {
        return this->_handle->queue_position();
    }

    bool TorrentHandle::ResolveCountries::get()
    {
        return this->_handle->resolve_countries();
    }

    void TorrentHandle::ResolveCountries::set(bool value)
    {
        this->_handle->resolve_countries(value);
    }

    TorrentInfo^ TorrentHandle::TorrentFile::get()
    {
        auto ptr = this->_handle->torrent_file();

        if (!ptr)
        {
            return nullptr;
        }

        return gcnew TorrentInfo(*ptr.get());
    }

    int TorrentHandle::GetFilePriority(int fileIndex)
    {
        return this->_handle->file_priority(fileIndex);
    }

    void TorrentHandle::SetFilePriorities(cli::array<int>^ filePriorities)
    {
        std::vector<int> prios(filePriorities->Length);

        for (int i = 0; i < filePriorities->Length; i++)
        {
            prios[i] = filePriorities[i];
        }

        this->_handle->prioritize_files(prios);
    }

    void TorrentHandle::SetFilePriority(int fileIndex, int priority)
    {
        this->_handle->file_priority(fileIndex, priority);
    }

    cli::array<int>^ TorrentHandle::GetFilePriorities()
    {
        auto prios = this->_handle->file_priorities();
        auto result = gcnew cli::array<int>(prios.size());

        for (int i = 0; i < prios.size(); i++)
        {
            result[i] = prios[i];
        }

        return result;
    }

    void TorrentHandle::ForceReannounce()
    {
        this->_handle->force_reannounce();
    }

    void TorrentHandle::ForceReannounce(int seconds, int trackerIndex)
    {
        this->_handle->force_reannounce(seconds, trackerIndex);
    }

    void TorrentHandle::ForceDhtAnnounce()
    {
        this->_handle->force_dht_announce();
    }

    void TorrentHandle::ScrapeTracker()
    {
        this->_handle->scrape_tracker();
    }

    int TorrentHandle::UploadLimit::get()
    {
        return this->_handle->upload_limit();
    }

    void TorrentHandle::UploadLimit::set(int value)
    {
        this->_handle->set_upload_limit(value);
    }

    int TorrentHandle::DownloadLimit::get()
    {
        return this->_handle->download_limit();
    }

    void TorrentHandle::DownloadLimit::set(int value)
    {
        this->_handle->set_download_limit(value);
    }

    bool TorrentHandle::SequentialDownload::get()
    {
        return this->_handle->is_sequential_download();
    }

    void TorrentHandle::SequentialDownload::set(bool value)
    {
        this->_handle->set_sequential_download(value);
    }

    int TorrentHandle::MaxUploads::get()
    {
        return this->_handle->max_uploads();
    }

    void TorrentHandle::MaxUploads::set(int value)
    {
        this->_handle->set_max_uploads(value);
    }

    int TorrentHandle::MaxConnections::get()
    {
        return this->_handle->max_connections();
    }

    void TorrentHandle::MaxConnections::set(int value)
    {
        this->_handle->set_max_connections(value);
    }

    void TorrentHandle::SetTrackerLogin(System::String^ userName, System::String^ password)
    {
        this->_handle->set_tracker_login(Utils::GetStdStringFromManagedString(userName), Utils::GetStdStringFromManagedString(password));
    }

    void TorrentHandle::MoveStorage(System::String^ savePath, MoveFlags flags)
    {
        this->_handle->move_storage(Utils::GetStdStringFromManagedString(savePath), (int) flags);
    }

    void TorrentHandle::RenameFile(int fileIndex, System::String^ fileName)
    {
        this->_handle->rename_file(fileIndex, Utils::GetStdStringFromManagedString(fileName));
    }

    bool TorrentHandle::SuperSeeding::get()
    {
        return this->_handle->super_seeding();
    }

    void TorrentHandle::SuperSeeding::set(bool value)
    {
        this->_handle->super_seeding(value);
    }

    bool TorrentHandle::IsFinished::get()
    {
        return this->_handle->is_finished();
    }

    bool TorrentHandle::IsPaused::get()
    {
        return this->_handle->is_paused();
    }

    bool TorrentHandle::IsSeed::get()
    {
        return this->_handle->is_seed();
    }

    bool TorrentHandle::HasMetadata::get()
    {
        return this->_handle->has_metadata();
    }
	
	bool TorrentHandle::IsValid::get()
	{
		return this->_handle->is_valid();
	}
}
