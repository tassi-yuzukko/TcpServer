using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace TcpClientTest
{
    // クライアント
    public class Client : IDisposable
    {
        // 接続先のエンドポイント
        private readonly IPEndPoint _endpoint;
        private TcpClient client;
        private NetworkStream stream;

        public Client(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            client = new TcpClient();
        }

        // サーバにリクエストを送信してレスポンスを受信する
        public byte[] SendMessage(byte[] request)
        {
            // 2. サーバにリクエストを送信する
            Console.WriteLine($"Client send: {BytesToString(request)}");
            stream.WriteSendingData(request);

            // 3. サーバからレスポンスを受信する
            var response = stream.ReadReceivedData();
            Console.WriteLine($"Client received: {BytesToString(response)}");

            return response;
        }

        public async Task ConnectAsync()
        {
            await client.ConnectAsync(_endpoint.Address, _endpoint.Port);
            stream = client.GetStream();
        }

        public void Dispose()
        {
            client?.Dispose();
            stream?.Dispose();
        }

        string BytesToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var b in data)
            {
                sb.Append(b);
            }

            return sb.ToString();
        }
    }
}