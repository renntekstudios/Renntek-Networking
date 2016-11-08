#ifndef _RT_COMPRESSION_H_
#define _RT_COMPRESSION_H_
#include "enums.h"

namespace RTNet
{
    class RTCompression
    {
    public:
        static rt_byte* Compress(rt_byte* input, int length);
        static rt_byte* Decompress(rt_byte* input, int length, int* outLength);
    };
}
#endif