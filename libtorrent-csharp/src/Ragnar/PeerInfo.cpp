#include "stdafx.h"
#include "PeerInfo.h"
#include "Utils.h"

#include <libtorrent\peer_info.hpp>

namespace Ragnar
{
    PeerInfo::PeerInfo(const libtorrent::peer_info &info)
    {
        this->_info = new libtorrent::peer_info(info);
    }

    PeerInfo::~PeerInfo()
    {
        delete this->_info;
    }

    PeerFlags PeerInfo::Flags::get()
    {
        return (PeerFlags)this->_info->flags;
    }

    System::Net::IPEndPoint^ PeerInfo::EndPoint::get()
    {
        auto address = Utils::GetIPAddress(this->_info->ip.address());
        return gcnew System::Net::IPEndPoint(address, this->_info->ip.port());
    }

    int PeerInfo::UpSpeed::get()
    {
        return this->_info->up_speed;
    }

    int PeerInfo::DownSpeed::get()
    {
        return this->_info->down_speed;
    }

    int PeerInfo::PayloadUpSpeed::get()
    {
        return this->_info->payload_up_speed;
    }

    int PeerInfo::PayloadDownSpeed::get()
    {
        return this->_info->payload_down_speed;
    }

    long long PeerInfo::TotalDownload::get()
    {
        return this->_info->total_download;
    }

    long long PeerInfo::TotalUpload::get()
    {
        return this->_info->total_upload;
    }

    int PeerInfo::UploadLimit::get()
    {
        return this->_info->upload_limit;
    }

    int PeerInfo::DownloadLimit::get()
    {
        return this->_info->download_limit;
    }

    System::TimeSpan^ PeerInfo::LastRequest::get()
    {
        return System::TimeSpan::FromMilliseconds(libtorrent::total_milliseconds(this->_info->last_request));
    }

    System::TimeSpan^ PeerInfo::LastActive::get()
    {
        return System::TimeSpan::FromMilliseconds(libtorrent::total_milliseconds(this->_info->last_active));
    }

    System::TimeSpan^ PeerInfo::DownloadQueueTime::get()
    {
        return System::TimeSpan::FromMilliseconds(libtorrent::total_milliseconds(this->_info->download_queue_time));
    }

    int PeerInfo::QueueBytes::get()
    {
        return this->_info->queue_bytes;
    }

    System::TimeSpan^ PeerInfo::RequestTimeout::get()
    {
        return System::TimeSpan::FromSeconds(this->_info->request_timeout);
    }

    int PeerInfo::SendBufferSize::get()
    {
        return this->_info->send_buffer_size;
    }

    int PeerInfo::UsedSendBuffer::get()
    {
        return this->_info->used_send_buffer;
    }

    int PeerInfo::ReceiveBufferSize::get()
    {
        return this->_info->receive_buffer_size;
    }

    int PeerInfo::UsedReceiveBuffer::get()
    {
        return this->_info->used_receive_buffer;
    }

    int PeerInfo::NumHashfails::get()
    {
        return this->_info->num_hashfails;
    }

    System::String^ PeerInfo::CountryCode::get()
    {
        return gcnew System::String(this->_info->country);
    }

    System::String^ PeerInfo::InetAsName::get()
    {
        return gcnew System::String(this->_info->inet_as_name.c_str());
    }

    int PeerInfo::InetAs::get()
    {
        return this->_info->inet_as;
    }

    int PeerInfo::DownloadQueueLength::get()
    {
        return this->_info->download_queue_length;
    }

    int PeerInfo::TimedOutRequests::get()
    {
        return this->_info->timed_out_requests;
    }

    int PeerInfo::BusyRequests::get()
    {
        return this->_info->busy_requests;
    }

    int PeerInfo::RequestsInBuffer::get()
    {
        return this->_info->requests_in_buffer;
    }

    int PeerInfo::TargetDownloadQueueLength::get()
    {
        return this->_info->target_dl_queue_length;
    }

    int PeerInfo::UploadQueueLength::get()
    {
        return this->_info->upload_queue_length;
    }

    int PeerInfo::FailCount::get()
    {
        return this->_info->failcount;
    }

    int PeerInfo::DownloadingPieceIndex::get()
    {
        return this->_info->downloading_piece_index;
    }

    int PeerInfo::DownloadingBlockIndex::get()
    {
        return this->_info->downloading_block_index;
    }

    int PeerInfo::DownloadingProgress::get()
    {
        return this->_info->downloading_progress;
    }

    int PeerInfo::DownloadingTotal::get()
    {
        return this->_info->downloading_total;
    }

    System::String^ PeerInfo::Client::get()
    {
        return gcnew System::String(this->_info->client.c_str());
    }

    int PeerInfo::RemoteDownloadRate::get()
    {
        return this->_info->remote_dl_rate;
    }

    int PeerInfo::PendingDiskBytes::get()
    {
        return this->_info->pending_disk_bytes;
    }

    int PeerInfo::SendQuota::get()
    {
        return this->_info->send_quota;
    }

    int PeerInfo::ReceiveQuota::get()
    {
        return this->_info->receive_quota;
    }

    int PeerInfo::RoundTripTime::get()
    {
        return this->_info->rtt;
    }

    int PeerInfo::NumPieces::get()
    {
        return this->_info->num_pieces;
    }

    int PeerInfo::DownloadRatePeak::get()
    {
        return this->_info->download_rate_peak;
    }

    int PeerInfo::UploadRatePeak::get()
    {
        return this->_info->upload_rate_peak;
    }

    float PeerInfo::Progress::get()
    {
        return this->_info->progress;
    }

    int PeerInfo::ProgressPpm::get()
    {
        return this->_info->progress_ppm;
    }

    int PeerInfo::EstimatedReciprocationRate::get()
    {
        return this->_info->estimated_reciprocation_rate;
    }

    System::Net::IPEndPoint^ PeerInfo::LocalEndPoint::get()
    {
        auto address = Utils::GetIPAddress(this->_info->local_endpoint.address());
        return gcnew System::Net::IPEndPoint(address, this->_info->local_endpoint.port());
    }
}