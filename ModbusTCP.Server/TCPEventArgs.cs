using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ModbusTCP.Server
{
    /// <summary>
    /// 同步TcpListener TCP服务器事件类
    /// </summary>
    public class TCPEventArgs : EventArgs
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        public string _msg;

        /// <summary>
        /// 客户端状态封装类
        /// </summary>
        public TcpClient _handle;

        /// <summary>
        /// 是否已经处理过了
        /// </summary>
        public bool IsHandled { get; set; }

        public TCPEventArgs(string msg)
        {
            this._msg = msg;
            IsHandled = false;
        }
        public TCPEventArgs(TcpClient handle)
        {
            this._handle = handle;
            IsHandled = false;
        }
        public TCPEventArgs(string msg, TcpClient handle)
        {
            this._msg = msg;
            this._handle = handle;
            IsHandled = false;
        }
    }
}
