﻿using System;
using System.IO;

namespace RaidyFileUtils
{
    // File header.
    struct RaidyHeader
    {
        public char[] magic; // 8 chars. Always "SM2MPX10".
        public uint fileCount;
        public uint dataOffset;
        public char[] fileName; // Always the name of the actual file. Perhaps an identifier internally?
        public uint tocOffset;
    }

    /// <summary>
    ///     Defines the structure of each entry in the table of contents.
    /// </summary>
    public struct RaidyFileEntry
    {
        /// <summary>
        ///     The 12-character name of the file.
        /// </summary>
        /// <remarks>
        ///     When converted to a string, do not forget to trim trailing null characters (0).
        /// </remarks>
        public char[] fileName; // The file name has a maximum of 12 chars. 
        /// <summary>
        ///     The offset of the data in the archive.
        /// </summary>
        public uint offset;
        /// <summary>
        ///     The length of the data.
        /// </summary>
        public uint length;
    }

    /// <summary>
    ///     Reader for Lightning Warrior Raidy asset packages.
    /// </summary>
    public class RaidyReader : IDisposable
    {
        readonly BinaryReader reader;
        readonly RaidyHeader header;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RaidyReader"/> class with the specified stream.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        public RaidyReader(Stream source)
        {
            reader = new BinaryReader(source);

            // Read the header.
            header.magic = reader.ReadChars(8); // Should be SM2MPX10.
            // TODO: Handle invalid files.

            header.fileCount = reader.ReadUInt32();
            header.dataOffset = reader.ReadUInt32();
            header.fileName = reader.ReadChars(12); // Always 12 chars.
            header.tocOffset = reader.ReadUInt32();

            // Populate the FileEntries array.
            FileEntries = new RaidyFileEntry[header.fileCount];
            for (int i = 0; i < header.fileCount; i++)
            {
                FileEntries[i].fileName = reader.ReadChars(12); // Always 12 chars.
                FileEntries[i].offset = reader.ReadUInt32();
                FileEntries[i].length = reader.ReadUInt32();
            }
        }

        /// <summary>
        ///     The list of entries in the archive.
        /// </summary>
        public RaidyFileEntry[] FileEntries { get; }
        /// <summary>
        ///     Returns a stream of bytes of the specified entry in the archive.
        /// </summary>
        /// <param name="fileEntry">The file entry to read.</param>
        /// <returns>
        ///     A <see cref="MemoryStream"/> containing the data of a file selected by <paramref name="fileEntry"/>.
        /// </returns>
        public Stream GetFile(RaidyFileEntry fileEntry)
        {
            MemoryStream stream = new MemoryStream();

            reader.BaseStream.Position = fileEntry.offset;

            byte[] buffer = new byte[81920];
            for (int remaining = (int)fileEntry.length; remaining > 0;)
            {
                int read = reader.Read(buffer, 0, Math.Min(buffer.Length, remaining));
                stream.Write(buffer, 0, read);
                remaining -= read;
            }

            stream.Position = 0; // Reset the stream before returning it.

            return stream;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}