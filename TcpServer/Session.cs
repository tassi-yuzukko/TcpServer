using Microsoft.Extensions.Logging;
using System;
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

        readonly object streamLocker = new object();

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

            this.messageExchanger.NotifyAction = Notify;

            logger.LogInformation($"Session is created. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");
        }

        // クライアントからリクエストを受信してレスポンスを送信する
        public void StartConnectionProcedure()
        {
            // 終了処理を予約しておく
            cancellationToken.Register(() =>
            {
                lock (streamLocker)
                {
                    SessionTask?.Wait();

                    stream.Dispose();
                }
            });

            SessionTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        lock (streamLocker)
                        {
                            // クライアントとの接続確認処理
                            if (client.Client.Poll(1000, SelectMode.SelectRead) && (IsAvailable() == false))
                            {
                                logger.LogInformation($"Session is disconnected. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");

                                break;
                            }

                            // 上位からの終了処理
                            if (cancellationToken.IsCancellationRequested)
                            {
                                logger.LogInformation($"Session is canceled. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");

                                break;
                            }

                            // リクエスト受信処理
                            if (IsAvailable())
                            {
                                // クライアントからリクエストを受信する
                                var request = stream.ReadReceivedData();

                                logger.LogInformation($"Request received. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}, Data:{BytesToString(request)}>");

                                // リクエストを処理してレスポンスを作る
                                var response = messageExchanger.Response(request);

                                // クライアントにレスポンスを送信する
                                stream.WriteSendingData(response);

                                logger.LogInformation($"Response sent. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}, Data:{BytesToString(response)}>");
                            }
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

        // クライアントに対してプッシュ通知する
        void Notify(byte[] data)
        {
            try
            {
                lock (streamLocker)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.LogWarning($"Notify is aborted because of cancellation requested. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");
                        return;
                    }

                    if (stream.CanWrite == false)
                    {
                        logger.LogWarning($"Notify is aborted because stream cannot write. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");
                        return;
                    }

                    stream.WriteSendingData(data);

                    logger.LogInformation($"Notify is sent. <Address:{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}, Handle:{client.Client.Handle}>");
                }
            }
            catch (Exception ex)
            {
                // ログを出力するためにすべての例外をキャッチしてます
                logger.LogError(ex, "");

                throw ex;
            }
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
    }
}
