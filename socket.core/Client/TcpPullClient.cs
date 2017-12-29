﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace socket.core.Client
{
    /// <summary>
    /// pull 组件接收到数据时会触发监听事件对象OnReceive(pSender,dwConnID,iTotalLength)，告诉应用程序当前已经接收到了多少数据，应用程序检查数据的长度，如果满足则调用组件的Fetch(dwConnID,pData,iDataLength)方法，把需要的数据“拉”出来
    /// </summary>
    public class TcpPullClient
    {
        /// <summary>
        /// 基础类
        /// </summary>
        private TcpClients tcpClients;
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event Action<bool> OnAccept;
        /// <summary>
        /// 接收通知事件
        /// </summary>
        public event Action<int> OnReceive;
        /// <summary>
        /// 断开连接通知事件
        /// </summary>
        public event Action OnClose;
        /// <summary>
        /// 接收到的数据缓存
        /// </summary>
        private  List<byte> queue;

        /// <summary>
        /// 设置基本配置
        /// </summary>   
        /// <param name="receiveBufferSize">用于每个套接字I/O操作的缓冲区大小(接收端)</param>
        public TcpPullClient(int receiveBufferSize)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                queue = new List<byte>();
                tcpClients = new TcpClients( receiveBufferSize);
                tcpClients.OnAccept += TcpServer_eventactionAccept;
                tcpClients.OnReceive += TcpServer_eventactionReceive;
                tcpClients.OnClose += TcpServer_eventClose;
            }));
            thread.Start();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip">ip地址或域名</param>
        /// <param name="port">端口</param>
        public void Connect(string ip,int port)
        {
            while (tcpClients == null)
            {
                Thread.Sleep(2);
            }
            tcpClients.Connect(ip,port);
        }

        /// <summary>
        /// 连接成功事件方法
        /// </summary>
        /// <param name="success">是否成功连接</param>
        private void TcpServer_eventactionAccept(bool success)
        {
            if (OnAccept != null)
                OnAccept(success);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">偏移位</param>
        /// <param name="length">长度</param>
        public void Send( byte[] data, int offset, int length)
        {
            tcpClients.Send(data, offset, length);
        }

        /// <summary>
        /// 接收通知事件方法
        /// </summary>
        /// <param name="data"></param>
        private void TcpServer_eventactionReceive(byte[] data)
        {
            if (OnReceive != null)
            {                
                queue.AddRange(data);
                OnReceive(queue.Count);
            }
        }

        /// <summary>
        /// 获取已经接收到的长度
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return queue.Count;
        }

        /// <summary>
        /// 取出指定长度数据
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Fetch(int length)
        {            
            if (length > queue.Count)
            {
                length = queue.Count;
            }
            byte[] f = queue.Take(length).ToArray();
            queue.RemoveRange(0, length);
            return f;
        }        

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Close()
        {
            tcpClients.Close();
        }

        /// <summary>
        /// 断开连接通知事件方法
        /// </summary>
        private void TcpServer_eventClose()
        {
            queue.Clear();
            if (OnClose != null)
                OnClose();
        }

       
        
    }
}
