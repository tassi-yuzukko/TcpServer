﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    static class NetworkStreamExtensions
    {
        // byte配列の読み込み（ビッグエンディアン対応）
        public static byte[] ReadReceivedData(this NetworkStream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                // 長さを読み込んでから、バイト配列を読み込む
                var length = reader.ReadInt32();
                if (BitConverter.IsLittleEndian)
                    length = IPAddress.NetworkToHostOrder(length);  // コンピューターがリトルエンディアンの場合は、ビッグエンディアンに変換する
                return reader.ReadBytes(length);
            }
        }

        // byte配列の書き込み（ビッグエンディアン対応）
        public static void WriteSendingData(this NetworkStream stream, byte[] bytes)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                // 長さを書き込んでからバイト配列を書き込む
                var length = bytes.Length;
                if (BitConverter.IsLittleEndian)
                    length = IPAddress.HostToNetworkOrder(length);  // コンピューターがリトルエンディアンの場合は、ビッグエンディアンに変換する
                writer.Write(length);
                writer.Write(bytes);
            }
        }
    }
}