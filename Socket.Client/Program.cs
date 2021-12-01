using System.Net;
using System.Net.Sockets;
using System.Text;


//1.创建socket对象
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//2.创建连接
var ServerIp = "127.0.0.1";
var ServerPort = 9999;

var RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);//远程网络端点。
socket.Connect(RemoteEndPoint);
Console.WriteLine($"[连接服务端{ServerIp}:{ServerPort}]成功");

var LocalEndPoint = (IPEndPoint)socket.LocalEndPoint; //本地网络端点 注意只有连接到服务端成功LocalEndPoint才不会为NULL
Console.WriteLine($"本地端点为:{(LocalEndPoint.Address)}:{ LocalEndPoint.Port}\n");

//接收缓冲区
const int BUFFER_SIZE = 1024;
byte[] readBuff = new byte[BUFFER_SIZE];

//Send 发送消息
string SendMsg = "你好";
socket.Send(Encoding.UTF8.GetBytes(SendMsg));
Console.WriteLine($"[向服务端{ServerIp}:{ServerPort}发送消息]{SendMsg}");
Console.WriteLine(socket.ReceiveTimeout);
socket.ReceiveTimeout = 5000;
Console.WriteLine(socket.ReceiveTimeout);
/* 粘包测试 
 * 服务端定义数据缓冲区buffer为1024字节，
 * 客户端发送数据时，发送的buffer为实际发送内容的长度，当发送数据的长度小于服务端接收buffer的长度时，
 * 多个发送内容就会存放到服务端的buffer中，导致上次发送的数据和本次的发送的数据连在一起。
 * 下面注释掉的代码就会粘包 可以自己去掉注释自己测试一下是什么样的结果
*/

/*
SendMsg = "服务端定义数据缓冲区buffer为1024字节，客户端发送数据时，发送的buffer为实际发送内容的长度，当发送数据的长度小于服务端接收buffer的长度时，多个发送内容就会存放到服务端的buffer中，导致数据连在一起。解决办法有根据添加数据结束标记，在服务端拆分（不喜欢这种方式）";
socket.Send(Encoding.UTF8.GetBytes(SendMsg));
Console.WriteLine($"[向服务端{ServerIp}:{ServerPort}发送消息]{SendMsg}");
SendMsg = "你好";
socket.Send(Encoding.UTF8.GetBytes(SendMsg));
*/

new Task(() =>
{
    while (true)
    {
        // 轮询获取服务端返回信息
        try
        {
            int count = socket.Receive(readBuff);
            var ReceiveMsg = Encoding.UTF8.GetString(readBuff, 0, count);
            Console.WriteLine($"[收到服务端{ServerIp}:{ServerPort}发送的消息]{ReceiveMsg}");

            if (ReceiveMsg.Contains("延迟一秒"))
            {
                SendMsg = $"客户端收到了服务端的消息并回复服务端";
                socket.Send(Encoding.UTF8.GetBytes(SendMsg));
            }
        }
        catch (Exception ex)
        {
            if (ex.Message == "远程主机强迫关闭了一个现有的连接。")
            {
                Console.WriteLine($"服务端:{(LocalEndPoint.Address)}:{ LocalEndPoint.Port}已经离线");
                break;
            }
        }
    }
}).Start();


Console.ReadLine(); //阻塞线程







