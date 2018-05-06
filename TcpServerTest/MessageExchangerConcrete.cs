using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpServer;

namespace TcpServerTest
{
    class MessageExchangerConcrete : MessageExchangerBase
    {
        int size = 1;

        public MessageExchangerConcrete()
        {
            //Task.Run(() =>
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        Thread.Sleep(1000);

            //        Notify(NotifyProc());
            //    }
            //});
        }

        override public byte[] Response(byte[] request)
        {
            return request.Reverse().ToArray();
        }

        byte[] NotifyProc()
        {
            var bytes = new byte[size];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)i;
            }

            size++;

            return bytes;
        }
    }
}
