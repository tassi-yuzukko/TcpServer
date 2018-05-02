using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServer
{
    /// <summary>
    /// メッセージ送受信の処理クラス
    /// </summary>
    public abstract class MessageExchangerBase
    {
        internal Action<byte[]> NotifyAction { private get; set; }

        /// <summary>
        /// リクエストレスポンス処理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        abstract public byte[] Response(byte[] request);

        /// <summary>
        /// 通知処理（プッシュ通知）
        /// ※技術的な通知処理は、<see cref="NotifyAction"/>に委譲する
        /// </summary>
        public void Notify(byte[] notifyData)
        {
            NotifyAction?.Invoke(notifyData);
        }
    }
}
