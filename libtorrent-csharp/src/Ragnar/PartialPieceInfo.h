#pragma once

namespace libtorrent
{
    struct partial_piece_info;
}

namespace Ragnar
{
    public ref class PartialPieceInfo
    {
    private:
        libtorrent::partial_piece_info* _info;

    internal:
        PartialPieceInfo(const libtorrent::partial_piece_info &info);

    public:
        ~PartialPieceInfo();

        property int PieceIndex { int get(); }

        property int BlocksInPiece { int get(); }

        property int Finished { int get(); }

        property int Writing { int get(); }

        property int Requested { int get(); }

        // TODO: block_info* blocks;
        // TODO: state_t piece_state;
    };
}

