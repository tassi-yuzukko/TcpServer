using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    static public class NetworkStreamExtensions
    {
        // byte配列の読み込み（ビッグエンディアン）
        public static byte[] ReadReceivedData(this NetworkStream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                // 長さを読み込んでから、バイト配列を読み込む
                var length = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                return reader.ReadBytes(length);
            }
        }

        // byte配列の書き込み（ビッグエンディアン）
        public static void WriteSendingData(this NetworkStream stream, byte[] bytes)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                // 長さを書き込んでからバイト配列を書き込む
                writer.Write(IPAddress.HostToNetworkOrder(bytes.Length));
                writer.Write(bytes);
            }
        }
    }
}