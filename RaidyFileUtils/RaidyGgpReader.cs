using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RaidyFileUtils
{
    struct GgpHeader
    {
        public char[] magic; // Always "GGPFAIKE".
        public uint unknown1;
        public ulong key;
        public uint dataOffset;
        public uint dataLength;
    }

    public class RaidyGgpReader : IDisposable
    {
        BinaryReader reader;
        GgpHeader header;

        public RaidyGgpReader(Stream source)
        {
            reader = new BinaryReader(source);

            header.magic = reader.ReadChars(8);
            header.unknown1 = reader.ReadUInt32();
            header.key = reader.ReadUInt64();
            header.dataOffset = reader.ReadUInt32();
            header.dataLength = reader.ReadUInt32();
        }

        public void ExtractImage(string outputFileName)
        {
            byte[] pngHeaderBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            byte[] key = new byte[8];

            reader.BaseStream.Position = header.dataOffset;

            byte[] sample = reader.ReadBytes(8);
            for (int i = 0; i < sample.Length; i++)
            {
                key[i] = (byte)(sample[i] ^ pngHeaderBytes[i]);
            }

            reader.BaseStream.Position = header.dataOffset;

            using (var writer = new BinaryWriter(new FileStream(outputFileName, FileMode.Create, FileAccess.Write)))
            {
                long position = 0;
                for (long i = 0; i < header.dataLength; i++)
                {
                    writer.Write((byte)(reader.ReadByte() ^ key[position++ % 8]));
                }
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
