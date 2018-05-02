using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TcpServer
{
    // サーバ
    class Listener
    {
        static ILogger logger = LogManager.GetLogger<Listener>();

        // 接続を待つエンドポイント
        readonly IPEndPoint endpoint;

        // リクエストからレスポンスを作成するクラスのファクトリ
        readonly IMessageExchangerFactory requestResponserFactory;

        // TCPリスナー
        readonly TcpListener listener;

        readonly IList<Session> sessions = new List<Session>();

        readonly CancellationToken cancellationToken;

        public Task ListenerTask { get; private set; }

        public Listener(
            IPEndPoint endpoint,
            IMessageExchangerFactory requestResponserFactory,
            CancellationToken cancellationToken)
        {
            this.endpoint = endpoint;
            this.requestResponserFactory = requestResponserFactory;
            this.cancellationToken = cancellationToken;

            listener = new TcpListener(this.endpoint);
        }

        // 接続待ちを開始
        public void StartListening()
        {
            // クライアントからの接続を待つ
            listener.Start();

            // 終了処理を予約しておく
            cancellationToken.Register(() =>
            {
                ListenerTask?.Wait();

                listener.Stop();
            });

            logger.LogInformation($"Listener started listening. <Address:{endpoint.Address}, Port:{endpoint.Port}>");

            ListenerTask = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        // クライアントからの接続を受け入れる
                        if (listener.Pending())
                        {
                            var client = await Task.Run(
                                () => listener.AcceptTcpClientAsync(),
                                cancellationToken);

                            var session = new Session(
                                client,
                                requestResponserFactory.CreateMessageExchanger(),
                                cancellationToken);

                            session.StartConnectionProcedure();

                            sessions.Add(session);

                            logger.LogInformation($"Listener accepted and added new session. <new session task id:{session.SessionTask.Id}, session list size:{sessions.Count()}>");
                        }

                        // 終了済みのセッションを削除する
                        foreach (var session in sessions.ToArray())
                        {
                            if (session.SessionTask.IsCompleted)
                            {
                                sessions.Remove(session);
                                logger.LogInformation($"Session completed. <completed session task id:{session.SessionTask.Id},session list size:{sessions.Count()}>");
                            }
                        }

                        // 上位からの終了処理
                        if (cancellationToken.IsCancellationRequested)
                        {
                            logger.LogInformation($"Listener is canceled. <Address:{endpoint.Address}, Port:{endpoint.Port}>");
                            break;
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
    }
}