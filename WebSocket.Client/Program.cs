using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

// 建立连接
var wsConntie = "ws://127.0.0.1:9999"; //连接WebSocket服务端的URI必须是ws://或者wss://

var ws = new ClientWebSocket();
if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
{
    return;
}

string netErr = string.Empty;
Task.Run(async () =>
{
    if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
        return;

    string netErr = string.Empty;
    try
    {
        Console.WriteLine("正在连接服务端...");
        await ws.ConnectAsync(new Uri(wsConntie), CancellationToken.None);
        Console.WriteLine("连接服务端[" + wsConntie + "]成功");

        //全部消息容器
        List<byte> bs = new List<byte>();
        //缓冲区
        var buffer = new byte[1024 * 4];
        //监听Socket信息
        WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //是否关闭
        while (!result.CloseStatus.HasValue)
        {
            //文本消息
            if (result.MessageType == WebSocketMessageType.Text)
            {
                bs.AddRange(buffer.Take(result.Count));

                //消息是否已接收完全
                if (result.EndOfMessage)
                {
                    //发送过来的消息
                    string Msg = Encoding.UTF8.GetString(bs.ToArray(), 0, bs.Count);
                    Console.WriteLine("[收到服务端消息]:" + Msg);
                    if (Msg.Contains("你好"))
                    {
                        await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine($"[客户端向服务端{wsConntie}发送消息]:" + Msg);
                    }
                }
            }
            //继续监听Socket信息
            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message.ToString());
    }
    finally
    {
        await ws.CloseAsync(ws.CloseStatus.Value, ws.CloseStatusDescription + netErr, CancellationToken.None);
        Console.WriteLine("关闭连接");
    }
});

Console.ReadLine();
