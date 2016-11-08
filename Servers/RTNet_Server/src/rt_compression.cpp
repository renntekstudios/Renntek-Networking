#include "rt_compression.h"

using namespace RTNet;

rt_byte* RTCompression::Compress(rt_byte* input, int length, int* outLength)
{
    *outLength = length;
    return input;
}

rt_byte* RTCompression::Decompress(rt_byte* input, int length, int* outLength)
{
    *outLength = length;
    return input;   
}
