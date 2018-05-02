using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServer
{
    public static class LogManager
    {
        static ILoggerFactory loggerFactory = Default();

        /// <summary>
        /// LoggerFactoryを設定する
        /// このメソッドをコールしてLoggerFactoryをセットしないとログが出力されないので
        /// ライブラリ使用時の一番最初にコールすること
        /// </summary>
        /// <param name="factory">ILoggerFactory</param>
        static public void SetLoggerFactory(ILoggerFactory factory)
        {
            loggerFactory = factory;
        }

        /// <summary>
        /// ログクライアントを作成する
        /// 内部で GetClassFullName() メソッドで呼び出し元クラス名を取得する
        /// </summary>
        /// <returns></returns>
        static public ILogger GetLogger<T>()
        {
            return loggerFactory.CreateLogger<T>();
        }

        static ILoggerFactory Default()
        {
            return new LoggerFactory();
        }
    }
}
