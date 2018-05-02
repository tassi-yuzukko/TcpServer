using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    static class StreamExtensions
    {
        // byte配列の読み込み
        public static byte[] ReadReceivedData(this Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                // 長さを読み込んでから、バイト配列を読み込む
                var length = reader.ReadInt32();
                return reader.ReadBytes(length);
            }
        }

        // byte配列の書き込み
        public static void WriteSendingData(this Stream stream, byte[] bytes)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                // 長さを書き込んでからバイト配列を書き込む
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }
    }
}