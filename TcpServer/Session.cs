using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    class Session
    {
        static ILogger logger = LogManager.GetLogger<Session>();

        readonly TcpClient client;
        readonly NetworkStream stream;

        // リクエストからレスポンスを作成する処理
        readonly MessageExchangerBase messageExchanger;

        readonly CancellationToken cancellationToken;

        readonly ConcurrentQueue<byte[]> concurrentQueue = new ConcurrentQueue<byte[]>();

        public Task SessionTask { get; private set; }

        public Session(
            TcpClient client,
            MessageExchangerBase messageExchanger,
            CancellationToken cancellationToken)
        {
            this.client = client;
            this.messageExchanger = messageExchanger;
            this.cancellationToken = cancellationToken;

            stream = client.GetStream();

            // クライアントに対してプッシュ通知するために、キューに入れて予約しておく
            this.messageExchanger.NotifyAction = (data => concurrentQueue.Enqueue(data));

            logger.LogInformation($"Session is created. {GetSessionInfomation()}");
        }

        // クライアントからリクエストを受信してレスポンスを送信する
        public void StartConnectionProcedure()
        {
            // 終了処理を予約しておく+

            cancellationToken.Register(() =>
            {
                SessionTask?.Wait();

                stream.Dispose();
            });

            SessionTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        // クライアントとの接続確認処理
                        if (client.Client.Poll(1000, SelectMode.SelectRead) && (IsAvailable() == false))
                        {
                            logger.LogInformation($"Session is disconnected. {GetSessionInfomation()}");

                            break;
                        }

                        // 上位からの終了処理
                        if (cancellationToken.IsCancellationRequested)
                        {
                            logger.LogInformation($"Session is canceled. {GetSessionInfomation()}");

                            break;
                        }

                        // リクエスト受信処理
                        if (IsAvailable())
                        {
                            // クライアントからリクエストを受信する
                            var request = stream.ReadReceivedData();

                            logger.LogInformation($"Request received. {GetSessionInfomation()} <Data:{BytesToString(request)}>");

                            // リクエストを処理してレスポンスを作る
                            var response = messageExchanger.Response(request);

                            // クライアントにレスポンスを送信する
                            stream.WriteSendingData(response);

                            logger.LogInformation($"Response sent. {GetSessionInfomation()} <Data:{BytesToString(response)}>");
                        }

                        // クライアントに対してプッシュ通知する
                        while (concurrentQueue.IsEmpty == false)
                        {
                            if (concurrentQueue.TryDequeue(out byte[] notifyData) == false)
                            {
                                // キューから取り出し失敗した場合は、いったんループ抜けて、次のループで再トライする
                                logger.LogWarning($"TryDequeue failed. {GetSessionInfomation()}");
                                break;
                            }
                            stream.WriteSendingData(notifyData);

                            logger.LogInformation($"Notify sent. {GetSessionInfomation()}");
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    // ログを出力するためにすべての例外をキャッチしてます
                    logger.LogError(ex, "");

                    throw ex;
                }
            });
        }

        bool IsAvailable()
        {
            return client?.Available > 0;
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

        string GetSessionInfomation()
        {
            return "<Address:{ ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle: { client.Client.Handle}>";
        }
    }
}
