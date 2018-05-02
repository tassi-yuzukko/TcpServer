using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TcpServer;

namespace TcpServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // ロガーファクトリーを追加
            LogManager.SetLoggerFactory(new LoggerFactory().AddConsole().AddDebug());

            // サーバが接続を待つエンドポイント
            var endpoint = new IPEndPoint(IPAddress.Any, 54321);

            // サーバ
            using (var server = new Server())
            {
                server.AddListener(endpoint, new MessageExchangerFactoryConcrete());

                Console.WriteLine("終了する場合は何かキーを押してください...");
                Console.ReadLine();
            }

            Console.WriteLine("Program ended.");
        }
    }
}
