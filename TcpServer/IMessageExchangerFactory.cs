using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServer
{
    public interface IMessageExchangerFactory
    {
        MessageExchangerBase CreateMessageExchanger();
    }
}
