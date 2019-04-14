using System;

namespace Aika_Packet_Sniffer.Logger
{
    public class Decryption
    {
        public byte[] Data { get; }
        public int Size { get; }
        
        public Decryption(byte[] data)
        {
            Data = data;
            Size = Data.Length;
        }
        
        public bool Decrypt()
        {
            var p = 4;
            int pos = _encDecKeys[(Data[3] & 0xFF) * 2];
            var sum1 = 0u;
            var sum2 = 0u;
            var i = 1;
            while (i < (Size >> 2))
            {
                uint key = _encDecKeys[(pos & 0xFF) * 2 + 1];
                var buffer = BitConverter.ToUInt32(Data, p);
                sum1 += buffer;
                switch (i & 3)
                {
                    case 0:
                        buffer -= 4 * key;
                        break;
                    case 1:
                        buffer += key >> 1;
                        break;
                    case 2:
                        buffer -= 2 * key;
                        break;
                    case 3:
                        buffer += key >> 2;
                        break;
                }

                Data[p] = (byte) ((buffer >> 00) & 0xFF);
                Data[p + 1] = (byte) ((buffer >> 08) & 0xFF);
                Data[p + 2] = (byte) ((buffer >> 16) & 0xFF);
                Data[p + 3] = (byte) ((buffer >> 24) & 0xFF);

                sum2 += buffer;
                i++;
                pos++;
                p += 4;
            }

            sum1 &= 0xFF;
            sum2 &= 0xFF;
            
            return Data[2] == ((sum1 - sum2) & 0xFF);
        }
        
