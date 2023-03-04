using System.IO.Compression;

namespace Epsz
{
    sealed class Entry
    {
        public DateTimeOffset TimeStamp;
        public byte[] Body;

        public static Entry FromStream(Stream stream)
        {
            return new Entry(stream);
        }

        private Entry(Stream stream)
        {
            var reader = new BinaryReader(stream);
            TimeStamp = ReadTimeStamp(reader);
            var _magic = reader.ReadChars(4); // ePSZ
            Body = ReadBody(reader);
        }

        private static DateTimeOffset ReadTimeStamp(BinaryReader reader)
        {
            return DateTimeOffset.FromUnixTimeSeconds(reader.ReadUInt32());
        }

        private static byte[] ReadBody(BinaryReader reader)
        {
            byte[] block;
            var dataStream = new MemoryStream();
            do
            {
                block = ReadBlock(reader);
                dataStream.Write(block);
            }
            while (block.Length > 0);

            return dataStream.ToArray();
        }

        private static byte[] ReadBlock(BinaryReader reader)
        {
            var blockLength = reader.ReadUInt32();
            if (blockLength == 0xfffffffc)
            {
                return Array.Empty<byte>();
            }
            var zlibBlock = reader.ReadBytes((int)blockLength);

            return ZlibInflate(zlibBlock);
        }

        private static byte[] ZlibInflate(byte[] zlibData)
        {
            var dataStream = new MemoryStream();
            using (var zlibStream = new MemoryStream(zlibData))
            using (var zs = new ZLibStream(zlibStream, CompressionMode.Decompress))
            zs.CopyTo(dataStream);

            return dataStream.ToArray();
        }
    }
}
