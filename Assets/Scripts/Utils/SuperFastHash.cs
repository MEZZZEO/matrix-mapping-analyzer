using System;

namespace Utils
{
    /// <summary>
    /// Реализация алгоритма SuperFastHash от Paul Hsieh
    /// Используется для быстрого хеширования данных матриц
    /// </summary>
    public static class SuperFastHash
    {
        public static UInt32 Hash(Byte[] dataToHash)
        {
            Int32 dataLength = dataToHash.Length;
            if (dataLength == 0)
                return 0;
            
            UInt32 hash = (UInt32)dataLength;
            Int32 remainingBytes = dataLength & 3; // mod 4
            Int32 numberOfLoops = dataLength >> 2; // div 4
            Int32 currentIndex = 0;
            
            while (numberOfLoops > 0)
            {
                hash += (UInt16)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8);
                UInt32 tmp = (UInt32)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8) << 11 ^ hash;
                hash = (hash << 16) ^ tmp;
                hash += hash >> 11;
                numberOfLoops--;
            }

            switch (remainingBytes)
            {
                case 3:
                    hash += (UInt16)(dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8);
                    hash ^= hash << 16;
                    hash ^= ((UInt32)dataToHash[currentIndex]) << 18;
                    hash += hash >> 11;
                    break;
                case 2:
                    hash += (UInt16)(dataToHash[currentIndex++] | dataToHash[currentIndex] << 8);
                    hash ^= hash << 11;
                    hash += hash >> 17;
                    break;
                case 1:
                    hash += dataToHash[currentIndex];
                    hash ^= hash << 10;
                    hash += hash >> 1;
                    break;
            }

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }
    }
}