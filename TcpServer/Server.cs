using System;
using System.Collections.Concurrent;
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
    public class Server : IDisposable
    {
        static ILogger logger = LogManager.GetLogger<Server>();

        readonly CancellationTokenSource tokenSource;
        readonly IList<Listener> listeners;

        public Server()
        {
            tokenSource = new CancellationTokenSource();
            listeners = new List<Listener>();
        }

        public void AddListener(
            IPEndPoint endpoint,
            IMessageExchangerFactory messageExchangerFactory)
        {
            var listener = new Listener(
                endpoint,
                messageExchangerFactory,
                tokenSource.Token);

            listener.StartListening();

            listeners.Add(listener);

            logger.LogInformation($"Server added new Listener. <Address:{endpoint.Address}, Port:{endpoint.Port}>");
        }

        public void Dispose()
        {
            tokenSource.Cancel();

            logger.LogInformation($"Server stopping ...");

            try
            {
                Task.WaitAll(
                    listeners.Select(i => i.ListenerTask).Where(i => i != null).ToArray());
            }
            finally
            {
                tokenSource.Dispose();
            }

            logger.LogInformation($"Server just stopped ...");
        }
    }
}