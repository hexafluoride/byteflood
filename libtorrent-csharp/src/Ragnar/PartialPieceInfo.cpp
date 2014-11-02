#include "stdafx.h"
#include "PartialPieceInfo.h"

#include <libtorrent\torrent_handle.hpp>

namespace Ragnar
{
    PartialPieceInfo::PartialPieceInfo(const libtorrent::partial_piece_info &info)
    {
        this->_info = new libtorrent::partial_piece_info(info);
    }

    PartialPieceInfo::~PartialPieceInfo()
    {
        delete this->_info;
    }

    int PartialPieceInfo::PieceIndex::get()
    {
        return this->_info->piece_index;
    }

    int PartialPieceInfo::BlocksInPiece::get()
    {
        return this->_info->blocks_in_piece;
    }

    int PartialPieceInfo::Finished::get()
    {
        return this->_info->finished;
    }

    int PartialPieceInfo::Writing::get()
    {
        return this->_info->writing;
    }

    int PartialPieceInfo::Requested::get()
    {
        return this->_info->requested;
    }


}