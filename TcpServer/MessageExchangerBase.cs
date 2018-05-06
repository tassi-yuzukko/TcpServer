using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace TcpServer
{
    /// <summary>
    /// メッセージ送受信の処理クラス
    /// </summary>
    public abstract class MessageExchangerBase
    {
        /// <summary>
        /// 通知処理を行うためにオブザーバーパターンを実現するために使用する
        /// </summary>
        Subject<byte[]> subject = new Subject<byte[]>();

        /// <summary>
        /// リクエストレスポンス処理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        abstract public byte[] Response(byte[] request);

        /// <summary>
        /// 通知処理の登録
        /// </summary>
        /// <param name="notifyAction"></param>
        public void Subscribe(Action<byte[]> notifyAction) => subject.Subscribe(notifyAction);

        /// <summary>
        /// 通知処理（プッシュ通知）
        /// </summary>
        public void Notify(byte[] notifyData)
        {
            subject.OnNext(notifyData);
        }
    }
}
