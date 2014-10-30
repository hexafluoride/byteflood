#pragma once

namespace libtorrent
{
    class sha1_hash;
}

namespace Ragnar
{
    public ref class SHA1Hash
    {
    private:
        libtorrent::sha1_hash* _hash;

    internal:
        SHA1Hash(const libtorrent::sha1_hash &hash);

    public:
        property bool IsZero { bool get(); }

        void Clear();

        System::String^ ToHex();

        virtual System::String^ ToString() override;
    };
}
