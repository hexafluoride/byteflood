#pragma once

namespace Ragnar
{
    namespace Interop
    {
        template<typename T, typename U>
        public ref class ValueConverter abstract
        {
        public:
            virtual T To(U value) abstract;

            virtual U From(const T &value) abstract;
        };
    }
}

