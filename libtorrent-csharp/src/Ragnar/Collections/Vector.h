#pragma once

#include "../Interop/ValueConverter.h"

#include <string>
#include <vector>

namespace Ragnar
{
    namespace Collections
    {
        template<typename T, typename U>
        ref class VectorEnumerator : System::Collections::IEnumerator, System::Collections::Generic::IEnumerator<U>
        {

        };

        template<typename T, typename U>
        ref class Vector : System::Collections::Generic::IList<U>
        {
        private:
            std::vector<T>* _vector;
            Ragnar::Interop::ValueConverter<T, U>^ _converter;

        internal:
            Vector(std::vector<T> &vector, Ragnar::Interop::ValueConverter<T, U>^ converter)
            {
                this->_vector = &vector;
                this->_converter = converter;
            }

        public:
            virtual property U default[int]
            {
                U get(int index)
                {
                    if (index < 0 || index >= this->_vector->size())
                    {
                        throw gcnew ArgumentOutOfRangeException("index");
                    }

                    return this->_converter->From(this->_vector->at(index));
                }

                void set(int index, U value)
                {
                    if (index < 0 || index >= this->_vector->size())
                    {
                        throw gcnew ArgumentOutOfRangeException("index");
                    }

                    auto val = this->_converter->To(value);
                    this->_vector->at(index) = val;
                }
            }

            virtual void Add(U item)
            {
                T val = this->_converter->To(item);
                this->_vector->push_back(val);
            }

            virtual void Clear()
            {
                this->_vector->clear();
            }

            virtual bool Contains(U item)
            {
                auto val = this->_converter->To(item);
                return std::find(this->_vector->begin(), this->_vector->end(), val) != this->_vector->end();
            }

            virtual void CopyTo(cli::array<U>^ arr, int startIndex)
            {
                for (int i = 0; i < this->_vector->size(); i++)
                {
                    arr[startIndex + i] = this->_converter->From(this->_vector->at(i));
                }
            }

            virtual property int Count
            {
                int get() { return this->_vector->size(); }
            }

            virtual System::Collections::IEnumerator^ GetEnumerator()
            {
                throw gcnew System::NotImplementedException();
            }

            virtual System::Collections::Generic::IEnumerator<U>^ GetEnumeratorGeneric() = System::Collections::Generic::IEnumerable<U>::GetEnumerator
            {
                throw gcnew System::NotImplementedException();
            }

            virtual int IndexOf(U item)
            {
                auto val = this->_converter->To(item);
                auto it = std::find(this->_vector->begin(), this->_vector->end(), val);

                if (it == this->_vector->end())
                {
                    return -1;
                }

                return std::distance(this->_vector->begin(), it);
            }

            virtual void Insert(int index, U item)
            {
                if (index < 0 || index >= this->_vector->size())
                {
                    throw gcnew ArgumentOutOfRangeException("index");
                }

                auto val = this->_converter->To(item);
                this->_vector->insert(this->_vector->begin() + index, val);
            }

            virtual property bool IsReadOnly
            {
                bool get() { return false; }
            }

            virtual bool Remove(U item)
            {
                auto val = this->_converter->To(item);
                auto end = std::remove(this->_vector->begin(), this->_vector->end(), val);
                bool removed = end != this->_vector->end();

                this->_vector->erase(end, this->_vector->end());

                return removed;
            }

            virtual void RemoveAt(int index)
            {
                this->_vector->erase(this->_vector->begin() + index);
            }
        };
    }
}
