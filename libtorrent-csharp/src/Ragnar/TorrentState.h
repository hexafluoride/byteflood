#pragma once

namespace Ragnar
{
    public enum class TorrentState : int
    {
        QueuedForChecking = 0,
        CheckingFiles = 1,
        DownloadingMetadata = 2,
        Downloading = 3,
        Finished = 4,
        Seeding = 5,
        Allocating = 6,
        CheckingResumeData = 7
    };
}
