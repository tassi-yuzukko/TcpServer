using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace TcpClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // クライアントが接続するサーバのエンドポイント
            var endpoint = new IPEndPoint(IPAddress.Loopback, 54321);

            using (var client = new Client(endpoint))
            {

                await client.ConnectAsync();

                while (true)
                {
                    Console.WriteLine("Enter Send Message. If you want to exit, press Enter.");

                    var read = Console.ReadLine();
                    if (read.Length == 0)
                    {
                        break;
                    }

                    client.SendMessage(Encoding.UTF8.GetBytes(read));
                }
            }
        }
    }
}
