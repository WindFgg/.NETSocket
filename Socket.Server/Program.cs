using System.Net;
using System.Net.Sockets;
using System.Text;


//1.创建socket对象
var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//2.Bind指定套接字的IP和端口
var Ip = "127.0.0.1";
var Port = 9999;
IPEndPoint port = new IPEndPoint(IPAddress.Parse(Ip), Port);
socket.Bind(port);
socket.Listen(0);
Console.WriteLine($"[服务端]启动成功,当前服务端地址:{Ip}:{Port}");

var ListSocketClinet = new Dictionary<string, Socket>(); //Socket客户端列表
var ListSocketReceiveTask = new Dictionary<string, Task>(); //Socket客户端消息回调线程列表

/// <summary>
/// 添加Socket客户端接收消息回调线程
/// </summary>
void AddSocketReceiveTask(Socket connfd)
{
    var RemoteEndPoint = (IPEndPoint)connfd.RemoteEndPoint; //获取到远程Socket的IP地址以及端口
    ListSocketClinet.Add($"{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}", connfd);

    var task = new Task(() =>
    {
        //接收缓冲区
        byte[] readBuff = new byte[1024];

        while (true)
        {
            try
            {
                //Recv 接收客户端数据 是阻塞方法
                int count = connfd.Receive(readBuff);
                string ReceiveMsg = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);

                Console.WriteLine($"[服务端接收到来自{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}的消息]" + ReceiveMsg);

                //Send 发送数据
                if (ReceiveMsg == "你好")
                {
                    var SednMessage = $"延迟一秒,你好呀,这是服务端向客户端发送的消息 {(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}";
                    connfd.Send(Encoding.Default.GetBytes(SednMessage));
                    Console.WriteLine($"[服务端发送到客户端{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}的消息]" + SednMessage);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "远程主机强迫关闭了一个现有的连接。")
                {
                    ListSocketReceiveTask.Remove($"{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}");
                    ListSocketClinet.Remove($"{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}");


                    Console.WriteLine($"监听客户端消息回调-1,当前监听客户端消息回调总数:" + ListSocketReceiveTask.Count);
                    Console.WriteLine($"客户端总数-1,当前客户端总数:" + ListSocketClinet.Count);
                    Console.WriteLine($"客户端:{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}关闭了连接");

                    break;
                }
            }
        }


    });

    ListSocketReceiveTask.Add($"{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}", task);
    Console.WriteLine($"监听客户端消息回调+1,当前监听客户端总数:" + ListSocketReceiveTask.Count + "\n");
    task.Start();
}

/// <summary>
/// 监听来自客户端的连接
/// </summary>
void ListenerSocketClinetConnect()
{
    new Task(() =>
    {
        while (true)
        {
            //Accept接收客户端连接 是阻塞方法
            Socket connfd = socket.Accept();
            var RemoteEndPoint = (IPEndPoint)connfd.RemoteEndPoint; //获取客户端IP和端口

            Console.WriteLine($"[服务端收到来自{(RemoteEndPoint.Address)}:{RemoteEndPoint.Port}的连接]");
            Console.WriteLine($"客户端总数+1,当前客户端总数:" + ListSocketClinet.Count + "\n");

            AddSocketReceiveTask(connfd); //添加到客户端消息接收列表
        }
    }).Start();
}

ListenerSocketClinetConnect();

Console.ReadLine(); //阻塞线程
