using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

//创建HttpListener类，并启动监听：
var listener = new HttpListener();
listener.Prefixes.Add("http://127.0.0.1:9999/");
listener.Start();

//等待连接
Console.WriteLine($"创建WebSocket服务端端完毕,等待客户端连接...");
var context = await listener.GetContextAsync();
var RemoteEndPoint = (IPEndPoint)context.Request.RemoteEndPoint;

//接收websocket
var wsContext = await context.AcceptWebSocketAsync(null);
var ws = wsContext.WebSocket;
Console.WriteLine($"收到来自客户端[{RemoteEndPoint.Address}:{RemoteEndPoint.Port}]的连接");

//定义缓冲区
var buffer = new byte[1024];

//发送消息
var MsgByte = Encoding.UTF8.GetBytes("你好服务端");
await ws.SendAsync(MsgByte, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
Console.WriteLine($"[向服务端发送消息]:你好服务端");

//接受消息回调
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(async () =>
{
    var wsdata = await ws.ReceiveAsync(buffer, CancellationToken.None);
    Console.WriteLine(wsdata.Count);
    byte[] bRec = new byte[wsdata.Count];
    Array.Copy(buffer, bRec, wsdata.Count);

    Console.WriteLine(Encoding.UTF8.GetString(bRec));
});
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法


Console.ReadLine();