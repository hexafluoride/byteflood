#pragma once

namespace libtorrent
{
    struct fingerprint;
}

namespace Ragnar
{
    public ref class Fingerprint
    {
    private:
        libtorrent::fingerprint* _fingerprint;

    internal:
        libtorrent::fingerprint* get_ptr() { return this->_fingerprint; }

    public:
        Fingerprint(System::String^ id, int major, int minor, int revision, int tag);

        property System::String^ Id { System::String^ get(); }

        property int Major { int get(); }

        property int Minor { int get(); }

        property int Revision { int get(); }

        property int Tag { int get(); }
    };
}