        private readonly byte[] _encDecKeys =
        {
            0x52, 0xB3, 0xF7, 0x20, 0x34, 0xFB, 0x8E, 0x4C, 0x21, 0x2A, 0x64, 0xE9, 0x2F, 0x89,
            0xE7, 0xB9, 0xF1, 0xF2, 0xDF, 0x79, 0xF9, 0x40, 0x09, 0xFF, 0x9A, 0x04, 0x02, 0x85,
            0xF3, 0x7A, 0x67, 0x45, 0x28, 0xBB, 0x3B, 0x46, 0xF3, 0x33, 0x89, 0x75, 0x5C, 0x56,
            0x4A, 0xF7, 0xC4, 0x8F, 0x88, 0xBE, 0x28, 0x79, 0xC4, 0x5A, 0xA4, 0x52, 0x96, 0x12,
            0x36, 0xAD, 0x93, 0x34, 0xC5, 0x66, 0x73, 0xA9, 0x62, 0xDB, 0x74, 0xCC, 0xCD, 0x59,
            0xF9, 0x7D, 0x3B, 0xD5, 0x77, 0x70, 0x56, 0xDD, 0x91, 0xCB, 0x86, 0xCE, 0x82, 0xF1,
            0x6F, 0x46, 0xBC, 0x98, 0xBB, 0xDC, 0xCE, 0x22, 0x19, 0x0F, 0x8B, 0x2A, 0x85, 0x7D,
            0x69, 0x5D, 0xCB, 0x56, 0x27, 0x8B, 0x46, 0x0F, 0xB3, 0xFD, 0xEE, 0x5B, 0x4A, 0x0A,
            0x92, 0x57, 0xE1, 0xE5, 0x61, 0x04, 0xC4, 0xB9, 0xAF, 0xFB, 0x7D, 0xF8, 0xF6, 0x5C,
            0xF7, 0xF0, 0x1B, 0x08, 0xE3, 0x9F, 0xF3, 0x10, 0x5B, 0xC8, 0x06, 0x6D, 0xC5, 0x46,
            0x93, 0xF1, 0xFB, 0xA1, 0xD4, 0x7D, 0xA9, 0xDF, 0x82, 0x75, 0xF6, 0x9C, 0x9C, 0x71,
            0x66, 0x5D, 0x65, 0x35, 0xFE, 0x22, 0xAC, 0xE4, 0xAA, 0x3A, 0x4F, 0x70, 0xDD, 0x5B,
            0x02, 0x55, 0x77, 0xF2, 0x4D, 0x87, 0xEB, 0xB8, 0xD4, 0xA8, 0xA1, 0x86, 0xDB, 0x7F,
            0x99, 0x6A, 0x09, 0xA6, 0x52, 0xFA, 0x6C, 0x82, 0xEA, 0xE8, 0xBE, 0x78, 0x86, 0xD7,
            0xE7, 0x5E, 0xF4, 0x6D, 0xC2, 0x30, 0x8F, 0xAA, 0x24, 0x05, 0x63, 0x78, 0x1B, 0x41,
            0x92, 0x83, 0x73, 0x0B, 0xF7, 0x4A, 0x7F, 0x02, 0x08, 0x77, 0x16, 0x2B, 0x01, 0x6B,
            0xDB, 0x2E, 0x3F, 0x1E, 0xC1, 0xC3, 0xE9, 0x26, 0xCF, 0x67, 0xD6, 0x15, 0x21, 0x53,
            0xAB, 0x08, 0x30, 0xAE, 0x44, 0x7D, 0x53, 0x02, 0x56, 0x65, 0x85, 0xED, 0x52, 0x7B,
            0x68, 0x19, 0x8C, 0xD3, 0x8A, 0x6D, 0x9C, 0xB6, 0xE7, 0x85, 0x04, 0xAD, 0xB0, 0x60,
            0x14, 0xDC, 0x4B, 0x59, 0x0B, 0x91, 0x9B, 0x59, 0x7F, 0x1D, 0x81, 0x4A, 0xFF, 0xE3,
            0xA3, 0xCF, 0xF6, 0xAE, 0x6C, 0x32, 0xD2, 0x48, 0x54, 0x9E, 0x66, 0x48, 0x61, 0x8E,
            0x8D, 0x2B, 0xED, 0x85, 0x11, 0xA6, 0xAB, 0x00, 0xCB, 0x3B, 0xE5, 0xA8, 0x07, 0xCD,
            0x3A, 0xEB, 0x61, 0x10, 0xBD, 0xB9, 0x29, 0x5F, 0x1D, 0xF1, 0xBF, 0x27, 0x65, 0x7B,
            0x35, 0xC4, 0xCC, 0xC7, 0x0F, 0x3D, 0x94, 0x1C, 0x47, 0x2E, 0x32, 0x2D, 0x95, 0x05,
            0xAF, 0xEE, 0xED, 0x71, 0x4E, 0xA5, 0x48, 0x18, 0x6F, 0x44, 0xA7, 0x89, 0xB3, 0xF6,
            0x55, 0x71, 0x61, 0xF8, 0x6D, 0x11, 0x09, 0xAA, 0x9D, 0xEF, 0x67, 0xE6, 0x29, 0xCC,
            0x89, 0x90, 0x33, 0xD7, 0x34, 0x6E, 0x39, 0x20, 0x85, 0x3A, 0xDF, 0x4F, 0xD4, 0xF6,
            0xEF, 0x96, 0xDD, 0x80, 0x9E, 0xE4, 0x22, 0x66, 0x11, 0x5C, 0x8B, 0xFB, 0x1F, 0x05,
            0x50, 0xAB, 0x59, 0xC3, 0x18, 0x8B, 0x47, 0x86, 0x63, 0x34, 0xF5, 0xC1, 0x25, 0xD2,
            0xAE, 0x1E, 0xB3, 0x78, 0x08, 0x70, 0xE3, 0xB7, 0x21, 0xE8, 0x6F, 0x6E, 0x27, 0x8D,
            0x9B, 0xE3, 0x1E, 0xE6, 0x18, 0x13, 0xDD, 0xF9, 0x27, 0x47, 0x5A, 0x7A, 0x02, 0xE8,
            0x28, 0x3C, 0x77, 0x94, 0x3E, 0xEB, 0xD6, 0x71, 0xFA, 0xFD, 0x0D, 0xC2, 0x66, 0xE6,
            0x12, 0xB8, 0xB9, 0x8B, 0x81, 0x8A, 0x21, 0xFA, 0x87, 0xC8, 0xBF, 0x58, 0xFE, 0xEC,
            0xF3, 0x1A, 0xD9, 0x32, 0xDB, 0x79, 0xC3, 0xA9, 0x16, 0x1F, 0x03, 0x8A, 0xCE, 0x27,
            0xA3, 0xC9, 0xF5, 0x44, 0xD1, 0xEB, 0xCE, 0x40, 0x85, 0x17, 0xB0, 0xA9, 0x64, 0x6F,
            0x07, 0xC7, 0xE5, 0xA1, 0x9B, 0xD0, 0xB2, 0xB8, 0x15, 0x5F, 0x51, 0x39, 0xBF, 0x23,
            0x03, 0x6B, 0x8B, 0xD4, 0xEC, 0xF6, 0x57, 0x6C
        };
    }
}