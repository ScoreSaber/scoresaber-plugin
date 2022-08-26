using System;
using System.IO;
using SevenZip;
using Decoder = SevenZip.Compression.LZMA.Decoder;
using Encoder = SevenZip.Compression.LZMA.Encoder;

namespace ScoreSaber.Utilities
{
    internal static class SevenZipExtensions
    {
        const int Dictionary = 1 << 23;
        const bool Eos = false;
        static readonly CoderPropID[] propIDs =
                {
                    CoderPropID.DictionarySize,
                    CoderPropID.PosStateBits,
                    CoderPropID.LitContextBits,
                    CoderPropID.LitPosBits,
                    CoderPropID.Algorithm,
                    CoderPropID.NumFastBytes,
                    CoderPropID.MatchFinder,
                    CoderPropID.EndMarker
                };
        static readonly object[] properties =
                {
                    Dictionary,
                    2,
                    3,
                    0,
                    2,
                    128,
                    "bt4",
                    Eos
                };

        public static byte[] Compress(this byte[] inputBytes)
        {
            using MemoryStream inStream = new(inputBytes);
            using MemoryStream outStream = new();
            Encoder encoder = new();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            long fileSize = inStream.Length;
            for (int i = 0; i < 8; i++)
                outStream.WriteByte((byte)(fileSize >> (8 * i)));
            encoder.Code(inStream, outStream, -1, -1, null);

            return outStream.ToArray();
        }

        public static byte[] Decompress(this byte[] inputBytes)
        {
            using MemoryStream newInStream = new(inputBytes);

            Decoder decoder = new();

            newInStream.Seek(0, 0);
            using MemoryStream newOutStream = new();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            byte[] b = newOutStream.ToArray();
            return b;
        }
    }
}
