using System;
using System.Collections.Generic;
using System.Text;
using TcpServer;

namespace TcpServerTest
{
    class MessageExchangerFactoryConcrete : IMessageExchangerFactory
    {
        public MessageExchangerBase CreateMessageExchanger()
        {
            return new MessageExchangerConcrete();
        }
    }
}
